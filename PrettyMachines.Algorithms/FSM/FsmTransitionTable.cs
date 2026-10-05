using PrettyMachines.Automata;


namespace PrettyMachines.FSM;

/// <summary>
/// Ordered set of transitions that define a finite state machine.
/// </summary>
public sealed class FsmTransitionTable : TransitionTable<AutomatonState, char, FsmAction>
{
    /// <summary>Gets the ordered list of defined transitions.</summary>
    public IReadOnlyList<AutomatonInstruction<AutomatonState, char, FsmAction>> Rules => Transitions;

    /// <summary>Gets the action returned when no transition matches.</summary>
    protected override FsmAction DefaultAction => default;

    /// <summary>Finite state machines allow transitions from accepting states.</summary>
    protected override bool AllowTerminalInitialState => true;


    /// <summary>Initializes a new transition table with the given set of allowed symbols.</summary>
    /// <param name="alphabetSymbols">Alphabet that defines the set of allowed symbols.</param>
    public FsmTransitionTable(IEnumerable<char> alphabetSymbols)
        : base(alphabetSymbols, default(char), EqualityComparer<char>.Default)
    {
    }

    /// <summary>Initializes a new transition table with an unrestricted alphabet.</summary>
    public FsmTransitionTable()
        : base(null, default(char), EqualityComparer<char>.Default)
    {
    }

    /// <summary>Creates a deep copy of another transition table.</summary>
    /// <param name="other">The transitions to copy.</param>
    public FsmTransitionTable(FsmTransitionTable other) : base(other)
    {
    }
}
