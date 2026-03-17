using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


namespace PrettyMachines.Implementations.Data;


/// <summary>Default tape with sequence of symbols to feed into Turing machine.</summary>
/// <typeparam name="TSymbol">Data type of the tape cells.</typeparam>
[DebuggerDisplay("Current {currentCell.Value,nq}, length {Length,nq}, filled {filledCellsCount,nq}, indexes from {FirstIndex,nq} to {LastIndex,nq}")]
public class TuringMachineTape<TSymbol> : IMachineTape<TSymbol> where TSymbol : IEquatable<TSymbol>
{
    private readonly TSymbol? emptyCellValue;
    private readonly LinkedList<TapeSymbol<TSymbol>> cells;
    private LinkedListNode<TapeSymbol<TSymbol>> currentCell;
    private int filledCellsCount;
    
    /// <summary>Get current tape's length, counting all leading and trailing empty cells.</summary>
    public int Length => cells.Count;
    
    /// <summary>Get current cell's index.</summary>
    public int CurrentIndex => currentCell.Value.Position;
    
    /// <summary>Get first cell's index.</summary>
    public int FirstIndex => cells.First!.Value.Position;
    
    /// <summary>Get last cell's index.</summary>
    public int LastIndex => cells.Last!.Value.Position;

    /// <inheritdoc/> 
    public bool IsEmpty => filledCellsCount == 0;
    
    
    /// <summary>Create new tape from given cell values.</summary>
    public TuringMachineTape(params TSymbol?[] initialCells) 
        : this((IEnumerable<TSymbol?>)initialCells)
    {
    }
    
    /// <summary>Create new tape from given cell values.</summary>
    public TuringMachineTape(IEnumerable<TSymbol?> initialCells) 
        : this(0, InitTape(initialCells), default)
    {
    }
    
    /// <summary>Create new tape from given cell values. Initial cell will be set at given index.</summary>
    public TuringMachineTape(int currentCellIndex, IEnumerable<TSymbol?> initialCells) 
        : this(currentCellIndex, InitTape(initialCells), default)
    {
    }
    
    /// <summary>Create new tape from given cell values.</summary>
    /// <remarks>This constructor allows to select special value for the empty cells instead of <c>null</c>.</remarks>
    public TuringMachineTape(IEnumerable<TSymbol?> initialCells, TSymbol emptyCellValue) 
        : this(0, InitTape(initialCells, emptyCellValue), emptyCellValue)
    {
    }
    
    /// <summary>Create new tape from given cell values. Initial cell will be set at given index.</summary>
    /// <remarks>This constructor allows to select special value for the empty cells instead of <c>null</c>.</remarks>
    public TuringMachineTape(int currentCellIndex, IEnumerable<TSymbol?> initialCells, TSymbol emptyCellValue) 
        : this(currentCellIndex, InitTape(initialCells, emptyCellValue), emptyCellValue)
    {
    }
    
    private TuringMachineTape(int currentCellIndex, IEnumerable<TapeSymbol<TSymbol>> initialCells, TSymbol? emptyCell)
    {
        ArgumentNullException.ThrowIfNull(initialCells);
        ArgumentOutOfRangeException.ThrowIfNegative(currentCellIndex, nameof(currentCellIndex));
        
        emptyCellValue = emptyCell;
        cells = new LinkedList<TapeSymbol<TSymbol>>(initialCells);
        if (cells.Count == 0) 
            cells.AddFirst(TapeSymbol<TSymbol>.Empty(emptyCellValue));
        
        if (currentCellIndex >= cells.Count)
            throw new ArgumentOutOfRangeException(nameof(currentCellIndex), $"{nameof(currentCellIndex)} is outside the range of available cells.");
        
        currentCell = currentCellIndex == 0 
            ? cells.First! 
            : GetNthCell(currentCellIndex);

        filledCellsCount = cells.Count(c => !c.IsEmpty);
    }
    
    
    /// <inheritdoc/>
    public void MoveHead(TapeMovement movement)
    {
        switch (movement)
        {
            case TapeMovement.Left:
            {
                if (currentCell.Previous == null) 
                    cells.AddFirst(TapeSymbol<TSymbol>.Empty(emptyCellValue, currentCell.Value.Position - 1));
                currentCell = currentCell.Previous!;
                break;
            }
            case TapeMovement.Right:
            {
                if (currentCell.Next == null) 
                    cells.AddLast(TapeSymbol<TSymbol>.Empty(emptyCellValue, currentCell.Value.Position + 1));
                currentCell = currentCell.Next!;
                break;
            }
        }
    }

