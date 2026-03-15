namespace PrettyMachines.Implementations.Turing;

/// <summary>Wrapper type for single cell of the tape.</summary>
/// <typeparam name="TSymbol">Data type of the cell.</typeparam>
public readonly struct TapeSymbol<TSymbol>(TSymbol? symbol, bool isEmpty = false, int position = 0)
{
    /// <summary>Create empty cell.</summary>
    public static TapeSymbol<TSymbol> Empty(TSymbol? symbol = default, int index = 0) => new(symbol, true, index);
    
    /// <summary>Returns <c>true</c> if this cell has no actual value.</summary>
    public bool IsEmpty { get; } = isEmpty;

    /// <summary>Returns actual value inside this cell.</summary>
    public TSymbol? Symbol { get; } = symbol;

    /// <summary>Position of this cell in the tape.</summary>
    public int Position { get; } = position;


    public override string ToString()
    {
        return IsEmpty ? $"[{Position}] <empty>" : $"[{Position}] {Symbol?.ToString() ?? "<null>"}";
    }
}
