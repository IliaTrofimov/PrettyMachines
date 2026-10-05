using PrettyMachines.Abstract;
using PrettyMachines.Automata;
using PrettyMachines.FSM;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.FSM;

public class FiniteStateMachineTests(ITestOutputHelper output)
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullTransitions_ThrowsArgumentNullException()
    {
        Action act = () => new FiniteStateMachine(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNoStates_ThrowsArgumentException()
    {
        Action act = () => new FiniteStateMachine(new FsmTransitionTable());

        act.Should().Throw<ArgumentException>().WithMessage("*at least 1 state*");
    }

    [Fact]
    public void Constructor_WithSpecificInitialState_SetsInitialState()
    {
        var table = new FsmTransitionTable();
        var q0 = new AutomatonState(0, "q0");
        var q1 = new AutomatonState(1, "q1", true);
        table.AddRule(q0, FuzzyKey<char>.Exact('a'), new FsmAction(q1));

        var machine = new FiniteStateMachine(table, initialState: q0);

        machine.InitialState.Should().BeSameAs(q0);
    }

    [Fact]
    public void Constructor_WithoutInitialState_UsesFirstStateFromTransitions()
    {
        var table = new FsmTransitionTable();
        var q0 = new AutomatonState(0, "q0");
        var q1 = new AutomatonState(1, "q1", true);
        table.AddRule(q1, FuzzyKey<char>.Exact('b'), new FsmAction(q0));
        table.AddRule(q0, FuzzyKey<char>.Exact('a'), new FsmAction(q1));

        var machine = new FiniteStateMachine(table);

        machine.InitialState.Should().BeSameAs(q0);
    }

    [Fact]
    public void InitialState_SetToNonExistentState_ThrowsArgumentException()
    {
        var machine = CreateEvenOnesMachine();
        var missing = new AutomatonState(100, "missing");

        Action act = () => machine.InitialState = missing;

        act.Should().Throw<ArgumentException>()
            .WithMessage($"*Initial state '{missing}' does not exist*");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void ValidateInput_WithStrictAlphabet_RejectsUnknownSymbols()
    {
        var machine = CreateEvenOnesMachine();

        machine.ValidateInput("").Should().BeTrue();
        machine.ValidateInput("0110").Should().BeTrue();
        machine.ValidateInput("012").Should().BeFalse();
    }

    [Fact]
    public void ValidateInput_WithUnrestrictedAlphabet_AcceptsAnyInput()
    {
        var machine = CreateAnyMachine();

        machine.ValidateInput("anything goes here").Should().BeTrue();
        machine.ValidateInput("").Should().BeTrue();
    }

    #endregion

    #region Execution Tests

    [Theory]
    [InlineData("", TerminationStatus.Success, "A")]
    [InlineData("0", TerminationStatus.Success, "A")]
    [InlineData("11", TerminationStatus.Success, "A")]
    [InlineData("101", TerminationStatus.Success, "A")]
    [InlineData("1", TerminationStatus.Stuck, "R")]
    [InlineData("10", TerminationStatus.Stuck, "R")]
    [InlineData("100", TerminationStatus.Stuck, "R")]
    public void Execute_EvenNumberOfOnes_ReturnsExpectedResult(string input, TerminationStatus termination, string expectedOutput)
    {
        var machine = CreateEvenOnesMachine();

        var result = machine.Execute(input, AlgorithmCancellation.Default);

        output.WriteLine($"{input} -> {result.Output} ({result.Termination} after {result.Steps} steps)");
        result.Termination.Should().Be(termination);
        result.Output.Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("b", TerminationStatus.Success)]
    [InlineData("ab", TerminationStatus.Success)]
    [InlineData("aaab", TerminationStatus.Success)]
    [InlineData("", TerminationStatus.Stuck)]
    [InlineData("a", TerminationStatus.Stuck)]
    [InlineData("ba", TerminationStatus.Stuck)]
    [InlineData("abab", TerminationStatus.Stuck)]
    public void Execute_AnB_Recognizer_ReturnsExpectedTermination(string input, TerminationStatus termination)
    {
        var machine = FiniteStateMachine.Create("a^n b")
            .AddInitialState("scan", out var scan)
            .AddTerminalState("accept", out var accept)
            .BuildRules(rules => rules
                .AddRule(scan, 'a', scan)
                .AddRule(scan, 'b', accept));

        var result = machine.Execute(input, AlgorithmCancellation.Default);

        output.WriteLine($"{input} -> {result.Output} ({result.Termination} after {result.Steps} steps)");
        result.Termination.Should().Be(termination);
    }

    [Fact]
    public void Execute_WhenNoTransitionMatchesMidInput_ReturnsStuck()
    {
        var machine = FiniteStateMachine.Create()
            .AddInitialState(out var q0)
            .AddTerminalState(out var q1)
            .BuildRules(rules => rules.AddRule(q0, 'a', q1));

        var result = machine.Execute("ab", AlgorithmCancellation.Default);

        result.Termination.Should().Be(TerminationStatus.Stuck);
        result.Output.Should().Be("R");
    }

    [Fact]
    public void Execute_WithStrictAlphabetAndInvalidSymbol_ReturnsInvalidInput()
    {
        var machine = CreateEvenOnesMachine();

        var result = machine.Execute("012", AlgorithmCancellation.Default);

        result.Termination.Should().Be(TerminationStatus.InvalidInput);
        result.Output.Should().Be("R");
        result.Steps.Should().Be(0);
    }

    [Fact]
    public void Execute_ReportsAppliedTransitionIndices()
    {
        var machine = CreateEvenOnesMachine();

        var result = machine.Execute("11", AlgorithmCancellation.Default);

        result.Termination.Should().Be(TerminationStatus.Success);
        result.AppliedInstructions.Should().Equal(1, 3);
    }

    [Fact]
    public void Execute_WithVerboseMode_IncludesTraceInResult()
    {
        var machine = CreateEvenOnesMachine();

        var result = machine.Execute("11", AlgorithmCancellation.Default, verbose: true);

        output.WriteLine(string.Join(Environment.NewLine, result.Trace));
        result.Trace.Should().NotBeEmpty();
        result.Trace.Should().Contain(line => line.Contains("->"));
    }

    [Fact]
    public void Execute_WithCustomOutputTokens_UsesConfiguredTokens()
    {
        var machine = FiniteStateMachine.Create()
            .WithOutput("YES", "NO")
            .AddTerminalState(out var qEven)
            .AddState(out var qOdd)
            .BuildRules(rules => rules
                .AddRule(qEven, '0', qEven)
                .AddRule(qEven, '1', qOdd)
                .AddRule(qOdd, '0', qOdd)
                .AddRule(qOdd, '1', qEven));

        machine.Execute("11", AlgorithmCancellation.Default).Output.Should().Be("YES");
        machine.Execute("1", AlgorithmCancellation.Default).Output.Should().Be("NO");
    }

    [Fact]
    public void Execute_WhenCancelledBeforeCompletion_ReturnsAborted()
    {
        var machine = FiniteStateMachine.Create()
            .AddInitialState(out var q0)
            .AddTerminalState(out var q1)
            .BuildRules(rules => rules
                .AddRule(q0, SymbolMatch.Any, q0)
                .AddRule(q0, 'x', q1));

        var result = machine.Execute("aaaa", new AlgorithmCancellation(2), verbose: true);

        result.Termination.Should().Be(TerminationStatus.Aborted);
    }

    [Fact]
    public void Run_ProducesInitialSnapshotWithInitialState()
    {
        var machine = CreateEvenOnesMachine();

        var first = machine.Run("11", AlgorithmCancellation.Default).First();

        first.Steps.Should().Be(0);
        first.Termination.Should().Be(TerminationStatus.Unknown);
        first.Output.Should().Be(machine.InitialState.ToString());
    }

    #endregion

    #region Builder Tests

    [Fact]
    public void Builder_AddRuleByName_ResolvesStates()
    {
        var machine = FiniteStateMachine.Create("named")
            .AddInitialState("start", out _)
            .AddTerminalState("accept", out _)
            .BuildRules(rules => rules.AddRule("start", 'x', "accept"));

        var result = machine.Execute("x", AlgorithmCancellation.Default);

        result.Termination.Should().Be(TerminationStatus.Success);
        result.Output.Should().Be("A");
    }

    [Fact]
    public void Builder_WithSingleCharAlphabet_AddsEachCharacter()
    {
        var machine = FiniteStateMachine.Create()
            .WithSingleCharAlphabet("ab")
            .AddInitialState(out var q0)
            .AddTerminalState(out var q1)
            .BuildRules(rules => rules.AddRule(q0, 'a', q1));

        machine.ValidateInput("a").Should().BeTrue();
        machine.ValidateInput("c").Should().BeFalse();
    }

    [Fact]
    public void Builder_ExactTransitionHasPriorityOverAny()
    {
        var machine = FiniteStateMachine.Create()
            .AddInitialState("start", out var start)
            .AddTerminalState("accept", out var accept)
            .AddState("other", out var other)
            .BuildRules(rules => rules
                .AddRule(start, 'a', accept)
                .AddRule(start, SymbolMatch.Any, other));

        machine.Execute("a", AlgorithmCancellation.Default).Termination.Should().Be(TerminationStatus.Success);
        machine.Execute("b", AlgorithmCancellation.Default).Termination.Should().Be(TerminationStatus.Stuck);
    }

    [Theory]
    [InlineData(SymbolMatch.Empty)]
    [InlineData(SymbolMatch.NotEmpty)]
    public void Builder_RejectsEmptyAndNotEmptySymbolMatches(SymbolMatch match)
    {
        Action act = () => FiniteStateMachine.Create()
            .AddInitialState(out var q0)
            .AddTerminalState(out var q1)
            .BuildRules(rules => rules.AddRule(q0, match, q1));

        act.Should().Throw<AlgorithmException>()
            .WithMessage($"*'{match}'*");
    }

    [Fact]
    public void Builder_AddStateWithSameNameAndDifferentTerminalFlag_Throws()
    {
        Action act = () => FiniteStateMachine.Create()
            .AddState("q0", false, false, out _)
            .AddState("q0", false, true, out _);

        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region NextStep Tests

    [Fact]
    public void NextStep_WithMatchingAnyTransition_ReturnsTrue()
    {
        var machine = CreateAnyMachine();

        var found = machine.NextStep(machine.InitialState, 'z', out var action);

        found.Should().BeTrue();
        action.NextState.Should().BeSameAs(machine.Transitions[1].Action.NextState);
    }

    [Fact]
    public void NextStep_WithNoMatchingTransition_ReturnsFalse()
    {
        var machine = FiniteStateMachine.Create()
            .AddInitialState(out var q0)
            .AddTerminalState(out var q1)
            .BuildRules(rules => rules.AddRule(q0, 'a', q1));

        var found = machine.NextStep(machine.InitialState, 'b', out _);

        found.Should().BeFalse();
    }

    #endregion

    #region Helpers

    private static FiniteStateMachine CreateEvenOnesMachine()
    {
        return FiniteStateMachine.Create("Even number of ones")
            .WithAlphabet('0', '1')
            .AddTerminalState("even", out var qEven)
            .AddState("odd", out var qOdd)
            .BuildRules(rules => rules
                .AddRule(qEven, '0', qEven)
                .AddRule(qEven, '1', qOdd)
                .AddRule(qOdd, '0', qOdd)
                .AddRule(qOdd, '1', qEven));
    }

    private static FiniteStateMachine CreateAnyMachine()
    {
        return FiniteStateMachine.Create("Any")
            .AddInitialState("start", out var start)
            .AddTerminalState("accept", out var accept)
            .AddState("other", out var other)
            .BuildRules(rules => rules
                .AddRule(start, 'a', accept)
                .AddRule(start, SymbolMatch.Any, other)
                .AddRule(other, SymbolMatch.Any, other));
    }

    #endregion
}
