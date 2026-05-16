using PrettyMachines.Algorithms.Markov;


namespace PrettyMachines.Tests.Markov;

public class MarkovSubstitutions
{
    [Fact]
    public void Compare_EqualPatterns()
    {
        Substitution[] rules =
        [
            new Substitution("123", "abc", true),
            new Substitution("123", "abc", true),
            new Substitution("123", "ZZZ", true),
            new Substitution("123", "abc", false),
            new Substitution("123", "ZZZ", false)
        ];

        for (var i = 0; i < rules.Length; i++)
        {
            for (var j = i + 1; j < rules.Length; j++)
            {
                if (rules[i] != rules[j])
                {
                    Assert.Fail($"Rule {i} '{rules[i]}' must be equal to rule {j} '{rules[j]}'");
                }
            }
        }
    }
    
    [Fact]
    public void Compare_DifferentPatterns()
    {
        var rule1 = new Substitution("123", "abc", true);
        var rule2 = new Substitution("xyz", "abc", true);
        if (rule1 == rule2)
        {
            Assert.Fail($"Rule '{rule1}' must be not equal to rule '{rule2}'");
        }
    }
    
    [Theory]
    [InlineData("A", "a", "__A__", "__a__")]
    [InlineData("A", "a", "__AA__", "__aA__")]
    [InlineData("xx", "X", "AAxxB", "AAXB")]
    [InlineData("A", "AAA", "__A", "__AAA")]
    [InlineData("", "$", "12345", "$12345")]
    public void Replacing_SuccessExamples(string pattern, string replace, string input, string output)
    {
        CheckSubstitution(output, input, pattern, replace);
    }   
    
    [Theory]
    [InlineData("A", "#", "12")]
    [InlineData("A", "#", "aa")]
    [InlineData("12A", "#", "12b")]
    [InlineData("x", "X", "")]
    [InlineData("x", "$", "X")]
    [InlineData("X", "$", "x")]
    public void Replacing_FailExamples(string pattern, string replace, string input)
    {
        CheckSubstitution(null, input, pattern, replace);
    }    
        
    [Theory]
    [InlineData("x", "yyx", true)]
    [InlineData("xx", "yy", false)]
    [InlineData("xx", "xx", true)]
    [InlineData("xx", "xxxx", true)]
    [InlineData("xx", "__x__", false)]
    [InlineData("xx", "__xx__", true)]
    [InlineData("xx", "__xx__xx__", true)]
    public void Matching_SimpleExamples(string pattern, string input, bool shouldMatch)
    {
        CheckMatching(shouldMatch, input, pattern);
    }
    
    [Theory]
    [InlineData("xYz", "_xyz", false)]
    [InlineData("xYz", "_xYz", true)]
    [InlineData("xyz", "_xYz", false)]
    public void Matching_CaseSensitiveExamples(string pattern, string input, bool shouldMatch)
    {
        CheckMatching(shouldMatch, input, pattern);
    }
    
    [Theory]
    [InlineData("", "1234", true)]
    [InlineData("1234", "", false)]
    public void Matching_EmptyStringsExamples(string pattern, string input, bool shouldMatch)
    {
        CheckMatching(shouldMatch, input, pattern);
    }
    
    private static void CheckMatching(bool expectedMatch, string input, string pattern)
    {
        var rule = new Substitution(pattern, "#", isTerminal: false);
        if (expectedMatch == rule.Matches(input))
            return;
        
        Assert.Fail(
            $"""
             Expecting {nameof(Substitution)}.{nameof(Substitution.Matches)} to return {expectedMatch}
             - rule: {rule}
             - input: '{input}'
             """
        );
    }

    private static void CheckSubstitution(string? expectedOutput, string input, string pattern, string replacement)
    {
        var rule = new Substitution(pattern, replacement, isTerminal: false);
        var originalInput = new string(input);
        var actualMatch = rule.TrySubstitute(ref input);
        var shouldMatch = expectedOutput != null;
        
        if (!shouldMatch && !actualMatch && input == originalInput)
            return; 
        if (shouldMatch && actualMatch && input == expectedOutput)
            return;
        
        Assert.Fail(
            $"""
             Expecting {nameof(Substitution)}.{nameof(Substitution.TrySubstitute)} to return {shouldMatch} and{(shouldMatch ? " " : " not ")}produce new string
             - rule: {rule}
             - input:       '{originalInput}'
             - act. output: '{input}'
             - exp. output: '{expectedOutput ?? originalInput}'
             """
        );
    }
}