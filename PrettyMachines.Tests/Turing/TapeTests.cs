using PrettyMachines.Implementations.Data;
using PrettyMachines.Implementations.Turing;


namespace PrettyMachines.Tests.Turing;

public class TapeTests
{
    [Fact]
    public void CreatingTape_CharTapeWithNoCells()
    {
        var expected = new [] { '_' };
        var tape = new TuringMachineTape<char>(Array.Empty<char>(), '_');

        var symbols = tape.EnumerateCells(false).ToArray();
        Assert.Equivalent(expected, symbols);
        
        var symbolsTrimmed = tape.EnumerateCells(true).ToArray();
        Assert.Empty(symbolsTrimmed);
    }
    
    [Fact]
    public void CreatingTape_CharTapeWithOnlyEmptyCells()
    {
        var expected = new [] { '_', '_', '_' };
        var tape = new TuringMachineTape<char>("___", '_');

        var symbols = tape.EnumerateCells(false).ToArray();
        Assert.Equivalent(expected, symbols);
        
        var symbolsTrimmed = tape.EnumerateCells(true).ToArray();
        Assert.Empty(symbolsTrimmed);
    }
    
    [Fact]
    public void CreatingTape_DefaultCharTape()
    {
        char[] expectedSymbols = ['a', 'b', 'c', 'd', 'e', 'f', 'g'];
        var tape1 = new TuringMachineTape<char>("abcdefg", '\0');
        var tape2 = new TuringMachineTape<char>(['a', 'b', 'c', 'd', 'e', 'f', 'g']);
        var tape3 = new TuringMachineTape<char>('a', 'b', 'c', 'd', 'e', 'f', 'g');

        var symbols1 = tape1.EnumerateCells(false);
        var symbols2 = tape2.EnumerateCells(false);
        var symbols3 = tape3.EnumerateCells(false);
        
        Assert.Equivalent(expectedSymbols, symbols1);
        Assert.Equivalent(expectedSymbols, symbols2);
        Assert.Equivalent(expectedSymbols, symbols3);
    }
    
    [Fact]
    public void CreatingTape_DefaultIntTape()
    {
        int[] expectedSymbols = [1, 2, 3, 4, 5];
        var tape1 = new TuringMachineTape<int>(new List<int> {1, 2, 3, 4, 5}, 0);
        var tape2 = new TuringMachineTape<int>(new List<int> {1, 2, 3, 4, 5});
        var tape3 = new TuringMachineTape<int>(1, 2, 3, 4, 5);

        var symbols1 = tape1.EnumerateCells(false);
        var symbols2 = tape2.EnumerateCells(false);
        var symbols3 = tape3.EnumerateCells(false);
        
        Assert.Equivalent(expectedSymbols, symbols1);
        Assert.Equivalent(expectedSymbols, symbols2);
        Assert.Equivalent(expectedSymbols, symbols3);
    }
    
    [Fact]
    public void CreatingTape_WithEmptyCells()
    {
        const string initialTape = "__hello_world___";
        const char emptyCell = '_';
        
        var tape = new TuringMachineTape<char>(initialTape, emptyCell);
        var symbols = tape.EnumerateCells(false).ToArray();
        var symbolsTrimmed = tape.EnumerateCells(true).ToArray();

        Assert.Equivalent(initialTape.ToCharArray(), symbols);
        Assert.Equivalent(initialTape.Trim(emptyCell).ToCharArray(), symbolsTrimmed);
    }
    
    [Fact]
    public void ReadSymbols_Right()
    {
        const string initialTape = "he_lo";
        const char emptyCell = '_';
        
        var tape = new TuringMachineTape<char>(initialTape, emptyCell);
        
        TestSymbol(tape, 'h', false, 0);

        tape.MoveHead(TapeMovement.Right);
        TestSymbol(tape, 'e', false, 1);

        tape.MoveHead(TapeMovement.Right);
        TestSymbol(tape, emptyCell, true, 2);
        
        tape.MoveHead(TapeMovement.Right);
        TestSymbol(tape, 'l', false, 3);
        
        tape.MoveHead(TapeMovement.Right);
        TestSymbol(tape, 'o', false, 4);
        
        tape.MoveHead(TapeMovement.Right);
        TestSymbol(tape, emptyCell, true, 5);
        
        tape.MoveHead(TapeMovement.Right);
        TestSymbol(tape, emptyCell, true, 6);
    }
    
