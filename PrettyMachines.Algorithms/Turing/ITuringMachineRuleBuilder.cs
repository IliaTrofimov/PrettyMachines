namespace PrettyMachines.Algorithms.Turing;

/// <summary>
/// Fluent builder interface for defining Turing machine transition rules.
/// </summary>
public interface ITuringMachineRuleBuilder
{
    /// <summary>Adds a transition rule for given initial and next states.</summary>
    /// <param name="initialState">Current state.</param>
    /// <param name="scan">Symbol to read from the tape.</param>
    /// <param name="nextState">State to transition to.</param>
    /// <param name="print">Symbol to write to the tape or <c>null</c> to write nothing.</param>
    /// <param name="move">Direction to move the tape head.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ITuringMachineRuleBuilder AddRule(TuringMachineState initialState,
                                             FuzzyKey<string> scan,
                                             TuringMachineState nextState,
                                             string? print,
                                             TapeMovement move = TapeMovement.None);
    
    public TuringMachineState this[string name] { get; }
}