namespace PrettyMachines.Algorithms.Abstract;

/// <summary>Consumes a lazy sequence of execution snapshots and produces the final algorithm result.</summary>
public static class AlgorithmRunner
{
    /// <summary>Consumes the given snapshots and returns the outcome of the last one.</summary>
    /// <param name="snapshots">Lazy sequence of execution snapshots.</param>
    /// <returns>Result containing final status, output, and optional trace.</returns>
    public static AlgorithmResult<string> Execute(IEnumerable<IAlgorithmSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        var (last, trace) = Consume(snapshots);

        return last is null
            ? new AlgorithmResult<string>(TerminationStatus.Aborted, string.Empty)
            : new AlgorithmResult<string>(GetTermination(last), last.Output, last.Steps, trace);
    }

    /// <inheritdoc cref="Execute(IEnumerable{IAlgorithmSnapshot})"/>
    public static AlgorithmResult<TOutput> Execute<TOutput>(IEnumerable<IAlgorithmSnapshot<TOutput>> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);

        var (last, trace) = Consume(snapshots);

        return last is null
            ? new AlgorithmResult<TOutput>(TerminationStatus.Aborted, default!)
            : new AlgorithmResult<TOutput>(GetTermination(last), last.Output, last.Steps, trace);
    }


    private static (TSnapshot? Last, List<string>? Trace) Consume<TSnapshot>(IEnumerable<TSnapshot> snapshots)
        where TSnapshot : class, IAlgorithmSnapshot
    {
        var last = default(TSnapshot);
        List<string>? trace = null;

        foreach (var snapshot in snapshots)
        {
            last = snapshot;
            if (snapshot.TraceLine is not null)
                (trace ??= []).Add(snapshot.TraceLine);
        }

        return (last, trace);
    }

    private static TerminationStatus GetTermination(IAlgorithmSnapshot snapshot)
    {
        return snapshot.IsFinished ? snapshot.Termination : TerminationStatus.Aborted;
    }
}
