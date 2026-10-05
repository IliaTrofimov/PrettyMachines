using PrettyMachines.Automata;


namespace PrettyMachines.FSM;

/// <summary>
/// Fluent builder interface for constructing a finite state machine step by step.
/// </summary>
public interface IFiniteStateMachineBuilder
{
    /// <summary>Defines the set of allowed symbols.</summary>
    /// <param name="alphabetSymbols">Collection of alphabet symbols.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IFiniteStateMachineBuilder WithAlphabet(IEnumerable<char> alphabetSymbols);

    /// <summary>Sets the output tokens produced on acceptance and rejection.</summary>
    /// <param name="acceptedOutput">Output produced when the input is accepted.</param>
    /// <param name="rejectedOutput">Output produced when the input is rejected.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IFiniteStateMachineBuilder WithOutput(string acceptedOutput, string rejectedOutput);

    /// <summary>Adds new state.</summary>
    /// <param name="name">Name of the state or <c>null</c> if state is unnamed.</param>
    /// <param name="isInitial">Indicates that state is starting.</param>
    /// <param name="isTerminal">Indicates that state is accepting/final.</param>
    /// <param name="state">Outputs the created state object.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IFiniteStateMachineBuilder AddState(string? name, bool isInitial, bool isTerminal, out AutomatonState state);

    /// <summary>
    /// Builds the transition rules using a nested fluent builder.
    /// </summary>
    /// <param name="builderFunc">Action that configures rules via <see cref="IFiniteStateMachineRuleBuilder"/>.</param>
    /// <returns>The fully constructed finite state machine.</returns>
    public FiniteStateMachine BuildRules(Action<IFiniteStateMachineRuleBuilder> builderFunc);
}
