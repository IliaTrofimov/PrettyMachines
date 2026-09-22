using PrettyMachines.Abstract;
using PrettyMachines.Utils.Printing;


namespace PrettyMachines.Turing;

/// <summary>Immutable snapshot of a single Turing machine execution state.</summary>
internal sealed class TuringMachineSnapshot : IAlgorithmSnapshot<IReadOnlyTape>
{
    private readonly MachineTape tape;


    /// <summary>Initializes a new snapshot over an owned tape copy.</summary>
    /// <param name="tape">Tape copy that this snapshot owns.</param>
    /// <param name="steps">Number of steps executed to reach this state.</param>
    /// <param name="termination">Termination reason for this state.</param>
    /// <param name="traceLine">Trace line produced by the transition, or <c>null</c>.</param>
    public TuringMachineSnapshot(MachineTape tape, long steps, TerminationStatus termination, string? traceLine)
    {
        this.tape = tape;
        Steps = steps;
        Termination = termination;
        TraceLine = traceLine;
    }

    public long Steps { get; }

    public TerminationStatus Termination { get; }

    public bool IsFinished => Termination is not TerminationStatus.Unknown;

    public IReadOnlyTape Output => tape;

    string IAlgorithmSnapshot.Output => MachineTapePrinter.Print(tape);

    public string? TraceLine { get; }
}
