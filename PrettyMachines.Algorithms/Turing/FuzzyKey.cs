using System.Diagnostics;


namespace PrettyMachines.Algorithms.Turing;

/// <summary>
/// Represents special dictionary key that supports fuzzy matching. It can match exact values or any empty or not empty.
/// </summary>
/// <typeparam name="T">Type of the value.</typeparam>
[DebuggerDisplay("DebugString()")]
public readonly struct FuzzyKey<T>
{
    public readonly SymbolMatch Match;
    public readonly T? Value;
    public readonly int Hash;

    internal FuzzyKey(T? value, SymbolMatch match = SymbolMatch.Exact)
    {
        Value = value;
        Match = match;
        Hash = HashCode.Combine(value, match);
    }
    
    /// <summary>Creates new key that matches exact value.</summary>
    /// <param name="value">Value to test.</param>
    /// <returns>New fuzzy key instance.</returns>
    public static FuzzyKey<T> Exact(T value) => new(value, SymbolMatch.Exact);
    
    /// <summary>Get fuzzy key that matches empty values.</summary>
    public static FuzzyKey<T> Empty { get; } = new(default, SymbolMatch.Empty);
    
    /// <summary>Get fuzzy key that matches any not empty value.</summary>
    public static FuzzyKey<T> NotEmpty { get; } = new(default, SymbolMatch.NotEmpty);
    
    /// <summary>Get fuzzy key that matches any value.</summary>
    public static FuzzyKey<T> Any { get; } = new(default, SymbolMatch.Any);

    public override string ToString()
    {
        return Match == SymbolMatch.Exact ? Value?.ToString() ?? "" : Match.ToString();
    }

    private string DebugString()
    {
        return Match == SymbolMatch.Exact ? $"{Match} '{Value}'" : $"{Match} {typeof(T)}";
    }
}