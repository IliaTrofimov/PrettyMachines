using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Turing;


namespace PrettyMachines.Tests.Turing;

public class TuringMachineTests
{
    
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesMachine()
    {
        var machine = new TuringMachine(CreateSimpleInstructionsTable());
        machine.Should().NotBeNull();
        machine.Name.Should().BeNull();
        machine.HasStrictAlphabet.Should().BeFalse();
        machine.InitialState.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithName_SetsNameProperty()
    {
        var machine = new TuringMachine("MyTM", CreateSimpleInstructionsTable());
        machine.Name.Should().Be("MyTM");
    }

    [Fact]
    public void Constructor_WithStrictAlphabet_SetsStrictAlphabetProperty()
    {
        var machine = new TuringMachine(CreateSimpleInstructionsTable(), strictAlphabet: true);
        machine.HasStrictAlphabet.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNullInstructions_ThrowsArgumentNullException()
    {
        Action act = () => new TuringMachine(null!);
        
        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void Constructor_WithNoInstructions_ThrowsArgumentException()
    {
        Action act = () => new TuringMachine(new InstructionsTable());
        
        act.Should().Throw<ArgumentException>().WithMessage("*at least 1 rule*");
    }

    [Fact]
    public void Constructor_WithSpecificInitialState_SetsInitialState()
    {
        var instructions = new InstructionsTable();
        var initialState = new TuringMachineState(0, "start");
        instructions.AddRule(initialState, FuzzyKey<string>.Any, TuringMachineAction.Halt);
        
        var machine = new TuringMachine(instructions, initialState: initialState);
        
        machine.InitialState.Should().Be(initialState);
    }

    [Fact]
    public void Constructor_WithoutInitialState_UsesFirstStateFromInstructions()
    {
        var instructions = new InstructionsTable();
        var state1 = new TuringMachineState(1, "first");
        var state2 = new TuringMachineState(2, "second");
        instructions.AddRule(state2, new FuzzyKey<string>("2"), new TuringMachineAction(state1, "1"));
        instructions.AddRule(state1, new FuzzyKey<string>("1"), new TuringMachineAction(state2, "2"));
        
        var machine = new TuringMachine(instructions);
        
        machine.InitialState.Should().Be(state1);
    }

    #endregion

    #region InitialState Property Tests

    [Fact]
    public void InitialState_SetToExistingState_UpdatesSuccessfully()
    {
        var instructions = new InstructionsTable();
        var state1 = new TuringMachineState(1, "first");
        var state2 = new TuringMachineState(2, "second");
        instructions.AddRule(state2, new FuzzyKey<string>("2"), new TuringMachineAction(state1, "1"));
        instructions.AddRule(state1, new FuzzyKey<string>("1"), new TuringMachineAction(state2, "2"));
        
        var machine = new TuringMachine(instructions, initialState: state1);
        
        machine.InitialState = state2;
        
        machine.InitialState.Should().Be(state2);
    }

    [Fact]
    public void InitialState_SetToNonExistentState_ThrowsArgumentException()
    {
        var instructions = CreateSimpleInstructionsTable();
        var machine = new TuringMachine(instructions);
        var nonExistentState = new TuringMachineState(100, "missing");
        
        Action act = () => machine.InitialState = nonExistentState;
        
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*Initial state '{nonExistentState}' does not exist*");
    }

    #endregion

    #region NextStep Tests

    [Fact]
    public void NextStep_WithValidRule_AppliesActionAndReturnsTrue()
    {
        var machine = TuringMachine.Create()
            .AddInitialState(out var q0)
            .AddTerminalState(out var q1)
            .BuildRules(builder => builder.AddRule(q0, "0", q1, "2"));
        
        var tape = new MachineTape("0");
        
        var result = machine.NextStep(q0, tape, out var action);
        
        result.Should().BeTrue();
        action.PrintedSymbol.Should().Be("2");
        tape.CurrentSymbol.Should().Be("2");
    }

    [Fact]
    public void NextStep_WithNoMatchingRule_ReturnsFalseAndActionIsHalt()
    {
        var machine = TuringMachine.Create()
            .AddInitialState(out var q0)
            .AddTerminalState(out var q1)
            .BuildRules(builder => builder.AddRule(q0, "xxxx", q1));
        
        var tape = new MachineTape("0");
        var result = machine.NextStep(q0, tape, out var action);
        
        result.Should().BeFalse();
        action.Should().Be(TuringMachineAction.Halt);
    }

    [Fact]
    public void NextStep_WithPrintNull_DoesNotModifyTape()
    {
        var machine = TuringMachine.Create()
            .AddInitialState(out var q0)
            .AddTerminalState(out var q1)
            .BuildRules(builder => builder.AddRule(q0, "0", q1, null, TapeMovement.None));
        
        var tape = new MachineTape("0");
        var originalSymbol = tape.CurrentSymbol;
        
        machine.NextStep(q0, tape, out _);
        
        tape.CurrentSymbol.Should().Be(originalSymbol);
    }

    #endregion
    
    #region Execute Tests (MachineTape Input)

    [Fact]
    public void Execute_WithTapeInput_ModifiesTapeAndReturnsResult()
    {
        var machine = CreateSimpleIncrementMachine();
        var tape = new MachineTape("0");
        
        var result = machine.Execute(tape, AlgorithmCancellation.Default);
        
        result.Termination.Should().Be(TerminationStatus.Success);
        result.Output.Should().BeSameAs(tape);
        result.Output.Should().BeEquivalentTo(["1"]);
    }

    [Fact]
    public void Execute_WithStrictAlphabetAndInvalidSymbol_ReturnsInvalidInput()
    {
        var machine = CreateSimpleIncrementMachine(strictAlphabet: true);
        var tape = new MachineTape("2");
        
        var result = machine.Execute(tape, AlgorithmCancellation.Default);
        
        result.Termination.Should().Be(TerminationStatus.InvalidInput);
        result.Output.Should().BeEquivalentTo(["2"]);
    }

    [Fact]
    public void Execute_WhenNoRuleMatches_ReturnsStuck()
    {
        var machine = CreateSimpleIncrementMachine(strictAlphabet: false);
        var tape = new MachineTape("2");
        
        var result = machine.Execute(tape, AlgorithmCancellation.Default);
        
        result.Termination.Should().Be(TerminationStatus.Stuck);
        result.Output.Should().BeEquivalentTo(["2"]);
    }

    [Fact]
    public void Execute_ReachesTerminalState_ReturnsSuccess()
    {
        var machine = TuringMachine.Create()
            .WithAlphabet(["x", "y", "z", "w"], "_")
            .AddStates("q1", "q2")
            .AddTerminalState("end")
            .BuildRules(builder => builder
                .AddRule("q1", "x", "q2", "y", TapeMovement.Right)
                .AddRule("q2", "y", "end", "z")
            );

        var tape = new MachineTape("x", "y", "z", "w");
        
        var result = machine.Execute(tape, AlgorithmCancellation.Default, verbose: false);
        
        result.Termination.Should().Be(TerminationStatus.Success);
        result.Output.Should().BeEquivalentTo(["y", "z", "z", "w"]);
        result.Trace.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Execute_WithVerboseMode_IncludesTraceInResult()
    {
        var machine = CreateSimpleIncrementMachine();
        var tape = new MachineTape("0");
        
        var result = machine.Execute(tape, AlgorithmCancellation.Default, verbose: true);
        
        result.Trace.Should().NotBeNull();
        result.Trace.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Helper Methods for Test Setup

    private static TuringMachine CreateSimpleIncrementMachine(bool strictAlphabet = false)
    {
        var builder = TuringMachine.Create("TestMachine");

        if (strictAlphabet)
            builder.WithAlphabet("0", "1");
        
        return builder
            .AddInitialState(out var q0)
            .AddState(out var q1)
            .BuildRules(rules => rules
                .AddRule(q0, "0", q1, "1", TapeMovement.Right)
                .AddHalt(q1, SymbolMatch.Any)
            );
    }
    
    private static InstructionsTable CreateSimpleInstructionsTable()
    {
        var table = new InstructionsTable();
        var q1 = new TuringMachineState(1);
        var q2 = new TuringMachineState(2);
        table.AddRule(q1, new FuzzyKey<string>("1"), new TuringMachineAction(q2, "2"));
        return table;
    }

    private static TuringMachine CreateInfiniteLoopMachine()
    {
        return TuringMachine.Create("InfiniteLoopMachine")
            .AddState("loop", out var q0)
            .BuildRules(builder => builder
                .AddRule(q0, SymbolMatch.Any, q0, null, TapeMovement.Right)
            );
    }

    #endregion
}