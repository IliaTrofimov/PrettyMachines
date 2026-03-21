using System.Diagnostics;


namespace PrettyMachines.Implementations.Turing;

/// <summary>
/// Set of conditions and corresponding actions that define Turing machine instructions.
/// </summary>
[DebuggerDisplay("States {StatesCount}, symbols {AlphabetLength}, instructions {InstructionsCount}")]
public class InstructionsTable<TSymbol>
{
    protected readonly record struct Key(int? SymbolId, SymbolAcceptance Mode);
    
    private readonly Dictionary<int, TSymbol> addedSymbols = [];
    private readonly Dictionary<TuringMachineState, Dictionary<Key, TuringMachineAction<TSymbol>>?> instructions = [];
    private readonly IEqualityComparer<TSymbol> symbolComparer;


    protected InstructionsTable(
        IEqualityComparer<TSymbol> symbolComparer,
        Dictionary<int, TSymbol> addedSymbols,
        Dictionary<TuringMachineState, Dictionary<Key, TuringMachineAction<TSymbol>>?> instructions)
    {
        this.addedSymbols = addedSymbols;
        this.instructions = instructions;
        this.symbolComparer = symbolComparer;
    }

    /// <summary>Create Turing machine instructions.</summary>
    /// <param name="symbolComparer">Object that will be used to compare symbols.</param>
    public InstructionsTable(IEqualityComparer<TSymbol>? symbolComparer = null)
    {
        this.symbolComparer = symbolComparer ?? EqualityComparer<TSymbol>.Default;
    }

    /// <summary>
    /// If <c>true</c>, than calling <c>FindInstruction</c> for unknown symbol will throw <see cref="InvalidOperationException"/>.
    /// </summary>
    public bool StrictAlphabet { get; set; }
    
    /// <summary>Amount of all unique states.</summary>
    public int StatesCount => instructions.Keys.Count;
    
    /// <summary>Amount of all unique instructions.</summary>
    public int InstructionsCount => instructions.Sum(x => x.Value?.Count ?? 0);

    /// <summary>Amount of all unique symbols that can be scanned or printed.</summary>
    public int AlphabetLength => addedSymbols.Count;
    
    /// <summary>Enumerate all states that were added as initial or target.</summary>
    public IEnumerable<TuringMachineState> States => instructions.Keys;
    
    /// <summary>Enumerate all known symbols.</summary>
    public IEnumerable<TSymbol> Alphabet => addedSymbols.Values;

    /// <summary>
    /// Enumerate all instructions as flat sequence of tuples <c>(condition, action)</c> for each added state and symbol.
    /// </summary>
    public IEnumerable<(TuringMachineCondition<TSymbol>, TuringMachineAction<TSymbol>)> Instructions
    {
        get
        {
            foreach (var (state, symbolsDict) in instructions)
            {
                if (symbolsDict == null)
                    continue;
                
                foreach (var (symbolKey, action) in symbolsDict)
                {
                    var condition = symbolKey.Mode == SymbolAcceptance.ExactValue
                        ? new TuringMachineCondition<TSymbol>(state, addedSymbols[symbolKey.SymbolId!.Value])
                        : new TuringMachineCondition<TSymbol>(state, symbolKey.Mode);
                    
                    yield return (condition, action);
                }
            }
        }
    }
    

    /// <summary>Add new instruction for given condition and action.</summary>
    public virtual void AddInstruction(TuringMachineCondition<TSymbol> condition, TuringMachineAction<TSymbol> action)
    {
        var symbolKey = GetSymbolKey(condition.ScannedSymbol, condition.Mode);

        if (instructions.TryGetValue(condition.InitialState, out var symbolsDict) && symbolsDict != null)
        {
            symbolsDict[symbolKey] = action;
        }
        else
        {
            instructions[condition.InitialState] = new Dictionary<Key, TuringMachineAction<TSymbol>>
            {
                [symbolKey] = action
            };
        }
        
        if (action.NextState.Id != TuringMachineState.Halt.Id)
            instructions.TryAdd(action.NextState, null);
        
        if (condition is { Mode: SymbolAcceptance.ExactValue, ScannedSymbol: not null })
            addedSymbols.TryAdd(symbolComparer.GetHashCode(condition.ScannedSymbol), condition.ScannedSymbol!);
        
        if (action is { ShouldPrintSymbol: true, PrintedSymbol: not null })
            addedSymbols.TryAdd(symbolComparer.GetHashCode(action.PrintedSymbol), action.PrintedSymbol!);
    }

    /// <summary>Try finding action for given state and tape's symbol.</summary>
    /// <returns>Corresponding action or <c>null</c> if such initial values cannot be accepted.</returns>
    /// <exception cref="InvalidOperationException">Found unknown symbol with StrictAlphabet = true.</exception>
    public virtual TuringMachineAction<TSymbol>? FindInstruction(TuringMachineState state, bool isEmpty, TSymbol? symbol)
    {
        if (!instructions.TryGetValue(state, out Dictionary<Key, TuringMachineAction<TSymbol>>? symbolsDict) || symbolsDict == null)
            return null;
        
        TuringMachineAction<TSymbol> action;
        
        if (isEmpty)
        {
            if (symbolsDict.TryGetValue(GetSymbolKey(symbol, SymbolAcceptance.EmptyValue), out action))
                return action;
        }
        else
        { 
            if (StrictAlphabet && !addedSymbols.ContainsKey(symbolComparer.GetHashCode(symbol!)))
                throw new InvalidOperationException($"Symbol '{symbol}' wasn't present in the alphabet. " +
                                                    $"Cannot scan unknown symbols with {nameof(StrictAlphabet)} enabled.");
            
            if (symbolsDict.TryGetValue(GetSymbolKey(symbol, SymbolAcceptance.ExactValue), out action))
                return action;
            
            if (symbolsDict.TryGetValue(GetSymbolKey(symbol, SymbolAcceptance.NotEmptyValue), out action))
                return action;
        }
        
        return symbolsDict.TryGetValue(GetSymbolKey(symbol, SymbolAcceptance.AnyValue), out action) 
            ? action 
            : null;
    }

    private Key GetSymbolKey(TSymbol? symbol, SymbolAcceptance mode)
    {
        return mode == SymbolAcceptance.ExactValue
            ? new Key(symbolComparer.GetHashCode(symbol!), SymbolAcceptance.ExactValue)
            : new Key(null, mode);
    }


    /// <summary>Readonly version of <see cref="InstructionsTable{TSymbol}"/>. This class cannot add new instructions.</summary>
    public sealed class Readonly(InstructionsTable<TSymbol> instance)
        : InstructionsTable<TSymbol>(instance.symbolComparer, instance.addedSymbols, instance.instructions)
    {
        public override void AddInstruction(TuringMachineCondition<TSymbol> condition, TuringMachineAction<TSymbol> action)
        {
            throw new NotSupportedException("This instructions cannot be changed.");
        }
    }
}