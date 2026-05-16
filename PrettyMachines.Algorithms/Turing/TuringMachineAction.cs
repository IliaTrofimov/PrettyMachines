namespace PrettyMachines.Algorithms.Turing;

public readonly struct TuringMachineAction<TSymbol>
{
    public static TuringMachineAction<TSymbol> Halt { get; } = new(TuringMachineState.Halt);
    
    public TuringMachineState NextState { get; } = TuringMachineState.Halt;

    public TSymbol? PrintedSymbol { get; } = default;

    public TapeMovement Movement { get; } = TapeMovement.None;
    
    public bool ShouldPrintSymbol => PrintedSymbol != null;
    
    
    public TuringMachineAction(TuringMachineState nextState, TSymbol printedSymbol, TapeMovement movement = TapeMovement.None)
    {
        NextState = nextState;
        PrintedSymbol = printedSymbol;
        Movement = movement;
    }
    
    public TuringMachineAction(TuringMachineState nextState, TapeMovement movement = TapeMovement.None) 
    {
        NextState = nextState;
        PrintedSymbol = default;
        Movement = movement;
    }
    
    
    public override string ToString()
    {
        var movementChar = Movement switch
        {
            TapeMovement.None  => 'N',
            TapeMovement.Left  => 'L',
            TapeMovement.Right => 'R',
            _                  => '?'
        };
        
        return ShouldPrintSymbol 
            ? $"'{PrintedSymbol}' {movementChar} {NextState.ToString(true)}" 
            : $"{movementChar} {NextState.ToString(true)}";
    }
}