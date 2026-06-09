using System.Collections;
using System.Diagnostics;
using PrettyMachines.Algorithms.Abstract;

namespace PrettyMachines.Algorithms.Turing;

/// <summary>
/// Set of conditions and corresponding actions that define Turing machine instructions.
/// </summary>
[DebuggerDisplay("Rules: {RulesCount}, stats: {States.Count}, symbols: {Alphabet.Count}")]
public class InstructionsTable : IReadOnlyInstructionsTable
{
    private readonly bool isAutoAlphabet;
    private readonly HashSet<string?> alphabet;
    private readonly FuzzyKeyComparer<string> fuzzySymbolsComparer;
    private readonly Dictionary<TuringMachineState, Dictionary<FuzzyKey<string>, TuringMachineAction>> statesDict;
    
    
    /// <summary>Gets total number of added instructions.</summary>
    /// <remarks>Always less or equal than <i>States.Count</i> * <i>Alphabet.Count</i>.</remarks>
    public int RulesCount { get; protected set; }
    
    /// <summary>Gets special value that represents an empty symbol.</summary>
    public string? BlankSymbol { get; init; }
    
    /// <summary>Gets the collection of all defined states.</summary>
    public IReadOnlyCollection<TuringMachineState> States => statesDict.Keys;

    /// <summary>Gets collection of allowed symbols. Blank symbol is always included.</summary>
    public IReadOnlySet<string?> Alphabet => alphabet;
    
    
    /// <summary>Initializes new instructions table with given set of allowed symbols and string comparision type.</summary>
    /// <param name="alphabetSymbols">Alphabet that defines set of allowed symbols. Duplicate items will be ignored.</param>
    /// <param name="symbolsComparison"></param>
    public InstructionsTable(IEnumerable<string> alphabetSymbols, StringComparison symbolsComparison = StringComparison.Ordinal)
        : this(alphabetSymbols, null, symbolsComparison)
    {
    }

    /// <summary>
    /// Initializes new instructions table with an unrestricted alphabet, predefined empty symbol and string comparision type.
    /// </summary>
    /// <inheritdoc cref="InstructionsTable(IEnumerable{string}?,string?,StringComparison)"/>
    public InstructionsTable(string? blankSymbol, StringComparison symbolsComparison = StringComparison.Ordinal)
        : this(null, blankSymbol, symbolsComparison)
    {
    }
    
    /// <summary>
    /// Initializes new instructions table with an unrestricted alphabet set of allowed symbols and string comparision type.
    /// </summary>
    /// <inheritdoc cref="InstructionsTable(IEnumerable{string}?,string?,StringComparison)"/>
    public InstructionsTable(StringComparison symbolsComparison = StringComparison.Ordinal) 
        : this(null, null, symbolsComparison)
    {
    }
    
    /// <summary>
    /// Initializes new instructions table with given set of allowed symbols, predefined empty symbol and string comparision type.
    /// </summary>
    /// <param name="alphabetSymbols">
    /// Alphabet that defines set of allowed symbols. Duplicate items will be ignored.
    /// <c>Null</c> value means unrestricted alphabet.
    /// </param>
    /// <param name="blankSymbol">Special value that represents an empty symbol.</param>
    /// <param name="symbolsComparison">String comparision mode for the symbols.</param>
    public InstructionsTable(IEnumerable<string>? alphabetSymbols, string? blankSymbol, StringComparison symbolsComparison = StringComparison.Ordinal)
    {
        var comparer = StringComparer.FromComparison(symbolsComparison);

        if (alphabetSymbols != null)
        {
            isAutoAlphabet = false;
            alphabet = new HashSet<string?>(alphabetSymbols, comparer);
            if (alphabet.Count == 0)
                throw new ArgumentException("Alphabet cannot be empty.", nameof(alphabetSymbols));
        }
        else
        {
            isAutoAlphabet = true;
            alphabet = new HashSet<string?>(comparer);
        }
        
        alphabet.Add(blankSymbol);
        statesDict = [];
        BlankSymbol = blankSymbol;
        fuzzySymbolsComparer = new FuzzyKeyComparer<string>(comparer);
    }

    /// <summary>Creates a deep copy of another instructions table.</summary>
    /// <param name="other">The instructions to copy.</param>
    public InstructionsTable(InstructionsTable other)
    {
        isAutoAlphabet = other.isAutoAlphabet;
        alphabet = new HashSet<string?>(other.alphabet, other.alphabet.Comparer);
        fuzzySymbolsComparer = other.fuzzySymbolsComparer;
        statesDict = new Dictionary<TuringMachineState, Dictionary<FuzzyKey<string>, TuringMachineAction>>(other.statesDict.Count);
        
        foreach (var (state, symbolsDict) in other.statesDict)
            statesDict[state] = symbolsDict.ToDictionary(x => x.Key, x => x.Value, fuzzySymbolsComparer);
        
        BlankSymbol = other.BlankSymbol;
        RulesCount = other.RulesCount;
    }
    
