using PrettyMachines.Abstract;


namespace PrettyMachines.FSM;

/// <summary>Immutable snapshot of a single finite state machine execution state.</summary>
internal sealed class FiniteStateMachineSnapshot : IAlgorithmSnapshot<string>
{
    /// <summary>Initializes a new snapshot.</summary>
    /// <param name="steps">Number of steps executed to reach this state.</param>
    /// <param name="termination">Termination reason for this state.</param>
    /// <param name="output">Output at this state.</param>
    /// <param name="traceLine">Trace line produced by the transition, or <c>null</c>.</param>
    /// <param name="appliedInstruction">Zero-based index of the transition applied to reach this state, or <c>-1</c>.</param>
    public FiniteStateMachineSnapshot(long steps, TerminationStatus termination, string output, string? traceLine, int appliedInstruction = -1)
    {
        Steps = steps;
        Termination = termination;
        Output = output;
        TraceLine = traceLine;
        AppliedInstruction = appliedInstruction;
    }

    public long Steps { get; }

    public TerminationStatus Termination { get; }

    public bool IsFinished => Termination is not TerminationStatus.Unknown;

    public string Output { get; }

    public string? TraceLine { get; }

    public int AppliedInstruction { get; }
}
