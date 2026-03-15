namespace PrettyMachines.Implementations.Turing;

/// <summary>
/// Set of conditions and corresponding actions that define Turing machine instruction.
/// </summary>
/// <param name="symbolComparer">Object that will be used to compare symbols.</param>
public class InstructionsTable<TSymbol>(IEqualityComparer<TSymbol>? symbolComparer = null)
{
    private readonly record struct Key(int? SymbolId, SymbolAcceptance Mode);
    
    private readonly Dictionary<int, Dictionary<Key, TuringMachineAction<TSymbol>>> instructions = [];
    private readonly IEqualityComparer<TSymbol> symbolComparer = symbolComparer ?? EqualityComparer<TSymbol>.Default;


    /// <summary>Add new instruction for given condition and action.</summary>
    public void AddInstruction(TuringMachineCondition<TSymbol> condition, TuringMachineAction<TSymbol> action)
    {
        var key = GetSymbolKey(condition.ScannedSymbol, condition.Mode);
        
        if (instructions.TryGetValue(condition.InitialState.Id, out var symbols))
        {
            symbols[key] = action;
        }
        else
        {
            instructions.Add(condition.InitialState.Id, new Dictionary<Key, TuringMachineAction<TSymbol>>
            {
                [key] = action
            });
        }
    }

    /// <summary>Try finding action for given state and tape's symbol.</summary>
    /// <returns>Corresponding action or <c>null</c> if such initial values cannot be accepted.</returns>
    public TuringMachineAction<TSymbol>? FindInstruction(TuringMachineState state, bool isEmpty, TSymbol? symbol)
    {
        if (!instructions.TryGetValue(state.Id, out Dictionary<Key, TuringMachineAction<TSymbol>>? symbols))
            return null;
        
        TuringMachineAction<TSymbol>? action;
        
        if (isEmpty)
        {
            if (symbols.TryGetValue(GetSymbolKey(symbol, SymbolAcceptance.EmptyValue), out action))
                return action;
        }
        else if (symbols.TryGetValue(GetSymbolKey(symbol, SymbolAcceptance.ExactValue), out action))
        {
            return action;
        }
        else if (symbols.TryGetValue(GetSymbolKey(symbol, SymbolAcceptance.NotEmptyValue), out action))
        {
            return action;
        }
        
        return symbols.TryGetValue(GetSymbolKey(symbol, SymbolAcceptance.AnyValue), out action) 
            ? action 
            : null;
    }

    public IEnumerable<(int stateId, (int? symbolId, SymbolAcceptance mode) symbol, TuringMachineAction<TSymbol> action)> EnumerateInstructions()
    {
        foreach (var (stateId, symbols) in instructions)
        foreach (var (symbol, action) in symbols)
            yield return (stateId, (symbol.SymbolId, symbol.Mode), action);
    }


    private Key GetSymbolKey(TSymbol? symbol, SymbolAcceptance mode)
    {
        return mode == SymbolAcceptance.ExactValue
            ? new Key(symbolComparer.GetHashCode(symbol!), SymbolAcceptance.ExactValue)
            : new Key(null, mode);
    }
}