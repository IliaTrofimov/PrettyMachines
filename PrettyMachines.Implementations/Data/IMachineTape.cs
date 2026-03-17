using System.Diagnostics.CodeAnalysis;


namespace PrettyMachines.Implementations.Data;

/// <summary>Abstract automaton's tape.</summary>
/// <typeparam name="TSymbol">Data type of the tape cells.</typeparam>
public interface IMachineTape<TSymbol> where TSymbol : IEquatable<TSymbol>
{
    /// <summary>Returns <c>true</c> if this tape contains only empty cells or have zero length.</summary>
    public bool IsEmpty { get; }
    
    /// <summary>Move machine's head in given direction.</summary>
    public void MoveHead(TapeMovement movement);
    
    /// <summary>Put given symbol into current cell.</summary>
    public void PutSymbol(TSymbol symbol);
    
    /// <summary>Put empty symbol into current cell.</summary>
    public void EraseSymbol();
    
    /// <summary>Get current cell's value.</summary>
    /// <returns><c>True</c> if cell wasn't empty.</returns>
    public bool ReadSymbol([NotNullWhen(true)] out TSymbol? symbol);
    
    /// <summary>Get current cell.</summary>
    public TapeSymbol<TSymbol> ReadSymbol();
    
    /// <summary>Enumerate all cells.</summary>
    /// <remarks>Should skip all leading and trailing empty cells if <paramref name="trimEmptyCells"/> is <c>true</c>.</remarks>
    public IEnumerable<TSymbol?> EnumerateCells(bool trimEmptyCells = true);
}


public static class MachineTapeExtensions
{
    /// <summary>
    /// Concatenate all symbols into one string representation.
    /// </summary>
    public static string JoinAsString<T>(this IMachineTape<T> tape, string separator = "", bool trimEmptyCells = true) 
        where T : IEquatable<T>
    {
        return string.Join(separator, tape.EnumerateCells(trimEmptyCells));
    }

    /// <summary>
    /// Move head in given direction n times.
    /// </summary>
    public static void MoveHead<T>(this IMachineTape<T> tape, int times, TapeMovement movement)
        where T : IEquatable<T>
    {
        if (movement == TapeMovement.None && times <= 0)
            return;

        for (var i = 0; i < times; i++)
           tape.MoveHead(movement);
    }
}