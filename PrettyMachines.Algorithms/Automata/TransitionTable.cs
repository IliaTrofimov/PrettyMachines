using System.Diagnostics;
using PrettyMachines.Abstract;


namespace PrettyMachines.Automata;

/// <summary>
/// Ordered store of transition rules shared by automata implementations.
/// </summary>
/// <typeparam name="TState">Concrete state type.</typeparam>
/// <typeparam name="TSymbol">Type of the matched symbols (for example <see cref="string"/> or <see cref="char"/>).</typeparam>
/// <typeparam name="TAction">Action type associated with a transition.</typeparam>
[DebuggerDisplay("Rules: {RulesCount}, states: {States.Count}, symbols: {Alphabet.Count}")]
public abstract class TransitionTable<TState, TSymbol, TAction>
    where TState : AutomatonState
    where TSymbol : notnull
    where TAction : struct
{
    private readonly bool isAutoAlphabet;
    private readonly IEqualityComparer<TSymbol> symbolComparer;
    private readonly HashSet<TSymbol?> alphabet;
    private readonly FuzzyKeyComparer<TSymbol> fuzzySymbolsComparer;
    private readonly Dictionary<TState, Dictionary<FuzzyKey<TSymbol>, TAction>> statesDict;
    private readonly List<AutomatonInstruction<TState, TSymbol, TAction>> transitions;


    /// <summary>Gets total number of added transitions.</summary>
    /// <remarks>Always less or equal than <i>States.Count</i> * <i>Alphabet.Count</i>.</remarks>
    public int RulesCount { get; protected set; }

    /// <summary>Gets special value that represents an empty symbol.</summary>
    public TSymbol? BlankSymbol { get; init; }

    /// <summary>Gets the collection of all defined states.</summary>
    public IReadOnlyCollection<TState> States => statesDict.Keys;

    /// <summary>Gets collection of allowed symbols. Blank symbol is always included.</summary>
    public IReadOnlySet<TSymbol?> Alphabet => alphabet;

    /// <summary>Gets the ordered list of defined transitions.</summary>
    protected IReadOnlyList<AutomatonInstruction<TState, TSymbol, TAction>> Transitions => transitions;

    /// <summary>
    /// Gets the action returned when no transition matches.
    /// </summary>
    protected virtual TAction DefaultAction => default;

    /// <summary>
    /// Indicates whether rules may start from a terminal state.
    /// </summary>
    protected virtual bool AllowTerminalInitialState => false;


    /// <summary>Initializes a new transition table for the given alphabet and symbol comparer.</summary>
    /// <param name="alphabetSymbols">
    /// Alphabet that defines set of allowed symbols. Duplicate items will be ignored.
    /// <c>Null</c> value means unrestricted alphabet.
    /// </param>
    /// <param name="blankSymbol">Special value that represents an empty symbol.</param>
    /// <param name="symbolComparer">Comparer used for exact symbol matching. Defaults to <see cref="EqualityComparer{T}.Default"/>.</param>
    protected TransitionTable(IEnumerable<TSymbol>? alphabetSymbols, TSymbol? blankSymbol, IEqualityComparer<TSymbol>? symbolComparer = null)
    {
        this.symbolComparer = symbolComparer ?? EqualityComparer<TSymbol>.Default;
        var alphabetComparer = new NullableComparer(this.symbolComparer);

        if (alphabetSymbols != null)
        {
            isAutoAlphabet = false;
            alphabet = new HashSet<TSymbol?>(alphabetSymbols, alphabetComparer);
            if (alphabet.Count == 0)
                throw new ArgumentException("Alphabet cannot be empty.", nameof(alphabetSymbols));
        }
        else
        {
            isAutoAlphabet = true;
            alphabet = new HashSet<TSymbol?>(alphabetComparer);
        }

        alphabet.Add(blankSymbol);
        statesDict = new Dictionary<TState, Dictionary<FuzzyKey<TSymbol>, TAction>>(AutomatonStateComparer.Instance);
        transitions = [];
        BlankSymbol = blankSymbol;
        fuzzySymbolsComparer = new FuzzyKeyComparer<TSymbol>(this.symbolComparer);
    }

    /// <summary>Creates a deep copy of another transition table.</summary>
    /// <param name="other">The transition table to copy.</param>
    protected TransitionTable(TransitionTable<TState, TSymbol, TAction> other)
    {
        isAutoAlphabet = other.isAutoAlphabet;
        symbolComparer = other.symbolComparer;
        alphabet = new HashSet<TSymbol?>(other.alphabet, other.alphabet.Comparer);
        fuzzySymbolsComparer = other.fuzzySymbolsComparer;
        statesDict = new Dictionary<TState, Dictionary<FuzzyKey<TSymbol>, TAction>>(
            other.statesDict.Count,
            other.statesDict.Comparer
        );

        foreach (var (state, symbolsDict) in other.statesDict)
            statesDict[state] = symbolsDict.ToDictionary(x => x.Key, x => x.Value, fuzzySymbolsComparer);

        transitions = [..other.transitions];
        BlankSymbol = other.BlankSymbol;
        RulesCount = other.RulesCount;
    }


    /// <summary>Adds new state with no transitions. Does nothing if state is already added.</summary>
    /// <param name="state">State object.</param>
    public void AddState(TState state)
    {
        if (!statesDict.ContainsKey(state))
            statesDict[state] = new Dictionary<FuzzyKey<TSymbol>, TAction>(alphabet.Count, fuzzySymbolsComparer);
    }

    /// <summary>Adds new transition with given condition and action. Overrides transitions with same conditions.</summary>
    /// <param name="initialState">Initial state that matches this rule.</param>
    /// <param name="symbol">Scanned symbol that matches this rule. Symbol can use fuzzy matching.</param>
    /// <param name="action">Action that will be associated with given conditions.</param>
    /// <exception cref="AlgorithmException">Initial state is terminal and not allowed.</exception>
    /// <exception cref="SymbolIsNotAllowedException">Symbol or action have invalid symbols.</exception>
    public virtual void AddRule(TState initialState, in FuzzyKey<TSymbol> symbol, in TAction action)
    {
        if (initialState.IsTerminal && !AllowTerminalInitialState)
            throw new AlgorithmException("Instruction's initial state must not be terminal.");

        ValidateSymbols(in symbol, in action);

        if (!statesDict.TryGetValue(initialState, out var symbolsDict))
        {
            symbolsDict = new Dictionary<FuzzyKey<TSymbol>, TAction>(alphabet.Count, fuzzySymbolsComparer);
            statesDict.Add(initialState, symbolsDict);
        }

        var transition = new AutomatonInstruction<TState, TSymbol, TAction>
        {
            InitialState = initialState,
            ScannedSymbol = symbol,
            Action = action,
        };

        var existingIndex = FindIndex(initialState, symbol);
        if (existingIndex >= 0)
        {
            transitions[existingIndex] = transition;
        }
        else
        {
            transitions.Add(transition);
            RulesCount++;
        }

        symbolsDict[symbol] = action;
    }

    /// <summary>Outputs the action for given state and input symbol.</summary>
    /// <param name="state">Current state of the automaton.</param>
    /// <param name="symbol">Input symbol.</param>
    /// <param name="action">Resulting action. When the returned value is <c>false</c> always equal to <see cref="DefaultAction"/>.</param>
    /// <returns><c>True</c> if such action exists in the transition table.</returns>
    public virtual bool TryFindAction(TState state, TSymbol? symbol, out TAction action)
    {
        if (TryResolveMatch(state, symbol, out _, out var resolved))
        {
            action = resolved;
            return true;
        }

        action = DefaultAction;
        return false;
    }

    /// <summary>Gets the action that is defined for given state and symbol</summary>
    /// <param name="state">State to match.</param>
    /// <param name="symbolMatch">Symbol to match.</param>
    /// <returns>Found action or <c>null</c> if it isn't defined.</returns>
    public TAction? this[TState state, in FuzzyKey<TSymbol> symbolMatch]
    {
        get
        {
            if (statesDict.TryGetValue(state, out var symbolsDict) && symbolsDict.TryGetValue(symbolMatch, out var action))
                return action;
            return null;
        }
    }

    /// <summary>Gets the zero-based definition-order index of the transition matching the given state and symbol.</summary>
    /// <param name="state">Current state of the automaton.</param>
    /// <param name="symbol">Input symbol.</param>
    /// <returns>Index of the matching transition, or <c>-1</c> when no transition matches.</returns>
    /// <remarks>Uses the same exact/empty/not-empty/any priority as <see cref="TryFindAction"/>.</remarks>
    public virtual int IndexOf(TState state, TSymbol? symbol)
    {
        return TryResolveMatch(state, symbol, out var matchedKey, out _)
            ? FindIndex(state, matchedKey)
            : -1;
    }


    /// <summary>Validates the scanned symbol and any symbols produced by the action against the alphabet.</summary>
    /// <param name="symbol">Scanned symbol of the rule.</param>
    /// <param name="action">Action of the rule.</param>
    /// <exception cref="SymbolIsNotAllowedException">A symbol is not allowed by the alphabet.</exception>
    protected virtual void ValidateSymbols(in FuzzyKey<TSymbol> symbol, in TAction action)
    {
        if (isAutoAlphabet)
        {
            if (symbol.Match == SymbolMatch.Exact)
                alphabet.Add(symbol.Value);

            foreach (var produced in GetProducedSymbols(action))
            {
                if (produced is not null)
                    alphabet.Add(produced);
            }
        }
        else
        {
            if (symbol.Match == SymbolMatch.Exact && !alphabet.Contains(symbol.Value))
                throw new SymbolIsNotAllowedException(symbol.Value!, "invalid scanned symbol");

            foreach (var produced in GetProducedSymbols(action))
            {
                if (produced is not null && !alphabet.Contains(produced))
                    throw new SymbolIsNotAllowedException(produced, "invalid printed symbol");
            }
        }
    }

    /// <summary>Gets symbols produced by the given action that must belong to the alphabet.</summary>
    /// <param name="action">Action to inspect.</param>
    /// <returns>Sequence of produced symbols; empty by default.</returns>
    protected virtual IEnumerable<TSymbol?> GetProducedSymbols(TAction action) => [];


    /// <summary>
    /// Resolves the transition matching the given state and symbol using the
    /// exact → empty/non-empty → any priority shared by all lookups.
    /// </summary>
    /// <param name="state">Current state of the automaton.</param>
    /// <param name="symbol">Input symbol.</param>
    /// <param name="matchedKey">Outputs the fuzzy key that matched when the result is <c>true</c>.</param>
    /// <param name="action">Outputs the matched action, or <see cref="DefaultAction"/> when no match exists.</param>
    /// <returns><c>True</c> when a transition matches.</returns>
    private bool TryResolveMatch(TState state, TSymbol? symbol, out FuzzyKey<TSymbol> matchedKey, out TAction action)
    {
        matchedKey = default;
        action = DefaultAction;

        if (!statesDict.TryGetValue(state, out var symbolsDict))
            return false;

        if (symbol is not null)
        {
            var exactKey = FuzzyKey<TSymbol>.Exact(symbol!);
            if (symbolsDict.TryGetValue(exactKey, out action))
            {
                matchedKey = exactKey;
                return true;
            }
        }

        var fuzzyKey = SymbolsEqual(symbol, BlankSymbol) ? FuzzyKey<TSymbol>.Empty : FuzzyKey<TSymbol>.NotEmpty;
        if (symbolsDict.TryGetValue(fuzzyKey, out action))
        {
            matchedKey = fuzzyKey;
            return true;
        }

        if (symbolsDict.TryGetValue(FuzzyKey<TSymbol>.Any, out action))
        {
            matchedKey = FuzzyKey<TSymbol>.Any;
            return true;
        }

        return false;
    }

    private bool SymbolsEqual(TSymbol? x, TSymbol? y)
    {
        if (x is null) return y is null;
        if (y is null) return false;
        return symbolComparer.Equals(x!, y!);
    }

    private sealed class NullableComparer(IEqualityComparer<TSymbol> inner) : IEqualityComparer<TSymbol?>
    {
        public bool Equals(TSymbol? x, TSymbol? y)
        {
            if (x is null) return y is null;
            if (y is null) return false;
            return inner.Equals(x!, y!);
        }

        public int GetHashCode(TSymbol? obj) => obj is null ? 0 : inner.GetHashCode(obj!);
    }

    private int FindIndex(TState state, FuzzyKey<TSymbol> symbol)
    {
        var comparer = statesDict.Comparer;
        for (var i = 0; i < transitions.Count; i++)
        {
            if (comparer.Equals(transitions[i].InitialState, state) &&
                fuzzySymbolsComparer.Equals(transitions[i].ScannedSymbol, symbol))
                return i;
        }

        return -1;
    }
}
