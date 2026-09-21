using PrettyMachines.Algorithms.Abstract;


namespace PrettyMachines.Algorithms.Markov;

/// <summary>Immutable snapshot of a single Markov algorithm execution state.</summary>
internal sealed class MarkovAlgorithmSnapshot : IAlgorithmSnapshot<string>
{
    /// <summary>Initializes a new snapshot.</summary>
    /// <param name="output">Resulting string at this state.</param>
    /// <param name="steps">Number of steps executed to reach this state.</param>
    /// <param name="termination">Termination reason for this state.</param>
    /// <param name="traceLine">Trace line produced by the transition, or <c>null</c>.</param>
    public MarkovAlgorithmSnapshot(string output, long steps, TerminationStatus termination, string? traceLine)
    {
        Output = output;
        Steps = steps;
        Termination = termination;
        TraceLine = traceLine;
    }

    public long Steps { get; }

    public TerminationStatus Termination { get; }

    public bool IsFinished => Termination is not TerminationStatus.Unknown;

    public string Output { get; }

    public string? TraceLine { get; }
}
