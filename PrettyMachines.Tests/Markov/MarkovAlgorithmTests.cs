using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Markov;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.Markov;

public class MarkovAlgorithmTests(ITestOutputHelper output)
{
	#region Constructor Tests

    [Fact]
    public void CopyConstructor_CreatesDeepCopy()
    {
        var original = CreateTestAlgorithm();
        
        var copy = new MarkovAlgorithm(original);
        
        copy.Should().NotBeSameAs(original);
        copy.Name.Should().Be(original.Name);
        copy.Rules.Should().NotBeSameAs(original.Rules);
        copy.Rules.Should().BeEquivalentTo(original.Rules);
        copy.Alphabet.Should().BeEquivalentTo(original.Alphabet);
        copy.Markers.Should().BeEquivalentTo(original.Markers);
    }

	#endregion

	#region FindMatchingRule Tests

    [Theory]
    [InlineData("", -1)]
    [InlineData("abc", 0)]
    [InlineData("bac", 0)]
    [InlineData("cbc", 1)]
    [InlineData("xyz", -1)]
    public void FindMatchingRule_ReturnsFirstIndex_Or_Negative(string text, int index)
    {
        var algorithm = CreateTestAlgorithm();
        algorithm.FindMatchingRule(text).Should()
            .Be(index, index == -1 ? "no matches" : $"matches '{algorithm.Rules[index]}'");
    }

	#endregion

	#region GetRuleComment Tests

    [Fact]
    public void GetRuleComment_WithComment_ReturnsComment()
    {
        var builder = MarkovAlgorithm.Create();
        builder.AddRule("a", "b").WithComment("Test comment");
        var algorithm = builder.Build();
        
        var comment = algorithm.GetRuleComment(0);
        
        comment.Should().Be("Test comment");
    }

    [Fact]
    public void GetRuleComment_WithoutComment_ReturnsNull()
    {
        var builder = MarkovAlgorithm.Create();
        builder.AddRule("a", "b");
        var algorithm = builder.Build();
        
        var comment = algorithm.GetRuleComment(0);
        
        comment.Should().BeNull();
    }

    [Fact]
    public void GetRuleComment_WithInvalidIndex_ReturnsNull()
    {
        var builder = MarkovAlgorithm.Create();
        builder.AddRule("a", "b");
        var algorithm = builder.Build();
        
        var comment = algorithm.GetRuleComment(99);
        
        comment.Should().BeNull();
    }

	#endregion

	#region NextStep Tests

    [Fact]
    public void NextStep_WithMatchingRule_AppliesRuleAndReturnsMatchedRule()
    {
        var algorithm = CreateSimpleReplacementAlgorithm();
        const string input = "abc";
        
        var result = algorithm.NextStep(input, out var matchedRule);
        
        result.Should().Be("xbc");
        matchedRule.Should().NotBeNull();
        matchedRule.Pattern.Should().Be("a");
    }

    [Fact]
    public void NextStep_WithNoMatchingRule_ReturnsSameStringAndNullRule()
    {
        var algorithm = CreateSimpleReplacementAlgorithm();
        const string input = "xyz";
        
        var result = algorithm.NextStep(input, out var matchedRule);
        
        result.Should().Be(input);
        matchedRule.Should().BeNull();
    }
    
	#endregion

	#region Execute Tests

