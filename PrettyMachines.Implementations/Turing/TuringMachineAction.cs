using PrettyMachines.Implementations.Data;


namespace PrettyMachines.Implementations.Turing;

/// <summary>Represents action that Turing machine must take.</summary>
/// <typeparam name="TSymbol">Data type of the tape cells.</typeparam>
public readonly struct TuringMachineAction<TSymbol>
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


/// <summary>
/// Helper class for creating <see cref="TuringMachineAction{TSymbol}"/>.
/// </summary>
public static class TuringMachineAction 
{
    /// <summary>Creates new action that only switches machine's state.</summary>
    public static TuringMachineAction<T> SwitchState<T>(TuringMachineState nextState) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState);
    }

    /// <summary>Creates new action that switches machine's state, prints symbol and moves the tape.</summary>
    public static TuringMachineAction<T> PrintAndMoveRight<T>(TuringMachineState nextState, T? symbol) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState, symbol, TapeMovement.Right);
    }
    
    /// <summary>Creates new action that switches machine's state, prints symbol and moves the tape.</summary>
    public static TuringMachineAction<T> PrintAndMoveLeft<T>(TuringMachineState nextState, T? symbol) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState, symbol, TapeMovement.Left);
    }
    
    /// <summary>Creates new action that switches machine's state and prints symbol without moving the tape.</summary>
    public static TuringMachineAction<T> Print<T>(TuringMachineState nextState, T? symbol) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState, symbol);
    }
    
    /// <summary>Creates new action that prints symbol and stops the machine.</summary>
    public static TuringMachineAction<T> PrintAndHalt<T>(T? symbol) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(TuringMachineState.Halt, symbol);
    }
    
    /// <summary>Creates new action that stops the machine.</summary>
    public static TuringMachineAction<T> Halt<T>() where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(TuringMachineState.Halt);
    }
    
    /// <summary>Creates new action that switches machine's state and moves the tape without printing.</summary>
    public static TuringMachineAction<T> MoveRight<T>(TuringMachineState nextState) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState, TapeMovement.Right);
    }
    
    /// <summary>Creates new action that switches machine's state and moves the tape without printing.</summary>
    public static TuringMachineAction<T> MoveLeft<T>(TuringMachineState nextState) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState, TapeMovement.Left);
    }
}