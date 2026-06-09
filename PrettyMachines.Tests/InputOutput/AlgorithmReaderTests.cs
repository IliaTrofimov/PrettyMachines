using PrettyMachines.Algorithms.Markov;
using PrettyMachines.Algorithms.Utils.Parsing;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.InputOutput;

public class AlgorithmReaderTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("'a' -> 'b'", "a", "b", false)]
    [InlineData("'a' => 'b'", "a", "b", true)]
    [InlineData("'a123' -> 'bxx'", "a123", "bxx", false)]
    [InlineData("'456a' => 'zzzz'", "456a", "zzzz", true)]
    [InlineData("'' -> 'bxx'", "", "bxx", false)]
    [InlineData("'456a' => ''", "456a", "", true)]
    [InlineData("'456a'=>'111'", "456a", "111", true)]
    [InlineData("    ''      =>    '111'", "", "111", true)]
    public void ParseMarkovSubstitution_Quoted_Success(string text, string pattern, string replace, bool isTerminal)
    {
        var parser = new MarkovSubstitutionParser();
        var rule = parser.ParseQuoted(text);
        
        rule.Pattern.Should().Be(pattern);
        rule.Replacement.Should().Be(replace);
        rule.IsTerminal.Should().Be(isTerminal);
    }
    
    [Theory]
    [InlineData("a->b", "a", "b", false)]
    [InlineData("a=>b", "a", "b", true)]
    [InlineData("=>b", "", "b", true)]
    [InlineData("v->", "v", "", false)]
    [InlineData("   'a '  =>1231232321", "   'a '  ", "1231232321", true)]
    public void ParseMarkovSubstitution_Unquoted_Success(string text, string pattern, string replace, bool isTerminal)
    {
        var parser = new MarkovSubstitutionParser();
        var rule = parser.ParseUnquoted(text);
        
        rule.Pattern.Should().Be(pattern);
        rule.Replacement.Should().Be(replace);
        rule.IsTerminal.Should().Be(isTerminal);
    }
}