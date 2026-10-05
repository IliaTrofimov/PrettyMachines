using PrettyMachines.Automata;


namespace PrettyMachines.FSM;

/// <summary>
/// Fluent builder interface for defining finite state machine transition rules.
/// </summary>
public interface IFiniteStateMachineRuleBuilder
{
    /// <summary>Adds a transition rule for given initial and next states.</summary>
    /// <param name="from">Current state.</param>
    /// <param name="scan">Symbol condition to match (exact or any).</param>
    /// <param name="to">State to transition to.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IFiniteStateMachineRuleBuilder AddRule(AutomatonState from, FuzzyKey<char> scan, AutomatonState to);

    public AutomatonState this[string name] { get; }
}
