using PrettyMachines.Automata;


namespace PrettyMachines.FSM;

/// <summary>Convenience extensions for <see cref="IFiniteStateMachineBuilder"/>.</summary>
public static class FiniteStateMachineBuilderExtensions
{
    /// <inheritdoc cref="IFiniteStateMachineBuilder.WithAlphabet(IEnumerable{char})"/>
    public static IFiniteStateMachineBuilder WithAlphabet(this IFiniteStateMachineBuilder b, params char[] alphabetSymbols)
    {
        return b.WithAlphabet(alphabetSymbols.AsEnumerable());
    }

    /// <summary>Defines the alphabet from the characters of a single string.</summary>
    /// <param name="alphabetSymbols">String whose characters become alphabet symbols.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder WithSingleCharAlphabet(this IFiniteStateMachineBuilder b, string alphabetSymbols)
    {
        return b.WithAlphabet(alphabetSymbols.AsEnumerable());
    }

    // ------------------

    /// <summary>Adds a new state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder AddState(this IFiniteStateMachineBuilder b, string name, out AutomatonState state)
    {
        return b.AddState(name, false, false, out state);
    }

    /// <summary>Adds a new state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder AddState(this IFiniteStateMachineBuilder b, string name)
    {
        return b.AddState(name, false, false, out _);
    }

    /// <summary>Adds a new state without a name.</summary>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder AddState(this IFiniteStateMachineBuilder b, out AutomatonState state)
    {
        return b.AddState(null, false, false, out state);
    }

    // ------------------

    /// <summary>Adds a new accepting state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder AddTerminalState(this IFiniteStateMachineBuilder b, string name, out AutomatonState state)
    {
        return b.AddState(name, false, true, out state);
    }

    /// <summary>Adds a new accepting state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder AddTerminalState(this IFiniteStateMachineBuilder b, string name)
    {
        return b.AddState(name, false, true, out _);
    }

    /// <summary>Adds a new accepting state without a name.</summary>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder AddTerminalState(this IFiniteStateMachineBuilder b, out AutomatonState state)
    {
        return b.AddState(null, false, true, out state);
    }

    // ------------------

    /// <summary>Adds a new start state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder AddInitialState(this IFiniteStateMachineBuilder b, string name, out AutomatonState state)
    {
        return b.AddState(name, true, false, out state);
    }

    /// <summary>Adds a new start state with a specified name.</summary>
    /// <param name="name">The name of the state.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder AddInitialState(this IFiniteStateMachineBuilder b, string name)
    {
        return b.AddState(name, true, false, out _);
    }

    /// <summary>Adds a new start state without a name.</summary>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder AddInitialState(this IFiniteStateMachineBuilder b, out AutomatonState state)
    {
        return b.AddState(null, true, false, out state);
    }

    // ------------------

    /// <summary>Adds multiple states with the given names.</summary>
    /// <param name="stateNames">Names of states to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder AddStates(this IFiniteStateMachineBuilder b, params string[] stateNames)
    {
        foreach (var name in stateNames)
            b.AddState(name, false, false, out _);
        return b;
    }

    /// <summary>Adds multiple accepting states with the given names.</summary>
    /// <param name="stateNames">Names of accepting states to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IFiniteStateMachineBuilder AddTerminalStates(this IFiniteStateMachineBuilder b, params string[] stateNames)
    {
        foreach (var name in stateNames)
            b.AddState(name, false, true, out _);
        return b;
    }
}
