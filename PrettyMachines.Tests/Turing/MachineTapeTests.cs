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

    [Fact]
    public void Constructor_WithCells_StartsWithHeadAtIndexZero()
    {
        var tape = new MachineTape(["a", "b", "c"], DefaultBlank);
        
        tape.HeadIndex.Should().Be(0);
    }

    [Theory]
    [InlineData("R", 1)]
    [InlineData("RR", 2)]
    [InlineData("RRR", 3)]
    [InlineData("RL", 0)]
    [InlineData("RLR", 1)]
    [InlineData("LRLR", 1)]
    [InlineData("LLRR", 2)]
    public void MoveHead_UpdatesHeadIndex(string movements, int expectedHeadIndex)
    {
        var tape = new MachineTape(["a", "b", "c"], DefaultBlank);

        foreach (var movement in movements)
            tape.MoveHead(movement == 'L' ? TapeMovement.Left : TapeMovement.Right);

        tape.HeadIndex.Should().Be(expectedHeadIndex);
    }

    [Fact]
    public void MoveHead_LeftAtStart_KeepsHeadIndexAtZero()
    {
        var tape = new MachineTape(["a", "b"], DefaultBlank);
        
        tape.MoveHead(TapeMovement.Left);
        
        tape.Length.Should().Be(3);
        tape.HeadIndex.Should().Be(0);
        tape.CurrentSymbol.Should().Be(DefaultBlank);
    }

    [Fact]
    public void MoveHead_RightAtEnd_SetsHeadIndexToLastCell()
    {
        var tape = new MachineTape(["a", "b"], DefaultBlank);
        
        tape.MoveHead(TapeMovement.Right);
        tape.MoveHead(TapeMovement.Right);
        
        tape.HeadIndex.Should().Be(2);
        tape.HeadIndex.Should().Be(tape.Length - 1);
    }

    [Fact]
    public void MoveHead_ManyMoves_HeadIndexMatchesEnumeratedPosition()
    {
        var tape = new MachineTape(["a", "b", "c", "d"], DefaultBlank);
        var expectedIndex = 0;
        
        tape.MoveHead(TapeMovement.Right);
        expectedIndex++;
        tape.MoveHead(TapeMovement.Right);
        expectedIndex++;
        tape.MoveHead(TapeMovement.Left);
        expectedIndex--;
        tape.MoveHead(TapeMovement.Right);
        expectedIndex++;
        
        tape.EnumerateCells(trimEmptyCells: false).ElementAt(tape.HeadIndex).Should().Be(tape.CurrentSymbol);
        tape.HeadIndex.Should().Be(expectedIndex);
    }

    [Theory]
    [InlineData("R", 1)]
    [InlineData("RR", 2)]
    [InlineData("RL", 0)]
    [InlineData("L", 0)]
    public void CopyConstructor_PreservesHeadIndex(string movements, int expectedHeadIndex)
    {
        var source = new MachineTape(["a", "b", "c"], DefaultBlank);

        foreach (var movement in movements)
            source.MoveHead(movement == 'L' ? TapeMovement.Left : TapeMovement.Right);

        var copy = new MachineTape(source);
        
        copy.HeadIndex.Should().Be(expectedHeadIndex);
        copy.HeadIndex.Should().Be(source.HeadIndex);
        copy.CurrentSymbol.Should().Be(source.CurrentSymbol);
        copy.Length.Should().Be(source.Length);
    }

    [Fact]
    public void Clone_PreservesHeadIndex()
    {
        var tape = new MachineTape(["a", "b", "c"], DefaultBlank);
        tape.MoveHead(TapeMovement.Right);
        tape.MoveHead(TapeMovement.Right);
        
        var clone = tape.Clone();
        
        clone.HeadIndex.Should().Be(2);
    }

    [Fact]
    public void TrimEmptyCells_WithHeadOnLeadingBlank_MovesHeadToFirstFilledCell()
    {
        var tape = new MachineTape([DefaultBlank, DefaultBlank, "a", "b"], DefaultBlank);
        tape.HeadIndex.Should().Be(0);
        
        tape.TrimEmptyCells();
        
        tape.Length.Should().Be(2);
        tape.HeadIndex.Should().Be(0);
        tape.CurrentSymbol.Should().Be("a");
    }

    [Fact]
    public void TrimEmptyCells_WithHeadOnTrailingBlank_MovesHeadToLastFilledCell()
    {
        var tape = new MachineTape(["a", "b", DefaultBlank, DefaultBlank], DefaultBlank);
        tape.MoveHead(TapeMovement.Right);
        tape.MoveHead(TapeMovement.Right);
        tape.HeadIndex.Should().Be(2);
        
        tape.TrimEmptyCells();
        
        tape.Length.Should().Be(2);
        tape.HeadIndex.Should().Be(1);
        tape.CurrentSymbol.Should().Be("b");
    }

    [Fact]
    public void TrimEmptyCells_WithHeadOnFilledCell_ShiftsHeadIndexByRemovedLeadingBlanks()
    {
        var tape = new MachineTape([DefaultBlank, "a", DefaultBlank, "b", DefaultBlank], DefaultBlank);
        tape.MoveHead(TapeMovement.Right);
        tape.MoveHead(TapeMovement.Right);
        tape.MoveHead(TapeMovement.Right);
        tape.CurrentSymbol.Should().Be("b");
        
        tape.TrimEmptyCells();
        
        tape.EnumerateCells(trimEmptyCells: false).Should().Equal("a", DefaultBlank, "b");
        tape.HeadIndex.Should().Be(2);
        tape.CurrentSymbol.Should().Be("b");
    }

    [Fact]
    public void TrimEmptyCells_OnAllBlankTape_ResetsHeadIndexToZero()
    {
        var tape = new MachineTape([DefaultBlank, DefaultBlank, DefaultBlank], DefaultBlank);
        tape.MoveHead(TapeMovement.Right);
        tape.HeadIndex.Should().Be(1);
        
        tape.TrimEmptyCells();
        
        tape.Length.Should().Be(1);
        tape.HeadIndex.Should().Be(0);
        tape.CurrentSymbol.Should().Be(DefaultBlank);
    }

    [Fact]
    public void TrimEmptyCells_ThenMoveHead_KeepsHeadIndexConsistent()
    {
        var tape = new MachineTape([DefaultBlank, "a", "b", DefaultBlank], DefaultBlank);
        
        tape.TrimEmptyCells();
        tape.MoveHead(TapeMovement.Right);
        
        tape.HeadIndex.Should().Be(1);
        tape.CurrentSymbol.Should().Be("b");
    }
}