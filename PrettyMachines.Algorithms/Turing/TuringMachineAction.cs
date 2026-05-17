namespace PrettyMachines.Algorithms.Turing;

public readonly struct TuringMachineAction
{
    public TuringMachineState NextState { get; } = TuringMachineState.Halt;

    public string? PrintedSymbol { get; } = default;

    public TapeMovement Movement { get; } = TapeMovement.None;
    
    public bool ShouldPrintSymbol => PrintedSymbol != null;


    public static TuringMachineAction Halt { get; } = new(TuringMachineState.Halt);

    public static TuringMachineAction CreateHalt(string? printedSymbol, TapeMovement movement = TapeMovement.None)
    {
        return printedSymbol == null 
            ? new(TuringMachineState.Halt, movement) 
            : new(TuringMachineState.Halt, printedSymbol, movement);
    }
    
    public TuringMachineAction(TuringMachineState nextState, string printedSymbol, TapeMovement movement = TapeMovement.None)
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