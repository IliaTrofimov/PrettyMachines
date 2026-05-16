namespace PrettyMachines.Algorithms.Turing;

/// <summary>Symbol comparision types.</summary>
public enum SymbolMatch
{
    /// <summary>Matches when scanned symbol has expected exact value.</summary>
    Exact = 0, 
    /// <summary>Matches when scanned symbol is not empty.</summary>
    NotEmpty = 1,
    /// <summary>Matches when scanned symbol is empty.</summary>
    Empty = 2,
    /// <summary>Matches any symbol.</summary>
    Any = 3
}