    /// <summary>Adds new state with no instructions. Does nothing if state is already added.</summary>
    /// <param name="state">State object.</param>
    public void AddState(TuringMachineState state)
    {
        if (!statesDict.ContainsKey(state))
            statesDict[state] = new(Alphabet.Count, fuzzySymbolsComparer);
    }
    
    /// <summary>Adds new instruction with given condition and action. Overrides instructions with same conditions.</summary>
    /// <param name="initialState">Initial state that matches this rule.</param>
    /// <param name="symbol">Scanned symbol that matches this rule. Symbol can use fuzzy matching (not empty, empty, any).</param>
    /// <param name="action">Action that will be associated with given conditions.</param>
    /// <exception cref="AlgorithmException">Initial state is terminal.</exception>
    /// <exception cref="SymbolIsNotAllowedException">Symbol or action have invalid symbols.</exception>
    public void AddRule(TuringMachineState initialState, in FuzzyKey<string> symbol, in TuringMachineAction action)
    {
        if (initialState.IsTerminal)
            throw new AlgorithmException("Instruction's initial state must not be terminal.");

        ValidateSymbols(in symbol, in action);
        
        if (!statesDict.TryGetValue(initialState, out var symbolsDict))
        {
            symbolsDict = new(alphabet.Count, fuzzySymbolsComparer);
            statesDict.Add(initialState, symbolsDict);
        }

        if (!symbolsDict.ContainsKey(symbol))
            RulesCount++;
        
        symbolsDict[symbol] = action;
        
        if (action.NextState.Equals(TuringMachineState.Halt) && !statesDict.ContainsKey(action.NextState))
        {
            statesDict.Add(action.NextState, new(alphabet.Count, fuzzySymbolsComparer));
        }
    }

    /// <summary>Outputs the action for given state and input symbol.</summary>
    /// <param name="state">Current state of the machine.</param>
    /// <param name="symbol">Input symbol.</param>
    /// <param name="action">Resulting action. When the returned value is <c>false</c> always equal to the HALT action.</param>
    /// <returns><c>True</c> if such action exists in the instructions table.</returns>
    public bool TryFindAction(TuringMachineState state, string? symbol, out TuringMachineAction action)
    {
        if (!statesDict.TryGetValue(state, out var symbolsDict))
        {
            action = TuringMachineAction.Halt;
            return false;
        }

        if (symbol != null)
        {
            var exactKey = FuzzyKey<string>.Exact(symbol);
            if (symbolsDict.TryGetValue(exactKey, out action))
                return true;
        }
        
        var fuzzyKey = Equals(symbol, BlankSymbol) ? FuzzyKey<string>.Empty : FuzzyKey<string>.NotEmpty;
        if (symbolsDict.TryGetValue(fuzzyKey, out action))
            return true;

        if (symbolsDict.TryGetValue(FuzzyKey<string>.Any, out action)) 
            return true;

        action = TuringMachineAction.Halt;
        return false;
    }

    /// <summary>Gets the action that is defined for given state and symbol</summary>
    /// <param name="state">State to match.</param>
    /// <param name="symbolMatch">Symbol to match.</param>
    /// <returns>Found action or <c>null</c> if it isn't defined.</returns>
    public TuringMachineAction? this[TuringMachineState state, in FuzzyKey<string> symbolMatch]
    {
        get
        {
            if (statesDict.TryGetValue(state, out var symbolsDict) && symbolsDict.TryGetValue(symbolMatch, out var action))
                return action;
            return null;
        }
    }
    
    public IEnumerator<TuringInstruction> GetEnumerator()
    {
        foreach (var (state, symbolsDict) in statesDict)
        {
            foreach (var (key, action) in symbolsDict)
            {
                yield return new TuringInstruction
                {
                    InitialState = state,
                    ScannedSymbol = key,
                    PrintedSymbol = action.PrintedSymbol,
                    NextState = action.NextState,
                    Movement = action.Movement
                };
            }
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    private void ValidateSymbols(in FuzzyKey<string> symbol, in TuringMachineAction action)
    {
        if (isAutoAlphabet)
        {
            if (symbol.Match == SymbolMatch.Exact)
                alphabet.Add(symbol.Value);

            if (action.PrintedSymbol is not null)
                alphabet.Add(action.PrintedSymbol);
        }
        else
        {
            if (symbol.Match == SymbolMatch.Exact && !Alphabet.Contains(symbol.Value!))
                throw new SymbolIsNotAllowedException(symbol.Value!, "invalid scanned symbol");

            if (action.PrintedSymbol is not null && !Alphabet.Contains(action.PrintedSymbol))
                throw new SymbolIsNotAllowedException(action.PrintedSymbol, "invalid printed symbol");
        }
    }
}
