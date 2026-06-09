namespace PrettyMachines.Algorithms.Turing;

/// <summary>Turing machine's state.</summary>
public class TuringMachineState
{
    private string? stringView;
    
    /// <summary>Unique identifier of the state.</summary>
    public int Id { get; private init; }

    /// <summary>Returns <c>true</c> if this state requires Turing machine to stop.</summary>
    public bool IsTerminal { get; } = true;

    /// <summary>Optional state's name.</summary>
    public string? Name { get; }
    
    
    /// <summary>Get the default terminating state.</summary>
    public static TuringMachineState Halt { get; } = new() { Id = int.MinValue };
    
    private TuringMachineState() {}
    
    /// <summary>Initializes new state with id, optional name and terminal flag.</summary>
    public TuringMachineState(int id, string? stateName = null, bool isTerminal = false)
    {
        if (id == Halt.Id)
            throw new ArgumentException("Cannot create a new state with a default state ID.", nameof(id));
        
        Id = id;
        IsTerminal = isTerminal;
        Name = stateName;
    }
    
    /// <inheritdoc cref="object.ToString()"/>
    /// <remarks>
    /// <c>!</c> for the <see cref="Halt"/> state;<br/>
    /// <c>qO1</c> for non-terminal states;<br/>
    /// <c>!qO1</c> for terminal states.
    /// </remarks>
    public override string ToString()
    {
        if (Id == Halt.Id) return "!";
        return stringView ??= IsTerminal ? $"!q{Id:D2}" : $"q{Id:D2}";
    }
}