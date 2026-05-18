namespace PrettyMachines.Algorithms.Turing;

/// <summary>
/// Fluent builder interface for constructing a Turing machine step by step.
/// </summary>
public interface ITuringMachineBuilder
{
    /// <summary>Sets the string comparison mode for symbol matching.</summary>
    /// <param name="comparison">The string comparison type to use.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ITuringMachineBuilder WithStringComparer(StringComparison comparison);
    
    /// <summary>Sets the symbol used to represent blank cells on the tape.</summary>
    /// <param name="blankSymbol">The blank symbol.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ITuringMachineBuilder WithBlankSymbol(string? blankSymbol);
    
    /// <summary>Defines the set of allowed symbols.</summary>
    /// <param name="alphabetSymbols">Collection of alphabet symbols.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ITuringMachineBuilder WithAlphabet(IEnumerable<string> alphabetSymbols);
    
    /// <summary>
    /// Defines special marker symbols used for tape positioning or control.
    /// </summary>
    /// <param name="markerSymbols">Collection of marker symbols.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ITuringMachineBuilder WithMarkers(IEnumerable<string> markerSymbols);
    
    /// <summary>Adds new state.</summary>
    /// <param name="name">Name of the state or <c>null</c> if state is unnamed.</param>
    /// <param name="isInitial">Indicates that state is starting.</param>
    /// <param name="isTerminal">Indicates that state is final.</param>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ITuringMachineBuilder AddState(string? name, bool isInitial, bool isTerminal, out TuringMachineState state);
    
    /// <summary>
    /// Builds the transition rules using a nested fluent builder.
    /// </summary>
    /// <param name="builderFunc">Action that configures rules via <see cref="ITuringMachineRuleBuilder"/>.</param>
    /// <returns>The fully constructed Turing machine.</returns>
    public TuringMachine BuildRules(Action<ITuringMachineRuleBuilder> builderFunc);
}