namespace PrettyMachines.Algorithms.Markov;

/// <summary>Represents a single rewrite rule (pattern - replacement) in a Markov algorithm.</summary>
public class Substitution : IEquatable<Substitution>
{
    private string? stringView;
    private const StringComparison stringComparison = StringComparison.Ordinal;
    
    /// <summary>Gets the pattern string to search for.</summary>
    public string Pattern { get; }
    
    /// <summary>Gets the replacement string.</summary>
    public string Replacement { get; }
    
    /// <summary>Indicates whether this rule terminates the algorithm when applied.</summary>
    public bool IsTerminal { get; }
    
    
    /// <summary>Initializes a new substitution rule.</summary>
    /// <param name="pattern">Pattern to match (empty means prepend replacement).</param>
    /// <param name="replacement">Replacement text (empty means delete pattern).</param>
    /// <param name="isTerminal">If <c>true</c>, algorithm stops after applying this rule.</param>
    public Substitution(string pattern, string replacement, bool isTerminal = false)
    {
        Pattern = pattern;
        Replacement = replacement;
        IsTerminal = isTerminal;
    }

    /// <summary>Creates a copy of an existing substitution rule.</summary>
    /// <param name="other">The rule to copy.</param>
    public Substitution(Substitution other) : this(other.Pattern, other.Replacement, other.IsTerminal)
    {
    }
    
    
    /// <summary>Attempts to apply this rule to a string if the pattern matches.</summary>
    /// <param name="text">The string to transform (modified if match found).</param>
    /// <returns><c>True</c> if the rule was applied; <c>false</c> if pattern not found.</returns>
    public bool TrySubstitute(ref string text)
    {
        if (string.IsNullOrEmpty(Pattern))
        {
            text = Replacement + text;
            return true;
        }
        
        if (Pattern.Length > text.Length)
            return false;
        
        var startIndex = text.IndexOf(Pattern, stringComparison);
        if (startIndex == -1)
            return false;
        
        var inpLength = text.Length;
        var patLength = Pattern.Length;
        
        if (startIndex == 0)
        {
            ReadOnlySpan<char> tail = inpLength > patLength ? text.AsSpan(patLength) : default;
            text = string.Concat(Replacement, tail);
        }
        else
        {
            var endIndex = startIndex + patLength;
            ReadOnlySpan<char> head = text.AsSpan(0, startIndex);
            ReadOnlySpan<char> tail = endIndex < inpLength ? text.AsSpan(endIndex) : default;
            text = string.Concat(head, Replacement, tail);
        }
        
        return true;
    }
    
    /// <summary>Checks whether the rule's pattern exists in the given text.</summary>
    /// <param name="text">The string to search within.</param>
    /// <returns><c>True</c> if pattern is empty or found in text.</returns>
    public bool Matches(string text)
    {
        if (string.IsNullOrEmpty(Pattern))
            return true;
        if (Pattern.Length > text.Length)
            return false;
        return text.Contains(Pattern, stringComparison);
    }
    
    public override string ToString()
    {
        if (stringView == null)
        {
            var arrow = IsTerminal ? "=>" : "->";
            stringView = Pattern + arrow + Replacement;   
        }
        return stringView;
    }
    
    public string ToString(MarkovSubstitutionParsingFlags options)
    {
        if (options == MarkovSubstitutionParsingFlags.Default)
            return ToString();
        
        var arrow = IsTerminal 
            ? (options.HasFlag(MarkovSubstitutionParsingFlags.UseDottedArrow) ? "->." : "=>") 
            : "->";

        return options.HasFlag(MarkovSubstitutionParsingFlags.QuoteStrings) 
            ? $"'{Pattern}'{arrow}'{Replacement}'" 
            : Pattern + arrow + Replacement;
    }

    /// <summary>Two rules are considered equal if their <see cref="Pattern"/>s are equal.</summary> 
    public static bool operator==(Substitution left, Substitution right) => Equals(left, right);

    /// <summary>Two rules are considered unequal if their <see cref="Pattern"/>s are defferent.</summary> 
    public static bool operator!=(Substitution left, Substitution right) => !Equals(left, right);
    
    public override int GetHashCode() => Pattern.GetHashCode();
    
    /// <inheritdoc/>
    /// <returns>Two rules are considered equal if their <see cref="Pattern"/>s are equal.</returns> 
    public bool Equals(Substitution? other)
    {
        return ReferenceEquals(other, this) || (other is not null && Pattern.Equals(other.Pattern, stringComparison));
    }

    /// <inheritdoc/>
    /// <returns>Two rules are considered equal if their <see cref="Pattern"/>s are equal.</returns> 
    public override bool Equals(object? obj)
    {
        return obj is Substitution s && Equals(s);
    }

    /// <summary>Determines whether two <see cref="Substitution"/> have all equal properties.</summary>
    public bool ExactEquals(Substitution? other)
    {
        return ReferenceEquals(other, this) 
               || (other is not null && 
                   Pattern.Equals(other.Pattern, stringComparison) && 
                   Replacement.Equals(other.Replacement, stringComparison) &&
                   IsTerminal == other.IsTerminal);
    }
}