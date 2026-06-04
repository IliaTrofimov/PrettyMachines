namespace PrettyMachines.Algorithms.Turing;

/// <summary>Turing machine's state.</summary>
public class TuringMachineState
{
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
    
    public override string ToString() => ToString(false);
    
    public string ToString(bool shortString)
    {
        if (Id == Halt.Id) return "!";
        
        const string shortTerminalFmt = "!q{0:D2}";
        const string longTerminalFmt = "!q{0:D2} '{1}'";
        const string shortFmt = "q{0:D2}";
        const string longFmt = "q{0:D2} '{1}'"; 
        
        return shortString || string.IsNullOrWhiteSpace(Name)
            ? string.Format(IsTerminal ? shortTerminalFmt : shortFmt, Id)
            : string.Format(IsTerminal ? longTerminalFmt : longFmt, Id, Name);
    }
}