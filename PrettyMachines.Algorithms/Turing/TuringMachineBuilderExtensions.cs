namespace PrettyMachines.Algorithms.Turing;

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