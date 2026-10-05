using PrettyMachines.Automata;


namespace PrettyMachines.Turing;

/// <summary>Turing machine's state.</summary>
public class TuringMachineState : AutomatonState
{
    /// <summary>Get the default terminating state.</summary>
    public static TuringMachineState Halt { get; } = new();
    

    private TuringMachineState() : base(int.MinValue, null, true)
    {
    }
    
    /// <summary>Initializes new state with id, optional name and terminal flag.</summary>
    /// <param name="id">Unique identifier of the state.</param>
    /// <param name="stateName">Optional state's name.</param>
    /// <param name="isTerminal">Indicates that state requires Turing machine to stop.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> equals to the default state ID.</exception>
    public TuringMachineState(int id, string? stateName = null, bool isTerminal = false)
        : base(id, stateName, isTerminal)
    {
        if (id == Halt.Id)
            throw new ArgumentException("Cannot create a new state with a default state ID.", nameof(id));
    }
    
    /// <inheritdoc cref="object.ToString()"/>
    /// <remarks>
    /// <c>!</c> for the <see cref="Halt"/> state;<br/>
    /// <c>qO1</c> for non-terminal states;<br/>
    /// <c>!qO1</c> for terminal states.
    /// </remarks>
    public override string ToString()
    {
        return Id == Halt.Id ? "!" : base.ToString();
    }
}
