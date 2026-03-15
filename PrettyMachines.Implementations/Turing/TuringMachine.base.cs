using System.Diagnostics;


namespace PrettyMachines.Implementations.Turing;

/// <summary>
/// Base class for all Turing machines. Also contains static helper methods. 
/// </summary>
public abstract class TuringMachine
{
    protected readonly List<TuringMachineState> States = [];

    
    /// <summary>Returns index for the next added state.</summary>
    public abstract int GetNewStateIndex();
    
    
    /// <summary>For given initial state creates new condition that accepts exact value.</summary>
    public static TuringMachineCondition<T> AcceptsExact<T>(TuringMachineState initialState, T symbol) where T : IEquatable<T>
    {
        return new TuringMachineCondition<T>(initialState, symbol);
    }
    
    /// <summary>For given initial state creates new condition that accepts any filled cell.</summary>
    public static TuringMachineCondition<T> AcceptsNotEmpty<T>(TuringMachineState initialState) where T : IEquatable<T>
    {
        return new TuringMachineCondition<T>(initialState, SymbolAcceptance.NotEmptyValue);
    }
    
    /// <summary>For given initial state creates new condition that accepts only empty cells.</summary>
    public static TuringMachineCondition<T> AcceptsEmpty<T>(TuringMachineState initialState) where T : IEquatable<T>
    {
        return new TuringMachineCondition<T>(initialState, SymbolAcceptance.EmptyValue);
    }
    
    /// <summary>For given initial state creates new condition that accepts any cells.</summary>
    public static TuringMachineCondition<T> AcceptsAny<T>(TuringMachineState initialState) where T : IEquatable<T>
    {
        return new TuringMachineCondition<T>(initialState, SymbolAcceptance.AnyValue);
    }
    
    /// <summary>Creates new action that only switches machine's state.</summary>
    public static TuringMachineAction<T> SwitchesState<T>(TuringMachineState nextState) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState);
    }

    /// <summary>Creates new action that switches machine's state, prints symbol and moves the tape.</summary>
    public static TuringMachineAction<T> PrintsAndMovesRight<T>(TuringMachineState nextState, T? symbol) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState, symbol, TapeMovement.Right);
    }
    
    /// <summary>Creates new action that switches machine's state, prints symbol and moves the tape.</summary>
    public static TuringMachineAction<T> PrintsAndMovesLeft<T>(TuringMachineState nextState, T? symbol) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState, symbol, TapeMovement.Left);
    }
    
    /// <summary>Creates new action that switches machine's state and prints symbol without moving the tape.</summary>
    public static TuringMachineAction<T> Prints<T>(TuringMachineState nextState, T? symbol) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState, symbol);
    }
    
    /// <summary>Creates new action that prints symbol and stops the machine.</summary>
    public static TuringMachineAction<T> PrintsAndStops<T>(T? symbol) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(TuringMachineState.DefaultTerminalState, symbol);
    }
    
    /// <summary>Creates new action that stops the machine.</summary>
    public static TuringMachineAction<T> Stops<T>() where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(TuringMachineState.DefaultTerminalState);
    }
    
    /// <summary>Creates new action that switches machine's state and moves the tape without printing.</summary>
    public static TuringMachineAction<T> MovesRight<T>(TuringMachineState nextState) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState, TapeMovement.Right);
    }
    
    /// <summary>Creates new action that switches machine's state and moves the tape without printing.</summary>
    public static TuringMachineAction<T> MovesLeft<T>(TuringMachineState nextState) where T : IEquatable<T>
    {
        return new TuringMachineAction<T>(nextState, TapeMovement.Left);
    }
}