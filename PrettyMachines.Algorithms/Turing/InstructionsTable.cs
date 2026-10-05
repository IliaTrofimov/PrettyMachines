using System.Collections;
using System.Diagnostics;
using PrettyMachines.Automata;


namespace PrettyMachines.Turing;

/// <summary>
/// Set of conditions and corresponding actions that define Turing machine instructions.
/// </summary>
[DebuggerDisplay("Rules: {RulesCount}, stats: {States.Count}, symbols: {Alphabet.Count}")]
public class InstructionsTable : TransitionTable<TuringMachineState, string, TuringMachineAction>, IReadOnlyInstructionsTable
{
    /// <summary>Gets the action returned when no transition matches.</summary>
    protected override TuringMachineAction DefaultAction => TuringMachineAction.Halt;


    /// <summary>Initializes new instructions table with given set of allowed symbols and string comparison type.</summary>
    /// <param name="alphabetSymbols">Alphabet that defines set of allowed symbols. Duplicate items will be ignored.</param>
    /// <param name="symbolsComparison">String comparison mode for the symbols.</param>
    public InstructionsTable(IEnumerable<string> alphabetSymbols, StringComparison symbolsComparison = StringComparison.Ordinal)
        : this(alphabetSymbols, null, symbolsComparison)
    {
    }

    /// <summary>
    /// Initializes new instructions table with an unrestricted alphabet, predefined empty symbol and string comparison type.
    /// </summary>
    /// <inheritdoc cref="InstructionsTable(IEnumerable{string}?,string?,StringComparison)"/>
    public InstructionsTable(string? blankSymbol, StringComparison symbolsComparison = StringComparison.Ordinal)
        : this(null, blankSymbol, symbolsComparison)
    {
    }
    
    /// <summary>
    /// Initializes new instructions table with an unrestricted alphabet set of allowed symbols and string comparison type.
    /// </summary>
    /// <inheritdoc cref="InstructionsTable(IEnumerable{string}?,string?,StringComparison)"/>
    public InstructionsTable(StringComparison symbolsComparison = StringComparison.Ordinal) 
        : this(null, null, symbolsComparison)
    {
    }
    
    /// <summary>
    /// Initializes new instructions table with given set of allowed symbols, predefined empty symbol and string comparison type.
    /// </summary>
    /// <param name="alphabetSymbols">
    /// Alphabet that defines set of allowed symbols. Duplicate items will be ignored.
    /// <c>Null</c> value means unrestricted alphabet.
    /// </param>
    /// <param name="blankSymbol">Special value that represents an empty symbol.</param>
    /// <param name="symbolsComparison">String comparison mode for the symbols.</param>
    public InstructionsTable(IEnumerable<string>? alphabetSymbols, string? blankSymbol, StringComparison symbolsComparison = StringComparison.Ordinal)
        : base(alphabetSymbols, blankSymbol, StringComparer.FromComparison(symbolsComparison))
    {
    }

    /// <summary>Creates a deep copy of another instructions table.</summary>
    /// <param name="other">The instructions to copy.</param>
    public InstructionsTable(InstructionsTable other) : base(other)
    {
    }
    
    
    /// <summary>Gets the symbols produced by the given action that must belong to the alphabet.</summary>
    /// <param name="action">Action to inspect.</param>
    /// <returns>The printed symbol of the action, or an empty sequence.</returns>
    protected override IEnumerable<string?> GetProducedSymbols(TuringMachineAction action) => [action.PrintedSymbol];

    /// <summary>Adds new instruction with given condition and action. Overrides instructions with same conditions.</summary>
    /// <param name="initialState">Initial state that matches this rule.</param>
    /// <param name="symbol">Scanned symbol that matches this rule. Symbol can use fuzzy matching (not empty, empty, any).</param>
    /// <param name="action">Action that will be associated with given conditions.</param>
    /// <exception cref="AlgorithmException">Initial state is terminal.</exception>
    /// <exception cref="SymbolIsNotAllowedException">Symbol or action have invalid symbols.</exception>
    public override void AddRule(TuringMachineState initialState, in FuzzyKey<string> symbol, in TuringMachineAction action)
    {
        base.AddRule(initialState, in symbol, in action);

        if (action.NextState.Equals(TuringMachineState.Halt) && !States.Contains(action.NextState))
            AddState(action.NextState);
    }

    public IEnumerator<TuringInstruction> GetEnumerator()
    {
        foreach (var transition in Transitions)
        {
            yield return new TuringInstruction
            {
                InitialState = transition.InitialState,
                ScannedSymbol = transition.ScannedSymbol,
                NextState = transition.Action.NextState,
                PrintedSymbol = transition.Action.PrintedSymbol,
                Movement = transition.Action.Movement,
            };
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
