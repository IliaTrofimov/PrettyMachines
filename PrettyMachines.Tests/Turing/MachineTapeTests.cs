using PrettyMachines.Algorithms.Turing;


namespace PrettyMachines.Tests.Turing;

public class MachineTapeTests
{
    private const string DefaultBlank = "_";

    [Fact]
    public void Constructor_WithNullInitialCells_ThrowsArgumentNullException()
    {
        Action act = () => new MachineTape((IEnumerable<string?>)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNoCells_CreatesTapeWithSingleBlank()
    {
        var tape = new MachineTape([], DefaultBlank);
        
        tape.Length.Should().Be(1);
        tape.CurrentSymbol.Should().Be(DefaultBlank);
        tape.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithCells_CreatesTapeCorrectly()
    {
        var tape = new MachineTape(["a", "b", "c"], DefaultBlank);
        
        tape.Length.Should().Be(3);
        tape.CurrentSymbol.Should().Be("a");
        tape.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithBlankSymbol_SetsBlankSymbolCorrectly()
    {
        var blank = "#";
        var tape = new MachineTape(["a", blank, "c"]) { BlankSymbol = blank };
        
        tape.BlankSymbol.Should().Be(blank);
        tape.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void MoveHead_Left_AtStart_ExtendsTapeLeft()
    {
        var tape = new MachineTape(["a", "b"], DefaultBlank);
        
        tape.MoveHead(TapeMovement.Left);
        
        tape.Length.Should().Be(3);
        tape.CurrentSymbol.Should().Be(DefaultBlank);
    }

    [Fact]
    public void MoveHead_Left_FromMiddle_MovesCorrectly()
    {
        var tape = new MachineTape(["a", "b", "c"], DefaultBlank);
        
        tape.MoveHead(TapeMovement.Right); // Move to "b"
        tape.MoveHead(TapeMovement.Left);
        
        tape.CurrentSymbol.Should().Be("a");
        tape.Length.Should().Be(3);
    }

    [Fact]
    public void MoveHead_Right_AtEnd_ExtendsTapeRight()
    {
        var tape = new MachineTape(["a", "b"], DefaultBlank);
        tape.MoveHead(TapeMovement.Right); // Move to "b"
        
        tape.MoveHead(TapeMovement.Right);
        
        tape.Length.Should().Be(3);
        tape.CurrentSymbol.Should().Be(DefaultBlank);
    }

    [Fact]
    public void MoveHead_Right_FromMiddle_MovesCorrectly()
    {
        var tape = new MachineTape(["a", "b", "c"], DefaultBlank);
        
        tape.MoveHead(TapeMovement.Right);
        
        tape.CurrentSymbol.Should().Be("b");
    }

    [Fact]
    public void PutSymbol_OnEmptyCell_WritesSymbol()
    {
        var tape = new MachineTape([DefaultBlank, "b"], DefaultBlank);
        tape.MoveHead(TapeMovement.Left); // Move to blank
        tape.PutSymbol("x");
        
        tape.CurrentSymbol.Should().Be("x");
        tape.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PutSymbol_OnFilledCell_OverwritesSymbol()
    {
        var tape = new MachineTape(["a", "b"], DefaultBlank);
        tape.PutSymbol("x");
        
        tape.CurrentSymbol.Should().Be("x");
        tape.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void PutSymbol_WritingToBlank_IncreasesFilledCellCount()
    {
        var tape = new MachineTape([DefaultBlank, DefaultBlank], DefaultBlank);
        tape.IsEmpty.Should().BeTrue();
        
        tape.PutSymbol("x");
        
        tape.CurrentSymbol.Should().Be("x");
        tape.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void EnumerateCells_WithTrimEmptyCells_ReturnsTrimmedSequence()
    {
        var tape = new MachineTape([DefaultBlank, DefaultBlank, "a", "b", DefaultBlank], DefaultBlank);
        
        var result = tape.EnumerateCells(trimEmptyCells: true).ToList();
        
        result.Should().HaveCount(2);
        result[0].Should().Be("a");
        result[1].Should().Be("b");
    }

    [Fact]
    public void EnumerateCells_WithoutTrimEmptyCells_ReturnsAllCells()
    {
        var tape = new MachineTape([DefaultBlank, "a", "b", DefaultBlank], DefaultBlank);
        
        var result = tape.EnumerateCells(trimEmptyCells: false).ToList();
        
        result.Should().HaveCount(4);
        result[0].Should().Be(DefaultBlank);
        result[1].Should().Be("a");
        result[2].Should().Be("b");
        result[3].Should().Be(DefaultBlank);
    }

    [Fact]
    public void TrimEmptyCells_RemovesLeadingAndTrailingBlanks()
    {
        var tape = new MachineTape([DefaultBlank, DefaultBlank, "a", "b", DefaultBlank, DefaultBlank], DefaultBlank);
        tape.TrimEmptyCells();
        
        var result = tape.EnumerateCells(trimEmptyCells: false).ToList();
        
        result.Should().HaveCount(2);
        result[0].Should().Be("a");
        result[1].Should().Be("b");
    }

    [Fact]
    public void TrimEmptyCells_OnAllBlankTape_LeavesSingleBlank()
    {
        var tape = new MachineTape([DefaultBlank, DefaultBlank, DefaultBlank], DefaultBlank);
        tape.TrimEmptyCells();
        
        tape.Length.Should().Be(1);
        tape.CurrentSymbol.Should().Be(DefaultBlank);
        tape.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void TrimEmptyCells_OnTapeWithNoBlanks_DoesNothing()
    {
        var tape = new MachineTape(["a", "b", "c"], DefaultBlank);
        tape.TrimEmptyCells();
        
        tape.Length.Should().Be(3);
        tape.CurrentSymbol.Should().Be("a");
    }

    [Fact]
    public void GetEnumerator_ReturnsTrimmedSequence()
    {
        var tape = new MachineTape([DefaultBlank, "x", "y", DefaultBlank], DefaultBlank);
        
        var result = tape.ToList();
        
        result.Should().HaveCount(2);
        result[0].Should().Be("x");
        result[1].Should().Be("y");
    }

    [Fact]
    public void IsEmpty_WithOnlyBlanks_ReturnsTrue()
    {
        var tape = new MachineTape([DefaultBlank, DefaultBlank], DefaultBlank) { BlankSymbol = DefaultBlank };
        
        tape.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WithAtLeastOneNonBlank_ReturnsFalse()
    {
        var tape = new MachineTape([DefaultBlank, "a", DefaultBlank], DefaultBlank);
        
        tape.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Length_ReflectsCurrentTapeSizeAfterMoves()
    {
        var tape = new MachineTape(["a"], DefaultBlank);
        
        tape.MoveHead(TapeMovement.Left);
        tape.Length.Should().Be(2);
        
        tape.MoveHead(TapeMovement.Right);
        tape.MoveHead(TapeMovement.Right);
        tape.Length.Should().Be(3);
    }

    [Fact]
    public void CurrentSymbol_ReturnsCorrectValueAfterMoves()
    {
        var tape = new MachineTape(["a", "b", "c"], DefaultBlank);
        
        tape.CurrentSymbol.Should().Be("a");
        
        tape.MoveHead(TapeMovement.Right);
        tape.CurrentSymbol.Should().Be("b");
        
        tape.MoveHead(TapeMovement.Right);
        tape.CurrentSymbol.Should().Be("c");
    }

    [Fact]
    public void IsCurrentEmpty_ReturnsTrueForBlankCell()
    {
        var tape = new MachineTape([DefaultBlank, "a"], DefaultBlank);
        
        tape.IsCurrentEmpty.Should().BeTrue("first cell is empty");
        
        tape.MoveHead(TapeMovement.Right);
        tape.IsCurrentEmpty.Should().BeFalse("second cell is not empty");
    }
}