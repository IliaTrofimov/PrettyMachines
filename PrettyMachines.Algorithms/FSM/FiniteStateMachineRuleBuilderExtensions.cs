using PrettyMachines.Automata;


namespace PrettyMachines.FSM;

/// <summary>Convenience extensions for <see cref="IFiniteStateMachineRuleBuilder"/>.</summary>
public static class FiniteStateMachineRuleBuilderExtensions
{


    /// <inheritdoc cref="IFiniteStateMachineRuleBuilder.AddRule(AutomatonState,FuzzyKey{char},AutomatonState)"/>
    public static IFiniteStateMachineRuleBuilder AddRule(this IFiniteStateMachineRuleBuilder b,
                                                         string from,
                                                         FuzzyKey<char> scan,
                                                         AutomatonState to)
    {
        return b.AddRule(b[from], scan, to);
    }

    /// <inheritdoc cref="IFiniteStateMachineRuleBuilder.AddRule(AutomatonState,FuzzyKey{char},AutomatonState)"/>
    public static IFiniteStateMachineRuleBuilder AddRule(this IFiniteStateMachineRuleBuilder b,
                                                         AutomatonState from,
                                                         FuzzyKey<char> scan,
                                                         string to)
    {
        return b.AddRule(from, scan, b[to]);
    }

    /// <inheritdoc cref="IFiniteStateMachineRuleBuilder.AddRule(AutomatonState,FuzzyKey{char},AutomatonState)"/>
    public static IFiniteStateMachineRuleBuilder AddRule(this IFiniteStateMachineRuleBuilder b,
                                                         string from,
                                                         FuzzyKey<char> scan,
                                                         string to)
    {
        return b.AddRule(b[from], scan, b[to]);
    }

    // ------------------

    /// <inheritdoc cref="IFiniteStateMachineRuleBuilder.AddRule(AutomatonState,FuzzyKey{char},AutomatonState)"/>
    public static IFiniteStateMachineRuleBuilder AddRule(this IFiniteStateMachineRuleBuilder b,
                                                         AutomatonState from,
                                                         char scan,
                                                         AutomatonState to)
    {
        return b.AddRule(from, new FuzzyKey<char>(scan), to);
    }

    /// <inheritdoc cref="IFiniteStateMachineRuleBuilder.AddRule(AutomatonState,FuzzyKey{char},AutomatonState)"/>
    public static IFiniteStateMachineRuleBuilder AddRule(this IFiniteStateMachineRuleBuilder b,
                                                         string from,
                                                         char scan,
                                                         AutomatonState to)
    {
        return b.AddRule(b[from], new FuzzyKey<char>(scan), to);
    }

    /// <inheritdoc cref="IFiniteStateMachineRuleBuilder.AddRule(AutomatonState,FuzzyKey{char},AutomatonState)"/>
    public static IFiniteStateMachineRuleBuilder AddRule(this IFiniteStateMachineRuleBuilder b,
                                                         AutomatonState from,
                                                         char scan,
                                                         string to)
    {
        return b.AddRule(from, new FuzzyKey<char>(scan), b[to]);
    }

    /// <inheritdoc cref="IFiniteStateMachineRuleBuilder.AddRule(AutomatonState,FuzzyKey{char},AutomatonState)"/>
    public static IFiniteStateMachineRuleBuilder AddRule(this IFiniteStateMachineRuleBuilder b,
                                                         string from,
                                                         char scan,
                                                         string to)
    {
        return b.AddRule(b[from], new FuzzyKey<char>(scan), b[to]);
    }

    // ------------------

    /// <inheritdoc cref="IFiniteStateMachineRuleBuilder.AddRule(AutomatonState,FuzzyKey{char},AutomatonState)"/>
    public static IFiniteStateMachineRuleBuilder AddRule(this IFiniteStateMachineRuleBuilder b,
                                                         AutomatonState from,
                                                         SymbolMatch scan,
                                                         AutomatonState to)
    {
        return b.AddRule(from, new FuzzyKey<char>(default, scan), to);
    }

    /// <inheritdoc cref="IFiniteStateMachineRuleBuilder.AddRule(AutomatonState,FuzzyKey{char},AutomatonState)"/>
    public static IFiniteStateMachineRuleBuilder AddRule(this IFiniteStateMachineRuleBuilder b,
                                                         string from,
                                                         SymbolMatch scan,
                                                         AutomatonState to)
    {
        return b.AddRule(b[from], new FuzzyKey<char>(default, scan), to);
    }

    /// <inheritdoc cref="IFiniteStateMachineRuleBuilder.AddRule(AutomatonState,FuzzyKey{char},AutomatonState)"/>
    public static IFiniteStateMachineRuleBuilder AddRule(this IFiniteStateMachineRuleBuilder b,
                                                         AutomatonState from,
                                                         SymbolMatch scan,
                                                         string to)
    {
        return b.AddRule(from, new FuzzyKey<char>(default, scan), b[to]);
    }

    /// <inheritdoc cref="IFiniteStateMachineRuleBuilder.AddRule(AutomatonState,FuzzyKey{char},AutomatonState)"/>
    public static IFiniteStateMachineRuleBuilder AddRule(this IFiniteStateMachineRuleBuilder b,
                                                         string from,
                                                         SymbolMatch scan,
                                                         string to)
    {
        return b.AddRule(b[from], new FuzzyKey<char>(default, scan), b[to]);
    }
}
