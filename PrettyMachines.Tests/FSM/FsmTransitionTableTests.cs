using PrettyMachines.Abstract;
using PrettyMachines.Automata;
using PrettyMachines.FSM;


namespace PrettyMachines.Tests.FSM;

public class FsmTransitionTableTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithAlphabetSymbols_CreatesTable()
    {
        var table = new FsmTransitionTable(['a', 'b']);

        table.Alphabet.Should().Contain('a').And.Contain('b');
        table.RulesCount.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithEmptyAlphabet_ThrowsArgumentException()
    {
        Action act = () => new FsmTransitionTable(Array.Empty<char>());

        act.Should().Throw<ArgumentException>().WithMessage("*Alphabet cannot be empty*");
    }

    [Fact]
    public void CopyConstructor_PreservesOrderAndRules()
    {
        var table = new FsmTransitionTable();
        var q0 = new AutomatonState(0, "q0");
        var q1 = new AutomatonState(1, "q1");
        table.AddRule(q1, FuzzyKey<char>.Exact('b'), new FsmAction(q0));
        table.AddRule(q0, FuzzyKey<char>.Exact('a'), new FsmAction(q1));

        var copy = new FsmTransitionTable(table);

        copy.RulesCount.Should().Be(2);
        copy.Rules.Select(r => r.InitialState.Name).Should().Equal("q1", "q0");
        copy.IndexOf(q0, 'a').Should().Be(1);
    }

    #endregion

    #region AddState Tests

    [Fact]
    public void AddState_WithExistingId_DoesNotDuplicate()
    {
        var table = new FsmTransitionTable();
        var first = new AutomatonState(0, "q0");
        var second = new AutomatonState(0, "q0*", true);

        table.AddState(first);
        table.AddState(second);

        table.States.Should().HaveCount(1).And.ContainSingle(s => s.Name == "q0");
    }

    #endregion

    #region AddRule Tests

    [Fact]
    public void AddRule_WithTerminalInitialState_IsAllowed()
    {
        var table = new FsmTransitionTable();
        var terminal = new AutomatonState(0, "q0", true);
        var next = new AutomatonState(1, "q1");

        Action act = () => table.AddRule(terminal, FuzzyKey<char>.Exact('a'), new FsmAction(next));

        act.Should().NotThrow();
    }

    [Fact]
    public void AddRule_WithDisallowedScannedSymbol_ThrowsSymbolIsNotAllowedException()
    {
        var table = new FsmTransitionTable(['a']);
        var state = new AutomatonState(0, "q0");
        var next = new AutomatonState(1, "q1", true);

        Action act = () => table.AddRule(state, FuzzyKey<char>.Exact('b'), new FsmAction(next));

        act.Should().Throw<SymbolIsNotAllowedException>().WithMessage("*invalid scanned symbol*");
    }

    [Fact]
    public void AddRule_WithAutoAlphabet_AddsExactSymbols()
    {
        var table = new FsmTransitionTable();
        var state = new AutomatonState(0, "q0");
        var next = new AutomatonState(1, "q1", true);

        table.AddRule(state, FuzzyKey<char>.Exact('a'), new FsmAction(next));

        table.Alphabet.Should().Contain('a');
    }

    [Fact]
    public void AddRule_OverridesExistingRuleInPlace()
    {
        var table = new FsmTransitionTable();
        var q0 = new AutomatonState(0, "q0");
        var q1 = new AutomatonState(1, "q1");
        var q2 = new AutomatonState(2, "q2");

        table.AddRule(q0, FuzzyKey<char>.Exact('a'), new FsmAction(q1));
        table.AddRule(q0, FuzzyKey<char>.Exact('a'), new FsmAction(q2));

        table.RulesCount.Should().Be(1);
        table.IndexOf(q0, 'a').Should().Be(0);
        table.TryFindAction(q0, 'a', out var action).Should().BeTrue();
        action.NextState.Should().BeSameAs(q2);
    }

    #endregion

    #region Lookup Tests

    [Fact]
    public void TryFindAction_WithAnyRule_FallsThroughWhenSymbolIsNotNull()
    {
        var table = new FsmTransitionTable();
        var q0 = new AutomatonState(0, "q0");
        var q1 = new AutomatonState(1, "q1");
        table.AddRule(q0, FuzzyKey<char>.Any, new FsmAction(q1));

        var found = table.TryFindAction(q0, 'z', out var action);

        found.Should().BeTrue();
        action.NextState.Should().BeSameAs(q1);
    }

    [Fact]
    public void TryFindAction_WhenNoRuleExists_ReturnsFalse()
    {
        var table = new FsmTransitionTable();
        var q0 = new AutomatonState(0, "q0");

        var found = table.TryFindAction(q0, 'z', out _);

        found.Should().BeFalse();
    }

    [Fact]
    public void IndexOf_PrioritizesExactOverAny()
    {
        var table = new FsmTransitionTable();
        var q0 = new AutomatonState(0, "q0");
        var q1 = new AutomatonState(1, "q1");
        table.AddRule(q0, FuzzyKey<char>.Any, new FsmAction(q1));
        table.AddRule(q0, FuzzyKey<char>.Exact('a'), new FsmAction(q1));

        table.IndexOf(q0, 'a').Should().Be(1);
        table.IndexOf(q0, 'z').Should().Be(0);
        table.IndexOf(q1, 'a').Should().Be(-1);
    }

    [Fact]
    public void Indexer_ReturnsTransitionAction()
    {
        var table = new FsmTransitionTable();
        var q0 = new AutomatonState(0, "q0");
        var q1 = new AutomatonState(1, "q1");
        table.AddRule(q0, FuzzyKey<char>.Exact('a'), new FsmAction(q1));

        table[q0, FuzzyKey<char>.Exact('a')]!.Value.NextState.Should().BeSameAs(q1);
        table[q0, FuzzyKey<char>.Exact('b')].Should().BeNull();
    }

    #endregion

    #region Comparer Tests

    [Fact]
    public void AutomatonStateComparer_ComparesById()
    {
        var a = new AutomatonState(7, "a");
        var b = new AutomatonState(7, "b");
        var c = new AutomatonState(8, "c");

        AutomatonStateComparer.Instance.Equals(a, b).Should().BeTrue();
        AutomatonStateComparer.Instance.Equals(a, c).Should().BeFalse();
        AutomatonStateComparer.Instance.GetHashCode(a).Should().Be(AutomatonStateComparer.Instance.GetHashCode(b));
    }

    #endregion
}
