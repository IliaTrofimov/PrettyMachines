using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Markov;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.Markov;

public class MarkovAlgorithmBuilding(ITestOutputHelper output) : BaseAlgorithmTest(output)
{
    [Fact]
    public void Builder_AddingUniqueRules()
    {
        var r1 = new Substitution("a", "A");
        var r2 = new Substitution("a", "x");
        var r3 = new Substitution("A", "A", true);
        var r4 = new Substitution("b", "B");
        var r5 = new Substitution("b", "bb");
        var r6 = new Substitution("B", "B", true);
        
        var algorithm = MarkovAlgorithm.Create()
            .AddRule(r1)
            .AddRule(r2)
            .AddRule(r3)
            .AddRule(r4)
            .AddRule(r5)
            .AddRule(r6).WithComment("Replaces B with B")
            .Build();
        
        Assert.Equal(4, algorithm.Rules.Count);
        Assert.Equivalent(new []{ r1, r3, r4, r6 }, algorithm.Rules);
    }
    
    [Fact]
    public void Builder_AlphabetValidation()
    {
        var ex1 = Assert.Throws<InvalidOperationException>(() =>
            MarkovAlgorithm.Create().WithAlphabet('1', '0').AddRule('x', '0').Build()
        );
        Assert.Contains("'x'", ex1.Message);
        Assert.Contains("pattern", ex1.Message);
        
        var ex2 = Assert.Throws<InvalidOperationException>(() =>
            MarkovAlgorithm.Create().WithAlphabet('1', '0').AddRule('1', 'y').Build()
        );
        Assert.Contains("'y'", ex2.Message);
        Assert.Contains("replacement", ex2.Message);
    }
    
    [Fact]
    public void Builder_MarkersValidation()
    {
        var ex1 = Assert.Throws<InvalidOperationException>(() =>
            MarkovAlgorithm.Create().WithMarkers('1', '0').AddRule('x', '0').Build()
        );
        Assert.Contains("'x'", ex1.Message);
        Assert.Contains("pattern", ex1.Message);
        
        var ex2 = Assert.Throws<InvalidOperationException>(() =>
            MarkovAlgorithm.Create().WithMarkers('1', '0').AddRule('1', 'y').Build()
        );
        Assert.Contains("'y'", ex2.Message);
        Assert.Contains("replacement", ex2.Message);
    }
    
    [Theory]
    [InlineData("abccba", "ABCCBA")]
    [InlineData("aaaaab", "AAAAAB")]
    [InlineData("abcccc", "ABCCCC")]
    [InlineData("", "")]
    [InlineData("xyz", "xyz", TerminationStatus.InvalidInput)]
    public void Execute_UpperCase(string input, string expectedOutput, TerminationStatus termination = TerminationStatus.Success)
    {
        var algorithm = MarkovAlgorithm.Create()
            .WithAlphabet('a', 'b', 'c', 'A', 'B', 'C')
            .WithMarkers('$')
            .AddRule("a", "A")
            .AddRule("b", "B")
            .AddRule("c", "C")
            .AddRule("$", "", true).WithComment("Remove marker and stop")
            .AddRule("", "$").WithComment("Place marker if no lower case symbols left")
            .Build();
        
        CheckAlgorithm(expectedOutput, termination, algorithm, input);
    }

    [Fact]
    public async Task Execute_InfiniteAlgorithm_PrematureStop()
    {
        var algorithm = MarkovAlgorithm.Create()
            .AddRule("a", "A")
            .AddRule("b", "B")
            .AddRule("c", "C")
            .AddRule("A", "aA").WithComment("loop")
            .AddRule("$", "", true)
            .AddRule("", "$")
            .Build();

        var input = "aa";
        
        var timeout = Task.Delay(TimeSpan.FromSeconds(1));
        var execution = Task.Run(() =>
        {
            var cancellation = new AlgorithmCancellation(10);
            CheckAlgorithm("AAAAAA", TerminationStatus.Aborted, algorithm, input, cancellation);
        });

        var firstTask = await Task.WhenAny(execution, timeout);
        if (firstTask == timeout)
            Assert.Fail("Timeout");
    }
    
    [Fact]
    public async Task Execute_InfiniteAlgorithm_TokenStop()
    {
        var algorithm = MarkovAlgorithm.Create()
            .AddRule("a", "A")
            .AddRule("b", "B")
            .AddRule("c", "C")
            .AddRule("A", "aA").WithComment("loop")
            .AddRule("$", "", true)
            .AddRule("", "$")
            .Build();

        var input = "aa";
        
        var timeout = Task.Delay(TimeSpan.FromSeconds(1));
        var execution = Task.Run(() =>
        {
            using var ctSource = new CancellationTokenSource(TimeSpan.FromSeconds(0.05));
            var cancellation = new AlgorithmCancellation(ctSource.Token);
            CheckAlgorithm(null, TerminationStatus.Aborted, algorithm, input, cancellation);
        });
        
        var firstTask = await Task.WhenAny(execution, timeout);
        if (firstTask == timeout)
            Assert.Fail("Timeout");
    }
}