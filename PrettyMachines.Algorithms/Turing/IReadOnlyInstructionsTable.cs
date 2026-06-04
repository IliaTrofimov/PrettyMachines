namespace PrettyMachines.Algorithms.Turing;

/// <summary>
/// Allows readonly access for the instructions of the Turing machine.
/// </summary>
public interface IReadOnlyInstructionsTable : IEnumerable<TuringMachineInstruction>
{
    /// <summary>Gets collection of allowed symbols. Blank symbol is always included.</summary>
    IReadOnlySet<string?> Alphabet { get; }

    /// <summary>Gets total number of added instructions.</summary>
    /// <remarks>Always less or equal than <i>States.Count</i> * <i>Alphabet.Count</i>.</remarks>
    int RulesCount { get; }

    /// <summary>Gets special value that represents an empty symbol.</summary>
    string? BlankSymbol { get; init; }

    /// <summary>Gets the collection of all defined states.</summary>
    IReadOnlyCollection<TuringMachineState> States { get; }

    /// <summary>Gets the action that is defined for given state and symbol</summary>
    /// <param name="state">State to match.</param>
    /// <param name="symbolMatch">Symbol to match.</param>
    /// <returns>Found action or <c>null</c> if it isn't defined.</returns>
    public TuringMachineAction? this[TuringMachineState state, in FuzzyKey<string> symbolMatch]
    {
        get;
    }
}