    /// <inheritdoc/>
    public void PutSymbol(TSymbol symbol)
    {
        var isEmpty = (emptyCellValue is null && symbol is null) || (emptyCellValue is not null && emptyCellValue.Equals(symbol));

        if (isEmpty && !currentCell.Value.IsEmpty)
            filledCellsCount--;
        else if (!isEmpty && currentCell.Value.IsEmpty)
            filledCellsCount++;
        
        currentCell.Value = new TapeSymbol<TSymbol>(symbol, isEmpty, currentCell.Value.Position);
    }

    /// <inheritdoc/>
    public void EraseSymbol() 
    {
        if (!currentCell.Value.IsEmpty)
            filledCellsCount--;
        currentCell.Value = TapeSymbol<TSymbol>.Empty(emptyCellValue, currentCell.Value.Position);
    }

    /// <inheritdoc/>
    public bool ReadSymbol([NotNullWhen(true)] out TSymbol? symbol)
    {
        if (currentCell.Value.IsEmpty)
        {
            symbol = emptyCellValue;
            return false;
        }

        symbol = currentCell.Value.Symbol!;
        return true;
    }

    /// <inheritdoc/>
    public TapeSymbol<TSymbol> ReadSymbol()
    {
        return currentCell.Value;
    }

    /// <inheritdoc/>
    public IEnumerable<TSymbol?> EnumerateCells(bool trimEmptyCells = true)
    {
        return trimEmptyCells
            ? EnumerateCellsTrimmed()
            : cells.Select(x => x.Symbol);
    }

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
            currentCell = cells.AddFirst(TapeSymbol<TSymbol>.Empty(emptyCellValue));
    }

    private IEnumerable<TSymbol?> EnumerateCellsTrimmed()
    {
        var first = FindFirstFilledCell();
        var last = FindLastFilledCell();

        while (first != null && !first.Equals(last))
        {
            yield return first.Value.Symbol;
            first = first.Next;
        }
        
        if (last != null)
            yield return last.Value.Symbol;
    }
    
    private LinkedListNode<TapeSymbol<TSymbol>>? FindFirstFilledCell()
    {
        var firstFilled = cells.First!;
        while (firstFilled is { Value.IsEmpty: true })
            firstFilled = firstFilled.Next;
        return firstFilled;
    }
    
    private LinkedListNode<TapeSymbol<TSymbol>>? FindLastFilledCell()
    {
        var lastFilled = cells.Last!;
        while (lastFilled is { Value.IsEmpty: true })
            lastFilled = lastFilled.Previous;
        return lastFilled;
    }

    private LinkedListNode<TapeSymbol<TSymbol>> GetNthCell(int n)
    {
        var current = cells.First!;
        while (current.Next != null && n-- > 0)
            current = current.Next;
        return current;
    }
    
    private static IEnumerable<TapeSymbol<TSymbol>> InitTape(IEnumerable<TSymbol?> initialCells)
    {
        return initialCells.Select((c, index) => new TapeSymbol<TSymbol>(c, c is null, index));
    }
    
    private static IEnumerable<TapeSymbol<TSymbol>> InitTape(IEnumerable<TSymbol?> initialCells, TSymbol emptyCellValue)
    {
        return initialCells.Select((c, index) => new TapeSymbol<TSymbol>(c, emptyCellValue.Equals(c), index));
    }
}