    [Fact]
    public void ReadSymbols_Left()
    {
        const string initialTape = "he_lo";
        const char emptyCell = '_';
        
        var tape = new TuringMachineTape<char>(initialTape.Length - 1, initialTape, emptyCell);
        
        TestSymbol(tape, 'o', false, 4);
        
        tape.MoveHead(TapeMovement.Left);
        TestSymbol(tape, 'l', false, 3);
        
        tape.MoveHead(TapeMovement.Left);
        TestSymbol(tape, emptyCell, true, 2);
        
        tape.MoveHead(TapeMovement.Left);
        TestSymbol(tape, 'e', false, 1);
     
        tape.MoveHead(TapeMovement.Left);
        TestSymbol(tape, 'h', false, 0);
        
        tape.MoveHead(TapeMovement.Left);
        TestSymbol(tape, emptyCell, true, -1);
        
        tape.MoveHead(TapeMovement.Left);
        TestSymbol(tape, emptyCell, true, -2);
    }
    
    [Fact]
    public void PutSymbols_Default()
    {
        const string initialTape = "he_lo";
        const char emptyCell = '_';
        
        var tape = new TuringMachineTape<char>(2, initialTape, emptyCell);
        
        tape.PutSymbol('L');
        TestSymbol(tape, 'L', false, 2);
        
        tape.MoveHead(TapeMovement.Right);
        tape.PutSymbol('L');
        TestSymbol(tape, 'L', false, 3);
        
        tape.MoveHead(TapeMovement.Right); // o
        tape.MoveHead(TapeMovement.Right); // _
        tape.PutSymbol('!');
        TestSymbol(tape, '!', false, 6);
        
        tape.MoveHead(TapeMovement.Right); // _
        TestSymbol(tape, emptyCell, true, 7);
    }

    
    private record Letter(string Name, int Value);

    [Fact]
    public void NullableSymbolsTest()
    {
        var tape = new TuringMachineTape<Letter>(
            new Letter("A", 1),
            new Letter("B", 2), 
            null,
            new Letter("D", 4)
        );

        Assert.Equal(4, tape.Length);
        tape.MoveHead(2, TapeMovement.Right);
        
        TestSymbol(tape, null, true, 2);
        tape.MoveHead(TapeMovement.Right);
        
        TestSymbol(tape, new Letter("D", 4), false, 3);
    }
    
    
    [Fact]
    public void PutSymbols_Empty()
    {
        const string initialTape = "hello";
        const char emptyCell = '_';
        
        var tape = new TuringMachineTape<char>(3, initialTape, emptyCell);
        
        tape.PutSymbol('_');
        TestSymbol(tape, emptyCell, true, 3);
    }
    
    [Fact]
    public void EraseSymbols_Default()
    {
        const string initialTape = "he_lo";
        const char emptyCell = '_';
        
        var tape = new TuringMachineTape<char>(3, initialTape, emptyCell);
        
        tape.EraseSymbol();
        TestSymbol(tape, emptyCell, true, 3);
    }
    
    [Fact]
    public void TrimEmptySymbols_AllEmpty()
    {
        var tape = new TuringMachineTape<char>("___", '_');
        Assert.Equal(3, tape.Length);
        Assert.True(tape.IsEmpty, "Tape has only empty symbols");
        
        var symbolsOriginal = tape.EnumerateCells(false).ToArray();
        Assert.Equivalent(new[] {'_', '_', '_'}, symbolsOriginal);

        tape.TrimEmptyCells();
        var symbolsTrimmed = tape.EnumerateCells(false).ToArray();
        Assert.Equivalent(new[] {'_'}, symbolsTrimmed); // always 1 cell
        Assert.True(tape.IsEmpty, "Tape has only empty symbols");
    }
    
    [Fact]
    public void TrimEmptySymbols_AllFilled()
    {
        var tape = new TuringMachineTape<char>("123", '_');
        Assert.Equal(3, tape.Length);
        
        var symbolsOriginal = tape.EnumerateCells(false).ToArray();
        Assert.Equivalent(new[] {'1', '2', '3'}, symbolsOriginal);

        tape.TrimEmptyCells();
        var symbolsTrimmed = tape.EnumerateCells(false).ToArray();
        Assert.Equivalent(new[] {'1', '2', '3'}, symbolsTrimmed);
    }
    
    [Fact]
    public void TrimEmptySymbols_Default()
    {
        var tape = new TuringMachineTape<char>("___123___", '_');
        Assert.Equal(9, tape.Length);
        
        var symbolsOriginal = tape.EnumerateCells(false).ToArray();
        Assert.Equivalent(new[] {'_', '_', '_', '1', '2', '3', '_', '_', '_'}, symbolsOriginal);

        tape.TrimEmptyCells();
        var symbolsTrimmed = tape.EnumerateCells(false).ToArray();
        Assert.Equivalent(new[] {'1', '2', '3'}, symbolsTrimmed);
    }

    private static void TestSymbol<T>(IMachineTape<T> tape, T? expected, bool isEmpty, int index) where T : IEquatable<T>
    {
        T? s;
        if (isEmpty) Assert.False(tape.ReadSymbol(out s), $"symbol {index} should be empty");
        else Assert.True(tape.ReadSymbol(out s), $"symbol {index} should not be empty");
        Assert.Equal(expected, s);
    }
}