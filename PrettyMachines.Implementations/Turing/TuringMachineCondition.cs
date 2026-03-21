namespace PrettyMachines.Implementations.Turing;


/// <summary>Turing machine's tape values comparision type: exact or soft.</summary>
public enum SymbolAcceptance
{
    ExactValue = 0, 
    NotEmptyValue,
    EmptyValue,
    AnyValue
}

/// <summary>
/// Represents conditions (initial state and symbol) that must be satisfied before executing some action with Turing machine.
/// </summary>
/// <typeparam name="TSymbol">Data type of the tape cells.</typeparam>
public readonly struct TuringMachineCondition<TSymbol>
{
    /// <summary>Create new condition for given initial state and soft symbol acceptance mode.</summary>
    /// <exception cref="ArgumentException">Initial state cannot be terminal and SymbolAcceptance cannot be ExactValue.</exception>
    public TuringMachineCondition(TuringMachineState initialState, SymbolAcceptance accepts)
    {
        if (initialState.IsTerminal) 
            throw new ArgumentException("Initial state cannot be terminal.", nameof(initialState));
        if (accepts == SymbolAcceptance.ExactValue) 
            throw new ArgumentException("SymbolAcceptance cannot be ExactValue with no value provided.", nameof(accepts));

        InitialState = initialState;
        Mode = accepts;
    }
    
    /// <summary>Create new condition for given initial state and exact initial value.</summary>
    /// <exception cref="ArgumentException">Initial state cannot be terminal.</exception>
    public TuringMachineCondition(TuringMachineState initialState, TSymbol symbol)
    {
        if (initialState.IsTerminal) 
            throw new ArgumentException("Initial state cannot be terminal.", nameof(initialState));
        
        InitialState = initialState;
        ScannedSymbol = symbol;
        Mode = SymbolAcceptance.ExactValue;
    }
    
        
    /// <summary>Reference to the initial state. Instructions can be applied when machine is in this state.</summary>
    public TuringMachineState InitialState { get; } 
    
    /// <summary>Exact value that should match current cell. If <c>null</c> than soft comparision will be used.</summary>
    public TSymbol? ScannedSymbol { get; }
    
    /// <summary>Value comparision type.</summary>
    public SymbolAcceptance Mode { get; }
    

    public override string ToString()
    {
        var symbol = Mode switch
        {
            SymbolAcceptance.ExactValue when ScannedSymbol is not null => $"'{ScannedSymbol}'",
            SymbolAcceptance.ExactValue when ScannedSymbol is null     => "<null>",
            SymbolAcceptance.NotEmptyValue                             => "<not-empty>",
            SymbolAcceptance.EmptyValue                                => "<empty>",
            SymbolAcceptance.AnyValue                                  => "<any>",
            _                                                          => "<?>"
        };
        return $"{InitialState.ToString(true)} {symbol}";
    }
}


/// <summary>
/// Helper class for creating <see cref="TuringMachineCondition{TSymbol}"/>.
/// </summary>
public static class TuringMachineCondition
{
    /// <summary>For given initial state creates new condition that accepts exact value.</summary>
    public static TuringMachineCondition<T> AcceptExact<T>(this TuringMachineState initialState, T symbol)
        where T : IEquatable<T>
    {
        return new TuringMachineCondition<T>(initialState, symbol);
    }

    /// <summary>For given initial state creates new condition that accepts any filled cell.</summary>
    public static TuringMachineCondition<T> AcceptNotEmpty<T>(this TuringMachineState initialState)
        where T : IEquatable<T>
    {
        return new TuringMachineCondition<T>(initialState, SymbolAcceptance.NotEmptyValue);
    }

    /// <summary>For given initial state creates new condition that accepts only empty cells.</summary>
    public static TuringMachineCondition<T> AcceptEmpty<T>(this TuringMachineState initialState) where T : IEquatable<T>
    {
        return new TuringMachineCondition<T>(initialState, SymbolAcceptance.EmptyValue);
    }

    /// <summary>For given initial state creates new condition that accepts any cells.</summary>
    public static TuringMachineCondition<T> AcceptAny<T>(this TuringMachineState initialState) where T : IEquatable<T>
    {
        return new TuringMachineCondition<T>(initialState, SymbolAcceptance.AnyValue);
    }
}