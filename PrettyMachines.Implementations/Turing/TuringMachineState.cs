namespace PrettyMachines.Implementations.Turing;

/// <summary>Turing machine's state.</summary>
public class TuringMachineState : IEquatable<TuringMachineState>
{
    /// <summary>Default terminating state.</summary>
    public static readonly TuringMachineState DefaultTerminalState = new() { IsTerminal = true, Id = int.MinValue };
    
    private TuringMachineState() {}

    /// <summary>Turing machine's state.</summary>
    public TuringMachineState(TuringMachine machine, string? stateName, bool isTerminal = false)
    {
        Machine = machine;
        IsTerminal = isTerminal;
        Name = stateName;
        Id = machine.GetNewStateIndex();
        if (Id == DefaultTerminalState.Id)
            throw new InvalidOperationException("Cannot create a new state with a default state ID.");
    }
    
    
    /// <summary>Reference to the Turing machine that defines this state.</summary>
    public TuringMachine Machine { get; }
    
    /// <summary>Returns <c>true</c> if this state requires Turing machine to stop.</summary>
    public bool IsTerminal { get; private init; }

    /// <summary>Optional state's name.</summary>
    public string? Name { get; }

    /// <summary>Unique identifier of the state given by Turing machine.</summary>
    public int Id { get; private init; }

    
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
        
        const string shortTerminalFmt = "*Q_{0}";
        const string longTerminalFmt = "*Q_{0} '{1}'";
        const string shortFmt = "Q_{0}";
        const string longFmt = "Q_{0} '{1}'"; 
        
        return shortString || string.IsNullOrWhiteSpace(Name)
            ? string.Format(IsTerminal ? shortTerminalFmt : shortFmt, Id)
            : string.Format(IsTerminal ? longTerminalFmt : longFmt, Id, Name);
    }
}