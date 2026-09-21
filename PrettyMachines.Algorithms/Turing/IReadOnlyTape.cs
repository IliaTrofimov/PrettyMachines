namespace PrettyMachines.Algorithms.Turing;

/// <summary>
/// Provides read-only access to a Turing machine tape.
/// </summary>
public interface IReadOnlyTape : IEnumerable<string?>
{
    /// <summary>Gets special value that represents an empty symbol.</summary>
    string? BlankSymbol { get; }

    /// <summary>Gets the total number of cells on the tape, including leading and trailing blanks.</summary>
    int Length { get; }

    /// <summary>Gets the zero-based index of the cell currently under the tape head.</summary>
    int HeadIndex { get; }

    /// <summary>Indicates whether all cells on the tape are blank symbols.</summary>
    bool IsEmpty { get; }

    /// <summary>Gets the symbol currently under the tape's head.</summary>
    string? CurrentSymbol { get; }

    /// <summary>Indicates whether the current cell contains the blank symbol.</summary>
    bool IsCurrentEmpty { get; }

    /// <summary>Enumerates all cells on the tape.</summary>
    /// <param name="trimEmptyCells">If <c>true</c>, excludes leading and trailing blank cells.</param>
    IEnumerable<string?> EnumerateCells(bool trimEmptyCells = true);
}
