namespace PrettyMachines.Implementations;

/// <summary>
/// Set of symbols that define correct brackets sequence. 
/// </summary>
public sealed class BracketsGrammarSymbols
{
    /// <summary>Get or set opening bracket.</summary>
    public char Left { get; init; } = '(';
    
    /// <summary>Get or set closing bracket.</summary>
    public char Right { get; init; } = ')';
    
    /// <summary>Get or set special marker symbol.</summary>
    public char Marked { get; init; } = 'x';
    
    /// <summary>Get or set special symbol for rejected input.</summary>
    public char Rejected { get; init; } = 'R';
    
    /// <summary>Get or set special symbol for accepted input.</summary>
    public char Accepted { get; init; } = 'A';
    
    public void Validate()
    {
        if (Left == Right)
            throw new InvalidOperationException("Symbols cannot be the same.");
        if (Left == Accepted || Right == Accepted)
            throw new InvalidOperationException("Symbols cannot be the same.");
        if (Left == Marked || Right == Marked)
            throw new InvalidOperationException("Symbols cannot be the same.");
        if (Left == Rejected || Right == Rejected)
            throw new InvalidOperationException("Symbols cannot be the same.");
        if (Marked == Rejected)
            throw new InvalidOperationException("Symbols cannot be the same.");
        if (Marked == Accepted)
            throw new InvalidOperationException("Symbols cannot be the same.");
    }
}