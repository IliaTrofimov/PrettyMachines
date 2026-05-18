using PrettyMachines.Algorithms.Markov;

namespace PrettyMachines.Tests.Markov;

public class SubstitutionTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithPatternAndReplacement_CreatesRule()
    {
        var rule = new Substitution("abc", "xyz");
        
        rule.Pattern.Should().Be("abc");
        rule.Replacement.Should().Be("xyz");
        rule.IsTerminal.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithTerminalFlag_CreatesTerminalRule()
    {
        var rule = new Substitution("abc", "xyz", isTerminal: true);
        
        rule.IsTerminal.Should().BeTrue();
    }

    [Fact]
    public void CopyConstructor_CreatesIdenticalRule()
    {
        var original = new Substitution("abc", "xyz", isTerminal: true);
        
        var copy = new Substitution(original);
        
        copy.Pattern.Should().Be(original.Pattern);
        copy.Replacement.Should().Be(original.Replacement);
        copy.IsTerminal.Should().Be(original.IsTerminal);
    }

    #endregion

    #region TrySubstitute Tests

    [Theory]
    [InlineData("12345678", "12", "x",   "x345678")]
    [InlineData("12345678", "12", "xx",  "xx345678")]
    [InlineData("12345678", "12", "xxx", "xxx345678")]
    [InlineData("12345678", "4", "x",    "123x5678")]
    [InlineData("12345678", "4", "xx",   "123xx5678")]
    [InlineData("12345678", "4", "",     "1235678")]
    [InlineData("12345678", "8", "xx",   "1234567xx")]
    [InlineData("1223221", "22", "x",    "1x3221")]
    [InlineData("123", "2", "qwertyuiop", "1qwertyuiop3")]
    public void TrySubstitute_ReplacesPattern(string text, string pattern, string replacement, string result)
    {
        var rule = new Substitution(pattern, replacement);
        var original = text;
    
        rule.Matches(text).Should().BeTrue();
        rule.TrySubstitute(ref text).Should().BeTrue();
        text.Should().Be(result, $"'{original}' contains '{pattern}'");
    }

    [Theory]
    [InlineData("1234", "yyy")]
    [InlineData("abcd", "A")]
    [InlineData("abcd", "___abcdefg___")]
    public void TrySubstitute_MissingPattern(string text, string pattern)
    {
        var rule = new Substitution(pattern, "xxx");
        var original = text;
        
        rule.Matches(text).Should().BeFalse();
        rule.TrySubstitute(ref text).Should().BeFalse();
        text.Should().Be(original, $"'{original}' does not contain '{pattern}'");
    }

    [Theory]
    [InlineData("")]
    [InlineData("content")]
    public void TrySubstitute_WithEmptyPattern_PrependsReplacement(string text)
    {
        var rule = new Substitution("", "prefix_");
        
        var result = rule.TrySubstitute(ref text);
        
        result.Should().BeTrue();
        text.Should().Be(rule.Pattern + text);
    }
    
    [Fact]
    public void TrySubstitute_ThenMatches_ReturnsTrueForRemainingMatches()
    {
        var rule = new Substitution("a", "x");
        var text = "aaa";
        
        rule.Matches(text).Should().BeTrue();
        rule.TrySubstitute(ref text);
        text.Should().Be("xaa");
        rule.Matches(text).Should().BeTrue();
    }
    
    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSamePattern_ReturnsTrue()
    {
        var rule1 = new Substitution("abc", "xyz");
        var rule2 = new Substitution("abc", "different");
        
        rule1.Equals(rule2).Should().BeTrue();
        (rule1 == rule2).Should().BeTrue();
        (rule1 != rule2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentPattern_ReturnsFalse()
    {
        var rule1 = new Substitution("abc", "xyz");
        var rule2 = new Substitution("def", "xyz");
        
        rule1.Equals(rule2).Should().BeFalse();
        (rule1 == rule2).Should().BeFalse();
        (rule1 != rule2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithSameObject_ReturnsTrue()
    {
        var rule = new Substitution("abc", "xyz");
        
        rule.Equals(rule).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var rule = new Substitution("abc", "xyz");
        
        rule.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ReturnsPatternHashCode()
    {
        var rule = new Substitution("abc", "xyz");
        
        var hash = rule.GetHashCode();
        
        hash.Should().Be("abc".GetHashCode());
    }

    [Fact]
    public void ExactEquals_WithSameAllProperties_ReturnsTrue()
    {
        var rule1 = new Substitution("abc", "xyz", isTerminal: true);
        var rule2 = new Substitution("abc", "xyz", isTerminal: true);
        
        rule1.ExactEquals(rule2).Should().BeTrue();
    }

    [Fact]
    public void ExactEquals_WithDifferentReplacement_ReturnsFalse()
    {
        var rule1 = new Substitution("abc", "xyz");
        var rule2 = new Substitution("abc", "123");
        
        rule1.ExactEquals(rule2).Should().BeFalse();
    }

    [Fact]
    public void ExactEquals_WithDifferentTerminalFlag_ReturnsFalse()
    {
        var rule1 = new Substitution("abc", "xyz", isTerminal: false);
        var rule2 = new Substitution("abc", "xyz", isTerminal: true);
        
        rule1.ExactEquals(rule2).Should().BeFalse();
    }

    [Fact]
    public void ExactEquals_WithDifferentPattern_ReturnsFalse()
    {
        var rule1 = new Substitution("abc", "xyz");
        var rule2 = new Substitution("def", "xyz");
        
        rule1.ExactEquals(rule2).Should().BeFalse();
    }

    [Fact]
    public void ExactEquals_WithSameReference_ReturnsTrue()
    {
        var rule = new Substitution("abc", "xyz");
        
        rule.ExactEquals(rule).Should().BeTrue();
    }

    #endregion
}