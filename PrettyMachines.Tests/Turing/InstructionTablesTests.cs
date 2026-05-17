using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Turing;


namespace PrettyMachines.Tests.Turing;

public class InstructionsTableTests
{
    private const string DefaultBlank = "_";
    
    #region Constructor Tests

    [Fact]
    public void Constructor_WithAlphabetSymbols()
    {
        var alphabet = new[] { "0", "1" };
        var markers = new[] { "$", "#" };
        var table = new InstructionsTable(alphabet, markers, DefaultBlank);
        
        table.Alphabet.Should().Contain(alphabet);
        table.Alphabet.Should().Contain(DefaultBlank);
        table.Alphabet.Should().HaveCount(3);
        table.Markers.Should().BeEquivalentTo(markers);
        table.RulesCount.Should().Be(0);
        table.BlankSymbol.Should().Be(DefaultBlank);
    }

    [Fact]
    public void Constructor_WithEmptyAlphabet_ThrowsArgumentException()
    {
        Action act = () => new InstructionsTable(Array.Empty<string>(), null, DefaultBlank);
        
        act.Should().Throw<ArgumentException>().WithMessage("*Alphabet cannot be empty*");
    }

    [Fact]
    public void Constructor_WithBlankSymbolOnly_CreatesUnrestrictedAlphabet()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        
        table.Alphabet.Should().Contain(DefaultBlank);
        table.Alphabet.Should().HaveCount(1);
        table.Markers.Should().BeNull();
        table.BlankSymbol.Should().Be(DefaultBlank);
    }
    
    [Fact]
    public void Constructor_WithStringComparison_UsesSpecifiedComparer()
    {
        var alphabet = new[] { "A", "b" };
        var table = new InstructionsTable(alphabet, StringComparison.OrdinalIgnoreCase);
        
        table.Alphabet.Should().Contain("A");
        table.Alphabet.Should().Contain("a");
    }

    [Fact]
    public void CopyConstructor_CreatesDeepCopy()
    {
        var original = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        var action = new TuringMachineAction(TuringMachineState.Halt, "1", TapeMovement.Right);
        original.AddRule(state, FuzzyKey<string>.Exact("0"), action);
        
        var copy = new InstructionsTable(original);
        
        copy.RulesCount.Should().Be(original.RulesCount);
        copy.States.Should().BeEquivalentTo(original.States);
        
        // Modify original, copy should remain unchanged
        original.AddRule(state, FuzzyKey<string>.Exact("1"), action);
        copy.RulesCount.Should().Be(1);
    }

    #endregion

    #region AddState Tests

    [Fact]
    public void AddState_WithNewState_AddsStateToTable()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(1, "q1");
        
        table.AddState(state);
        
        table.States.Should().Contain(state);
        table.RulesCount.Should().Be(0);
    }

    [Fact]
    public void AddState_WithExistingState_DoesNotDuplicate()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state1 = new TuringMachineState(1, "q1");
        var state2 = new TuringMachineState(1, "q1*", true);

        table.AddState(state1);
        table.AddState(state1);
        table.AddState(state2);

        table.States.Should()
            .HaveCount(1)
            .And.ContainSingle(s => s.Name == "q1");
    }

    #endregion

    #region AddRule Tests

    [Fact]
    public void AddRule_WithExactMatch_AddsRule()
    {
        var table = new InstructionsTable(["0", "1"], null, DefaultBlank);
        var initialState = new TuringMachineState(0, "q0");
        var nextState = new TuringMachineState(1, "q1");
        var action = new TuringMachineAction(nextState, "1", TapeMovement.Right);
        
        table.AddRule(initialState, FuzzyKey<string>.Exact("0"), action);
        
        table.RulesCount.Should().Be(1);
        table.States.Should().Contain(initialState);
    }

    [Theory]
    [InlineData(SymbolMatch.NotEmpty)]
    [InlineData(SymbolMatch.Empty)]
    [InlineData(SymbolMatch.Any)]
    public void AddRule_WithFuzzyMatch_AddsRule(SymbolMatch match)
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var initialState = new TuringMachineState(0, "q0");
        
        table.AddRule(initialState, new FuzzyKey<string>(null, match), TuringMachineAction.Halt);
        
        table.RulesCount.Should().Be(1);
    }

    [Fact]
    public void AddRule_WithTerminalInitialState_ThrowsAlgorithmException()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var terminalState = new TuringMachineState(1, "q_term", isTerminal: true);
        
        Action act = () => table.AddRule(terminalState, FuzzyKey<string>.Any, TuringMachineAction.Halt);
        
        act.Should().Throw<AlgorithmException>()
           .WithMessage("*initial state must not be terminal*");
    }

    [Fact]
    public void AddRule_WithDisallowedScannedSymbol_ThrowsSymbolIsNotAllowedException()
    {
        var table = new InstructionsTable(["0", "1"], null, DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        
        Action act = () => table.AddRule(state, FuzzyKey<string>.Exact("2"), TuringMachineAction.Halt);
        
        act.Should().Throw<SymbolIsNotAllowedException>()
           .WithMessage("*invalid scanned symbol*");
    }

    [Fact]
    public void AddRule_WithDisallowedPrintedSymbol_ThrowsSymbolIsNotAllowedException()
    {
        var table = new InstructionsTable(["0", "1"], null, DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        
        Action act = () => table.AddRule(state, FuzzyKey<string>.Exact("0"), TuringMachineAction.CreateHalt("2"));
        
        act.Should().Throw<SymbolIsNotAllowedException>()
           .WithMessage("*invalid printed symbol*");
    }

    [Fact]
    public void AddRule_WithAllowedMarkerSymbol_DoesNotThrow()
    {
        var table = new InstructionsTable(["0", "1"], [ "#" ], DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        var action = TuringMachineAction.CreateHalt("#");
        
        Action act = () => table.AddRule(state, FuzzyKey<string>.Exact("0"), action);
        
        act.Should().NotThrow();
    }

    [Fact]
    public void AddRule_WithAutoAlphabet_AutomaticallyAddsNewSymbols()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        var action = TuringMachineAction.CreateHalt("1");
        
        table.AddRule(state, FuzzyKey<string>.Exact("0"), action);
        
        table.Alphabet.Should().Contain("0");
        table.Alphabet.Should().Contain("1");
    }

    [Fact]
    public void AddRule_OverridesExistingRule()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        var action1 = new TuringMachineAction(new TuringMachineState(1, "q1"), "1", TapeMovement.Right);
        var action2 = new TuringMachineAction(new TuringMachineState(2, "q2"), "x", TapeMovement.Left);
        
        table.AddRule(state, FuzzyKey<string>.Exact("0"), action1);
        table.AddRule(state, FuzzyKey<string>.Exact("0"), action2);
        
        table.RulesCount.Should().Be(1);
        
        table.TryFindRule(state, "0", out var foundAction).Should().BeTrue();
        foundAction.PrintedSymbol.Should().Be("x");
    }

    #endregion

    #region TryFindRule Tests

    [Fact]
    public void TryFindRule_WithExactMatch_ReturnsTrueAndAction()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        var expectedAction = TuringMachineAction.CreateHalt("1");
        table.AddRule(state, FuzzyKey<string>.Exact("0"), expectedAction);
        
        var result = table.TryFindRule(state, "0", out var action);
        
        result.Should().BeTrue();
        action.Should().Be(expectedAction);
    }

    [Fact]
    public void TryFindRule_WithEmptySymbolMatch_ReturnsAction()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        var expectedAction = TuringMachineAction.CreateHalt("1");
        table.AddRule(state, FuzzyKey<string>.Empty, expectedAction);
        
        var result = table.TryFindRule(state, DefaultBlank, out var action);
        
        result.Should().BeTrue();
        action.Should().Be(expectedAction);
    }

    [Fact]
    public void TryFindRule_WithNotEmptySymbolMatch_ReturnsAction()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        var expectedAction = TuringMachineAction.CreateHalt("1");
        table.AddRule(state, FuzzyKey<string>.NotEmpty, expectedAction);
        
        var result = table.TryFindRule(state, "any_non_blank", out var action);
        
        result.Should().BeTrue();
        action.Should().Be(expectedAction);
    }

    [Fact]
    public void TryFindRule_WithAnySymbolMatch_ReturnsActionWhenNoExactMatch()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        var expectedAction = TuringMachineAction.CreateHalt("1");
        table.AddRule(state, FuzzyKey<string>.Any, expectedAction);
        
        var result = table.TryFindRule(state, "X", out var action);
        
        result.Should().BeTrue();
        action.Should().Be(expectedAction);
    }

    [Fact]
    public void TryFindRule_WhenNoRuleExists_ReturnsFalseAndHaltAction()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        
        var result = table.TryFindRule(state, "0", out var action);
        
        result.Should().BeFalse();
        action.Should().Be(TuringMachineAction.Halt);
    }

    [Fact]
    public void TryFindRule_WithStateNotFound_ReturnsFalseAndHaltAction()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        var otherState = new TuringMachineState(1, "q1");
        var action = TuringMachineAction.CreateHalt("1");
        table.AddRule(state, FuzzyKey<string>.Exact("0"), action);
        
        var result = table.TryFindRule(otherState, "0", out var foundAction);
        
        result.Should().BeFalse();
        foundAction.Should().Be(TuringMachineAction.Halt);
    }

    [Fact]
    public void TryFindRule_PrioritizesExactMatchOverFuzzy()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        var exactAction = TuringMachineAction.CreateHalt("X"); 
        var anyAction = TuringMachineAction.CreateHalt("Y"); 
        
        table.AddRule(state, FuzzyKey<string>.Exact("0"), exactAction);
        table.AddRule(state, FuzzyKey<string>.Any, anyAction);
        
        var result = table.TryFindRule(state, "0", out var action);
        
        result.Should().BeTrue();
        action.Should().Be(exactAction);
    }

    #endregion

    #region Enumerator Tests

    [Fact]
    public void GetEnumerator_ReturnsAllInstructions()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state1 = new TuringMachineState(0, "q0");
        var state2 = new TuringMachineState(1, "q1");
        var action1 = TuringMachineAction.CreateHalt("1");
        var action2 = TuringMachineAction.CreateHalt("0");
        
        table.AddRule(state1, FuzzyKey<string>.Exact("0"), action1);
        table.AddRule(state2, FuzzyKey<string>.Exact("1"), action2);
        
        var instructions = table.ToList();
        
        instructions.Should().HaveCount(2);
        instructions.Should().Contain(i => i.InitialState == state1);
        instructions.Should().Contain(i => i.InitialState == state2);
    }

    #endregion

    #region Properties Tests

    [Fact]
    public void States_ReturnsAllDefinedStates()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state1 = new TuringMachineState(0, "q0");
        var state2 = new TuringMachineState(1, "q1");
        var action = TuringMachineAction.CreateHalt("1");
        
        table.AddRule(state1, FuzzyKey<string>.Exact("0"), action);
        table.AddRule(state2, FuzzyKey<string>.Exact("1"), action);
        
        table.States.Should().Contain(state1);
        table.States.Should().Contain(state2);
    }

    [Fact]
    public void RulesCount_ReflectsNumberOfUniqueRules()
    {
        var table = new InstructionsTable(blankSymbol: DefaultBlank);
        var state = new TuringMachineState(0, "q0");
        var action = TuringMachineAction.CreateHalt("1");
        
        table.RulesCount.Should().Be(0);
        
        table.AddRule(state, FuzzyKey<string>.Exact("0"), action);
        table.RulesCount.Should().Be(1);
        
        // Override should not increase count
        table.AddRule(state, FuzzyKey<string>.Exact("0"), action);
        table.RulesCount.Should().Be(1);
        
        table.AddRule(state, FuzzyKey<string>.Exact("1"), action);
        table.RulesCount.Should().Be(2);
    }

    #endregion
}