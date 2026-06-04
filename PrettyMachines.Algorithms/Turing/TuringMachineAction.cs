using PrettyMachines.Algorithms.Utils;


namespace PrettyMachines.Algorithms.Turing;

/// <summary>
/// 
/// </summary>
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


    public override string ToString() => ToString(false);
    
    public string ToString(bool shortString)
    {
        return ShouldPrintSymbol 
            ? $"'{PrintedSymbol}', {Movement.ToChar()}, {NextState.ToString(shortString)}" 
            : $"{Movement.ToChar()}, {NextState.ToString(shortString)}";
    }
}