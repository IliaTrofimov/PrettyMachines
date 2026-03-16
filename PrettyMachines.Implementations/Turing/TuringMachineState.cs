namespace PrettyMachines.Implementations.Turing;

/// <summary>Turing machine's state.</summary>
public class TuringMachineState : IEquatable<TuringMachineState>
{
    /// <summary>Default terminating state.</summary>
    public static readonly TuringMachineState DefaultTerminalState = new();
    
    private TuringMachineState() {}

    /// <summary>Turing machine's state.</summary>
    public TuringMachineState(int id, string? stateName, bool isTerminal = false)
    {
        if (id == DefaultTerminalState.Id)
            throw new InvalidOperationException("Cannot create a new state with a default state ID.");
        
        Id = id;
        IsTerminal = isTerminal;
        Name = stateName;
    }
    
    /// <summary>Unique identifier of the state.</summary>
    public int Id { get; } = int.MinValue;

    /// <summary>Returns <c>true</c> if this state requires Turing machine to stop.</summary>
    public bool IsTerminal { get; } = true;

    /// <summary>Optional state's name.</summary>
    public string? Name { get; }
        
    /// <summary>Reference to the Turing machine that defines this state. State can be attached only once.</summary>
    public TuringMachine? Machine { get; private set; }


    /// <summary>Attach this state to given Turing machine. State can be attached only once.</summary>
    /// <exception cref="InvalidOperationException">State can be attached only once.</exception>
    public void Attach(TuringMachine machine)
    {
        if (Machine != null && !ReferenceEquals(Machine, machine))
            throw new InvalidOperationException("State is already attached to other Turing machine.");
        Machine = machine;
    }
    
    public bool Equals(TuringMachineState? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && ReferenceEquals(Machine, other.Machine);
    }

    public override bool Equals(object? obj) => obj is TuringMachineState state && Equals(state);

    public override int GetHashCode() => HashCode.Combine(Id);

    
    public override string ToString() => ToString(false);
    public string ToString(bool shortString)
    {
        if (Id == DefaultTerminalState.Id) 
            return "!";
        
        const string shortTerminalFmt = "!Q_{0}";
        const string longTerminalFmt = "!Q_{0} '{1}'";
        const string shortFmt = "Q_{0}";
        const string longFmt = "Q_{0} '{1}'"; 
        
        return shortString || string.IsNullOrWhiteSpace(Name)
            ? string.Format(IsTerminal ? shortTerminalFmt : shortFmt, Id)
            : string.Format(IsTerminal ? longTerminalFmt : longFmt, Id, Name);
    }
}