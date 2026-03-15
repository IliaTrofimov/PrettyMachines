namespace PrettyMachines.Implementations.Turing;

/// <summary>Represents action that Turing machine must take.</summary>
/// <typeparam name="TSymbol">Data type of the tape cells.</typeparam>
public class TuringMachineAction<TSymbol>
{
    /// <summary>Create new action that will change Turing machine's state, print given symbol and move the tape.</summary>
    public TuringMachineAction(TuringMachineState nextState, TSymbol? printedSymbol, TapeMovement movement = TapeMovement.None)
    {
        NextState = nextState;
        PrintedSymbol = printedSymbol;
        Movement = movement;
        ShouldPrintSymbol = true;
    }
    
    /// <summary>Create new action that will change Turing machine's state and move the tape.</summary>
    public TuringMachineAction(TuringMachineState nextState, TapeMovement movement = TapeMovement.None) 
    {
        NextState = nextState;
        PrintedSymbol = default;
        Movement = movement;
        ShouldPrintSymbol = false;
    }
    
    /// <summary>Reference to the next state.</summary>
    public TuringMachineState NextState { get; }
    
    /// <summary>Machine should put this symbol into the tape if this value is not <c>null</c>.</summary>
    public TSymbol? PrintedSymbol { get; }
    
    /// <summary>Machine should move the tape in this direction.</summary>
    public TapeMovement Movement { get; }
    
    /// <summary>Returns <c>true</c> if machine should put <see cref="PrintedSymbol"/> into the tape.</summary>
    public bool ShouldPrintSymbol { get; }
    
    
    public override string ToString()
    {
        var movementChar = Movement switch
        {
            TapeMovement.None  => 'N',
            TapeMovement.Left  => 'L',
            TapeMovement.Right => 'R',
            _                  => '?'
        };
        
        if (ShouldPrintSymbol)
        {
            return PrintedSymbol is null 
                ? $"<null> {movementChar} {NextState.ToString(true)}"
                : $"'{PrintedSymbol}' {movementChar} {NextState.ToString(true)}";
        }

        return $"{movementChar} {NextState.ToString(true)}";
    }
}