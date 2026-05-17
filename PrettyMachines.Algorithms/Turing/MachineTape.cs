using System.Collections;
using System.Diagnostics;


namespace PrettyMachines.Algorithms.Turing;


/// <summary>
/// Represents an infinite Turing machine tape with head that can move left or right.
/// </summary>
[DebuggerDisplay("Current {currentCell.Value,nq}, length {Length,nq}, filled {filledCellsCount,nq}}")]
public class MachineTape : IEnumerable<string?>
{
    private readonly LinkedList<string?> cells;
    private LinkedListNode<string?> currentCell;
    private int filledCellsCount;
    
    
    /// <summary>Gets special value that represents an empty symbol.</summary>
    public string? BlankSymbol { get; init; }
    
    /// <summary>Gets the total number of cells on the tape, including leading and trailing blanks.</summary>
    public int Length => cells.Count;

    /// <summary>Indicates whether all cells on the tape are blank symbols.</summary>
    public bool IsEmpty => filledCellsCount == 0;
    
    /// <summary>Gets the symbol currently under the tap's head.</summary>
    public string? CurrentSymbol => currentCell.Value;
    
    /// <summary>Indicates whether the current cell contains the blank symbol.</summary>
    public bool IsCurrentEmpty => currentCell.Value == BlankSymbol;

    
    /// <summary>Initializes a new tape from given cell values.</summary>
    /// <param name="initialCells">Initial cell values to populate the tape.</param>
    public MachineTape(params string?[] initialCells) : this(initialCells.AsEnumerable())
    {
    }
    
    /// <summary>Creates a new tape from a sequence of cell values.</summary>
    /// <param name="initialCells">Initial cell values to populate the tape.</param>
    /// <param name="blankSymbol">Symbol representing empty cells (default is null).</param>
    /// <exception cref="ArgumentNullException">Thrown when initialCells is null.</exception>
    public MachineTape(IEnumerable<string?> initialCells, string? blankSymbol = default)
    {
        ArgumentNullException.ThrowIfNull(initialCells);
        
        cells = new LinkedList<string?>(initialCells);
        if (cells.Count == 0) 
            cells.AddFirst(blankSymbol);
        
        BlankSymbol = blankSymbol;
        currentCell = cells.First!;
        filledCellsCount = cells.Count(c => BlankSymbol != c);
    }

    /// <summary>Moves the tape head left or right, extending the tape with blank cells if needed.</summary>
    /// <param name="movement">Direction to move the head.</param>
    public void MoveHead(TapeMovement movement)
    {
        switch (movement)
        {
            case TapeMovement.Left:
            {
                if (currentCell.Previous == null) 
                    cells.AddFirst(BlankSymbol);
                currentCell = currentCell.Previous!;
                break;
            }
            case TapeMovement.Right:
            {
                if (currentCell.Next == null) 
                    cells.AddLast(BlankSymbol);
                currentCell = currentCell.Next!;
                break;
            }
        }
    }

    /// <summary>Erases the current cell by writing the blank symbol.</summary>
    public void EraseSymbol() => PutSymbol(BlankSymbol);
    
    /// <summary>Writes a symbol into the current cell.</summary>
    /// <param name="symbol">The symbol to write.</param>
    public void PutSymbol(string? symbol)
    {
        var isErasing = BlankSymbol == symbol;
        var currentEmpty = currentCell.Value == BlankSymbol;
        
        if (isErasing && !currentEmpty)
            filledCellsCount--;
        else if (!isErasing && currentEmpty)
            filledCellsCount++;

        currentCell.Value = symbol;
    }

    /// <summary>Enumerates all cells on the tape.</summary>
    /// <param name="trimEmptyCells">If <c>true</c>, excludes leading and trailing blank cells.</param>
    public IEnumerable<string?> EnumerateCells(bool trimEmptyCells = true)
    {
        return trimEmptyCells ? EnumerateCellsTrimmed() : cells.AsEnumerable();
    }
    
    public IEnumerator<string?> GetEnumerator() => EnumerateCellsTrimmed().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Removes all leading and trailing empty cells from the tape storage.</summary>
    public void TrimEmptyCells()
    {
        var first = FindFirstFilledCell();
        var last = FindLastFilledCell();

        while (!ReferenceEquals(cells.First, first))
            cells.RemoveFirst();
        
        while (!ReferenceEquals(cells.Last, last))
            cells.RemoveLast();
        
        if (cells.Count == 0)
            currentCell = cells.AddFirst(BlankSymbol);
    }

    private IEnumerable<string?> EnumerateCellsTrimmed()
    {
        var first = FindFirstFilledCell();
        var last = FindLastFilledCell();

        while (first != null && !first.Equals(last))
        {
            yield return first.Value;
            first = first.Next;
        }
        
        if (last != null)
            yield return last.Value;
    }
    
    private LinkedListNode<string?>? FindFirstFilledCell()
    {
        var firstFilled = cells.First!;
        while (firstFilled != null && firstFilled.Value == BlankSymbol)
            firstFilled = firstFilled.Next;
        return firstFilled;
    }
    
    private LinkedListNode<string?>? FindLastFilledCell()
    {
        var lastFilled = cells.Last!;
        while (lastFilled != null && lastFilled.Value == BlankSymbol)
            lastFilled = lastFilled.Previous;
        return lastFilled;
    }
}