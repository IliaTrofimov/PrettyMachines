namespace PrettyMachines.Abstract;

/// <summary>
/// Represents an immutable point-in-time state of an algorithm execution.
/// </summary>
public interface IAlgorithmSnapshot
{
    /// <summary>Gets the number of steps executed to reach this state.</summary>
    public long Steps { get; }

    /// <summary>Gets the termination reason. Equal to <see cref="TerminationStatus.Unknown"/> while execution is still running.</summary>
    public TerminationStatus Termination { get; }

    /// <summary>Indicates whether this snapshot represents a terminal state.</summary>
    public bool IsFinished { get; }

    /// <summary>Gets the output of this state rendered as text.</summary>
    public string Output { get; }

    /// <summary>Gets the trace line produced by the transition into this state, or <c>null</c> unless verbose.</summary>
    public string? TraceLine { get; }

    /// <summary>
    /// Gets the zero-based index of the instruction used to reach this state,
    /// or <c>-1</c> when no instruction has been applied (initial or error state).
    /// </summary>
    public int AppliedInstruction { get; }
}


/// <summary>
/// Represents an immutable point-in-time state of an algorithm execution with strongly-typed output.
/// </summary>
/// <typeparam name="TOutput">Type of the algorithm output objects.</typeparam>
public interface IAlgorithmSnapshot<TOutput> : IAlgorithmSnapshot
{
    /// <summary>Gets the strongly-typed output of this state.</summary>
    public new TOutput Output { get; }
}
