using PrettyMachines.Automata;


namespace PrettyMachines.FSM;

/// <summary>Represents a single action that a finite state machine takes when executing a transition.</summary>
public readonly struct FsmAction
{
    /// <summary>Gets the state to transition to after this action.</summary>
    public AutomatonState NextState { get; }


    /// <summary>Initializes a new action that transitions into the given state.</summary>
    /// <param name="nextState">State to transition to after this action.</param>
    public FsmAction(AutomatonState nextState)
    {
        NextState = nextState;
    }


    /// <inheritdoc cref="object.ToString()"/>
    /// <inheritdoc cref="ToFormattedString(char)"/>
    public override string ToString() => ToFormattedString();

    /// <summary>Outputs string representation of this object with specified quoting character.</summary>
    /// <param name="quote">Character used to quote symbols. Unused for finite state machines.</param>
    /// <returns>The target state of this action.</returns>
    public string ToFormattedString(char quote = '\'')
    {
        return NextState?.ToString() ?? "?";
    }
}
