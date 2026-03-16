namespace PrettyMachines.Implementations.Turing;

/// <summary>
/// Represents conditions (initial state and symbol) that must be satisfied before executing some action with Turing machine.
/// </summary>
/// <typeparam name="TSymbol">Data type of the tape cells.</typeparam>
public class TuringMachineCondition<TSymbol>
{
    /// <summary>Create new condition for given initial state and soft symbol acceptance mode.</summary>
    /// <exception cref="ArgumentException">Initial state cannot be terminal and SymbolAcceptance cannot be ExactValue.</exception>
    public TuringMachineCondition(TuringMachineState initialState, SymbolAcceptance accepts)
    {
        if (initialState.IsTerminal) 
            throw new ArgumentException("Initial state cannot be terminal.", nameof(initialState));

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