    [Fact]
    public void Execute_WithValidInput_ReturnsSuccessResult()
    {
        var algorithm = CreateSimpleReplacementAlgorithm();
        
        var result = algorithm.Execute("abc", AlgorithmCancellation.Default);
        
        result.Termination.Should().Be(TerminationStatus.Success);
        result.Output.Should().Be("xbc");
        result.Steps.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Execute_WithTerminalRule_StopsAfterApplying()
    {
        var builder = MarkovAlgorithm.Create();
        builder.AddRule("a", "x", isTerminal: true);
        builder.AddRule("x", "y");
        var algorithm = builder.Build();
        
        var result = algorithm.Execute("abc", AlgorithmCancellation.Default);
        
        result.Termination.Should().Be(TerminationStatus.Success);
        result.Output.Should().Be("xbc"); // Terminal rule stops before second rule applies
        result.Steps.Should().Be(1);
    }

    [Fact]
    public void Execute_WhenNoRulesMatch_ReturnsStuck()
    {
        var algorithm = CreateSimpleReplacementAlgorithm();
        var cancellation = AlgorithmCancellation.Default;
        
        var result = algorithm.Execute("xyz", cancellation);
        
        result.Termination.Should().Be(TerminationStatus.Stuck);
        result.Output.Should().Be("xyz");
    }
    
    [Fact]
    public void Execute_WithStrictAlphabetAndEmptyInput_ValidatesSuccessfully()
    {
        var builder = MarkovAlgorithm.Create();
        builder.WithAlphabet('a', 'b', 'c');
        builder.AddRule("", "c", true);
        var algorithm = builder.Build();
        
        var result = algorithm.Execute("", AlgorithmCancellation.Default);
        
        result.Termination.Should().Be(TerminationStatus.Success);
    }

    [Fact]
    public void Execute_WithVerboseMode_ReturnsTrace()
    {
        var algorithm = CreateSimpleReplacementAlgorithm();
        var cancellation = AlgorithmCancellation.Default;
        
        var result = algorithm.Execute("abc", cancellation, verbose: true);
        
        result.Trace.Should().NotBeNull();
        result.Trace.Should().NotBeEmpty();
        result.Trace.First().Should().Contain("input");
    }

    [Fact]
    public void Execute_WithoutVerboseMode_TraceIsNull()
    {
        var algorithm = CreateSimpleReplacementAlgorithm();
        var cancellation = AlgorithmCancellation.Default;
        
        var result = algorithm.Execute("abc", cancellation, verbose: false);
        
        result.Trace.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Execute_WithCancellation_StopsExecution()
    {
        var builder = MarkovAlgorithm.Create();
        builder.AddRule("a", "aa"); // Creates infinite growth
        var algorithm = builder.Build();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(5);
        
        var result = algorithm.Execute("a", new AlgorithmCancellation(100, cts.Token));
        
        result.Termination.Should().Be(TerminationStatus.Aborted);
    }

    [Fact]
    public void Execute_WithMaxSteps_LimitsExecution()
    {
        var builder = MarkovAlgorithm.Create();
        builder.AddRule("a", "aa");
        var algorithm = builder.Build();
        
        var result = algorithm.Execute("a", new AlgorithmCancellation(3), true);
        output.WriteLine(string.Join('\n', result.Trace));
        
        result.Termination.Should().Be(TerminationStatus.Aborted);
        result.Steps.Should().Be(3);
    }

    [Fact]
    public void Execute_AppliesRulesSequentiallyUntilNoMatch()
    {
        var builder = MarkovAlgorithm.Create();
        builder.AddRule("a", "b");
        builder.AddRule("b", "c");
        builder.AddRule("c", "d");
        var algorithm = builder.Build();
        
        var result = algorithm.Execute("a", AlgorithmCancellation.Default, true);
        output.WriteLine(string.Join('\n', result.Trace));

        result.Termination.Should().Be(TerminationStatus.Stuck);
        result.Output.Should().Be("d");
        result.Steps.Should().Be(4);
    }

    [Fact]
    public void Execute_WithCommentInRule_TraceIncludesComment()
    {
        var builder = MarkovAlgorithm.Create();
        builder.AddRule("a", "b").WithComment("Convert A to B");
        var algorithm = builder.Build();
        var cancellation = AlgorithmCancellation.Default;
        
        var result = algorithm.Execute("a", cancellation, verbose: true);
        output.WriteLine(string.Join('\n', result.Trace));

        result.Trace.Should().Contain(trace => trace.Contains("Convert A to B"));
    }

	#endregion

	#region Property Tests

    [Fact]
    public void Rules_ReturnsReadOnlyListOfSubstitutions()
    {
        var algorithm = CreateTestAlgorithm();
        
        algorithm.Rules.Should().HaveCount(2);
    }

    [Fact]
    public void Name_ReturnsConfiguredName()
    {
        var algorithm = CreateTestAlgorithm("TestName");
        
        algorithm.Name.Should().Be("TestName");
    }

    [Fact]
    public void Alphabet_WhenRestricted_ReturnsAllowedCharacters()
    {
        var builder = MarkovAlgorithm.Create();
        builder.WithAlphabet('a', 'b', 'c');
        builder.WithMarkers('$', '%');
        builder.AddRule("a", "c");
        builder.AddRule("a", "%");
        var algorithm = builder.Build();
        
        algorithm.Alphabet.Should().BeEquivalentTo(['a', 'b', 'c']);
        algorithm.Markers.Should().BeEquivalentTo(['$', '%']);
    }
    
    [Fact]
    public void Alphabet_WhenRestricted_CantAddRestrictedSymbol()
    {
        var addInvalidPattern = () =>
        { 
            MarkovAlgorithm.Create().WithAlphabet('0', '1').AddRule('x', '0').Build();
        };

        var addInvalidReplacement = () =>
        {
            MarkovAlgorithm.Create().WithAlphabet('0', '1').AddRule('1', 'y').Build();
        };
        
        addInvalidPattern.Should().Throw<SymbolIsNotAllowedException>().WithMessage("*pattern*");
        addInvalidReplacement.Should().Throw<SymbolIsNotAllowedException>().WithMessage("*replacement*");
    }

    [Fact]
    public void Alphabet_WhenUnrestricted_ReturnsNull()
    {
        var algorithm = CreateTestAlgorithm();
        
        algorithm.Alphabet.Should().BeNull();
        algorithm.Markers.Should().BeNull();
    }
    
    [Fact]
    public void Markers_WhenNotConfigured_ReturnsNull()
    {
        var algorithm = CreateTestAlgorithm();
        
        algorithm.Markers.Should().BeNull();
    }

	#endregion

	#region Integration Tests

    [Fact]
    public void ComplexMarkovAlgorithm_ExecutesCorrectly()
    {
        // Binary increment algorithm: 101 -> 110
        var builder = MarkovAlgorithm.Create();
        builder.AddRule("B0", "BA", isTerminal: true);
        builder.AddRule("x1", "B");
        builder.AddRule("", "x");
        var algorithm = builder.Build();
        
        var result = algorithm.Execute("101", AlgorithmCancellation.Default);
        
        result.Termination.Should().Be(TerminationStatus.Success);
        result.Output.Should().Be("BA1");
        result.Trace.Should().BeNullOrEmpty();
    }

    [Fact]
    public void AlgorithmWithMultipleSteps_ProducesCorrectTrace()
    {
        var builder = MarkovAlgorithm.Create();
        builder.AddRule("a", "b");
        builder.AddRule("b", "c");
        var algorithm = builder.Build();
        var cancellation = AlgorithmCancellation.Default;
        
        var result = algorithm.Execute("a", cancellation, verbose: true);
        
        result.Steps.Should().Be(3);
        result.Trace.Should().HaveCount(4); // Initial + 2 steps
        result.Trace[0].Should().Contain("input");
        result.Trace[1].Should().Contain("b");
        result.Trace[2].Should().Contain("c");
    }

	#endregion

	#region Helper Methods

    /// <summary>Algorithm with single rules <c>a -> x</c>, <c>b -> y</c>.</summary>
    private static MarkovAlgorithm CreateTestAlgorithm(string? name = null)
    {
        var builder = MarkovAlgorithm.Create(name);
        builder.AddRule("a", "x");
        builder.AddRule("b", "y");
        return builder.Build();
    }

    /// <summary>Algorithm with single rule <c>a -> x</c>.</summary>
    private static MarkovAlgorithm CreateSimpleReplacementAlgorithm()
    {
        var builder = MarkovAlgorithm.Create();
        builder.AddRule("a", "x", true);
        return builder.Build();
    }

	#endregion
}