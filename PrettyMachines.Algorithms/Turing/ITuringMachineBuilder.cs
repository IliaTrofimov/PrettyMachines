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


public static class TuringMachineBuilderExtensions
{
    /// <inheritdoc cref="ITuringMachineBuilder.WithAlphabet(IEnumerable{string})"/> 
    public static ITuringMachineBuilder WithAlphabet(this ITuringMachineBuilder b, params string[] alphabetSymbols)
    {
        return b.WithAlphabet(alphabetSymbols.AsEnumerable());
    }

    /// <summary>Defines the alphabet with blank symbol and comparison mode in one call.</summary>
    /// <param name="alphabetSymbols">Collection of alphabet symbols.</param>
    /// <param name="blankSymbol">The blank symbol to use.</param>
    /// <param name="comparison">String comparison mode for symbol matching.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder WithAlphabet(this ITuringMachineBuilder b, 
                                                     IEnumerable<string> alphabetSymbols,
                                                     string? blankSymbol,
                                                     StringComparison comparison = StringComparison.Ordinal)
    {
        return b.WithAlphabet(alphabetSymbols).WithBlankSymbol(blankSymbol).WithStringComparer(comparison);
    }

    /// <inheritdoc cref="ITuringMachineBuilder.WithMarkers(IEnumerable{string})"/> 
    public static ITuringMachineBuilder WithMarkers(this ITuringMachineBuilder b, params string[] markerSymbols)
    {
        return b.WithMarkers(markerSymbols.AsEnumerable());
    }
    
    // ------------------

    /// <summary>Adds a new state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder AddState(this ITuringMachineBuilder b, string name, out TuringMachineState state)
    {
        return b.AddState(name, false, false, out state);
    }
    
    /// <summary>Adds a new state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder AddState(this ITuringMachineBuilder b, string name)
    {
        return b.AddState(name, false, false, out _);
    }
    
    /// <summary>Adds a new state without a name.</summary>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder AddState(this ITuringMachineBuilder b, out TuringMachineState state)
    {
        return b.AddState(null, false, false, out state);
    }
    
    // ------------------
    
    /// <summary>Adds a new terminal state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder AddTerminalState(this ITuringMachineBuilder b, string name, out TuringMachineState state)
    {
        return b.AddState(name, false, true, out state);
    }
    
    /// <summary>Adds a new terminal state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder AddTerminalState(this ITuringMachineBuilder b, string name)
    {
        return b.AddState(name, false, true, out _);
    }
    
    /// <summary>Adds a new terminal state without a name.</summary>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder AddTerminalState(this ITuringMachineBuilder b, out TuringMachineState state)
    {
        return b.AddState(null, false, true, out state);
    }
    
    // ------------------
    
    /// <summary>Adds a new start state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder AddInitialState(this ITuringMachineBuilder b, string name, out TuringMachineState state)
    {
        return b.AddState(name, true, false, out state);
    }
    
    /// <summary>Adds a new start state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder AddInitialState(this ITuringMachineBuilder b, string name)
    {
        return b.AddState(name, true, false, out _);
    }
    
    /// <summary>Adds a new start state without a name.</summary>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder AddInitialState(this ITuringMachineBuilder b, out TuringMachineState state)
    {
        return b.AddState(null, true, false, out state);
    }
    
    // ------------------

    /// <summary>Adds multiple states with the given names.</summary>
    /// <param name="stateNames">Names of states to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder AddStates(this ITuringMachineBuilder b, params string[] stateNames)
    {
        foreach (var name in stateNames)
            b.AddState(name, false, false, out _);
        return b;
    }

    /// <summary>Adds multiple terminal states with the given names.</summary>
    /// <param name="stateNames">Names of terminal states to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineBuilder AddTerminalStates(this ITuringMachineBuilder b, params string[] stateNames)
    {
        foreach (var name in stateNames)
            b.AddState(name, false, true, out _);
        return b;
    }
}