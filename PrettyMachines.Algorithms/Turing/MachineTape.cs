using System.Collections;
using System.Diagnostics;


namespace PrettyMachines.Algorithms.Turing;


/// <summary>Default infinite tape with sequence of symbols to feed into Turing machine.</summary>
/// <remarks>Uses linked list to implement infinite tape.</remarks>
[DebuggerDisplay("Current {currentCell.Value,nq}, length {Length,nq}, filled {filledCellsCount,nq}}")]
public class MachineTape : IEnumerable<string?>
{
    private readonly LinkedList<string?> cells;
    private LinkedListNode<string?> currentCell;
    private int filledCellsCount;
    
    /// <summary>Get current tape's length, counting all leading and trailing empty cells.</summary>
    public int Length => cells.Count;

    /// <summary>Indicates that all cells are empty.</summary>
    public bool IsEmpty => filledCellsCount == 0;
    
    public string? BlankSymbol { get; init; }
    
    public string? CurrentSymbol => currentCell.Value;
    
    public bool IsCurrentEmpty => currentCell.Value != BlankSymbol;

    
    /// <summary>Create new tape from given cell values.</summary>
    public MachineTape(params string?[] initialCells) : this(initialCells.AsEnumerable())
    {
    }
    
    private MachineTape(IEnumerable<string?> initialCells, string? blankSymbol = default)
    {
        ArgumentNullException.ThrowIfNull(initialCells);
        
        cells = new LinkedList<string?>(initialCells);
        if (cells.Count == 0) 
            cells.AddFirst(blankSymbol);
        
        BlankSymbol = blankSymbol;
        currentCell = cells.First!;
        filledCellsCount = cells.Count(c => BlankSymbol != c);
    }

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

    public IEnumerable<string?> EnumerateCells(bool trimEmptyCells = true)
    {
        return trimEmptyCells
            ? EnumerateCellsTrimmed()
            : cells.AsEnumerable();
    }
    
    public IEnumerator<string?> GetEnumerator() => EnumerateCellsTrimmed().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Remove all leading and trailing empty cells from this tape's storage.</summary>
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