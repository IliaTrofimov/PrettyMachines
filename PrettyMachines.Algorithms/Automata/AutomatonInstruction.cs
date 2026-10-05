namespace PrettyMachines.Automata;

/// <summary>
/// Represents a single transition rule shared by automata implementations.
/// </summary>
/// <typeparam name="TState">Concrete state type.</typeparam>
/// <typeparam name="TSymbol">Type of the symbols matched by this rule.</typeparam>
/// <typeparam name="TAction">Action type associated with the transition.</typeparam>
public readonly struct AutomatonInstruction<TState, TSymbol, TAction>
{
    /// <summary>Gets the state this rule starts from.</summary>
    public required TState InitialState { get; init; }

    /// <summary>Gets the scanned symbol condition of this rule.</summary>
    public required FuzzyKey<TSymbol> ScannedSymbol { get; init; }

    /// <summary>Gets the action associated with this rule.</summary>
    public required TAction Action { get; init; }
}
