using System.Runtime.CompilerServices;


namespace PrettyMachines.Algorithms.Abstract;

/// <summary>
/// Forward-and-backward cursor over a lazily produced snapshot sequence.
/// Rewinding replays the sequence from its factory; no snapshot history is stored.
/// </summary>
/// <typeparam name="TSnapshot">Type of the snapshots produced by the source.</typeparam>
public class ExecutionCursor<TSnapshot> where TSnapshot : IAlgorithmSnapshot
{
    private readonly Func<IEnumerable<TSnapshot>> source;
    private IEnumerator<TSnapshot>? enumerator;
    private TSnapshot current = default!;
    private bool hasCurrent;
    private long index = -1;


    /// <summary>Initializes a new cursor positioned at the first snapshot.</summary>
    /// <param name="source">Factory that produces a fresh snapshot sequence on demand.</param>
    public ExecutionCursor(Func<IEnumerable<TSnapshot>> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        this.source = source;
        Reset();
    }


    /// <summary>Gets the snapshot the cursor is currently positioned on.</summary>
    /// <exception cref="InvalidOperationException">Thrown when the source produced no snapshots.</exception>
    public TSnapshot Current => hasCurrent ? current : throw new InvalidOperationException("Cursor is not positioned on a snapshot.");

    /// <summary>Gets the zero-based index of the current snapshot. The initial snapshot is at index 0.</summary>
    public long Index => index;

    /// <summary>Indicates whether the cursor is positioned on the initial snapshot.</summary>
    public bool IsAtStart => index <= 0;


    /// <summary>Advances the live enumerator to the next snapshot.</summary>
    /// <returns><c>True</c> if the cursor moved; <c>False</c> at the end of the sequence.</returns>
    public bool MoveNext()
    {
        if (enumerator is null || !enumerator.MoveNext())
            return false;

        current = enumerator.Current;
        hasCurrent = true;
        index++;
        return true;
    }

    /// <summary>Moves the cursor one snapshot backwards by replaying the source.</summary>
    /// <returns><c>True</c> if the cursor moved; <c>False</c> at the start of the sequence.</returns>
    public bool MovePrevious() => index > 0 && MoveTo(index - 1);

    /// <summary>Moves the cursor to the given index by replaying the source from step zero.</summary>
    /// <param name="target">Zero-based target index.</param>
    /// <returns><c>True</c> if the cursor reached the target; otherwise <c>False</c>.</returns>
    public bool MoveTo(long target)
    {
        if (target < 0)
            return false;

        if (target == index && hasCurrent)
            return true;

        enumerator = source().GetEnumerator();
        index = -1;
        hasCurrent = false;

        while (index < target && enumerator.MoveNext())
        {
            current = enumerator.Current;
            hasCurrent = true;
            index++;
        }

        return index == target;
    }

    /// <summary>Moves the cursor back to the initial snapshot.</summary>
    public void Reset() => MoveTo(0);
}


/// <summary>
/// Forward-and-backward cursor over a string-pinned algorithm execution.
/// </summary>
public sealed class ExecutionCursor : ExecutionCursor<IAlgorithmSnapshot>
{
    /// <summary>Initializes a new cursor for the given algorithm and input.</summary>
    /// <param name="algorithm">Algorithm to run.</param>
    /// <param name="input">Initial input object.</param>
    /// <param name="cancellation">Controls when execution should stop.</param>
    /// <param name="verbose">If <c>true</c>, snapshots carry trace lines.</param>
    public ExecutionCursor(IAlgorithm algorithm, string input, AlgorithmCancellation cancellation, bool verbose = false)
        : base(CreateSource(algorithm, input, cancellation, verbose))
    {
    }


    private static Func<IEnumerable<IAlgorithmSnapshot>> CreateSource(IAlgorithm algorithm, string input,
                                                                     AlgorithmCancellation cancellation, bool verbose)
    {
        ArgumentNullException.ThrowIfNull(algorithm);
        ArgumentNullException.ThrowIfNull(input);
        return () => algorithm.Run(input, cancellation, verbose);
    }
}


/// <summary>Factory helpers for <see cref="ExecutionCursor{TSnapshot}"/>.</summary>
public static class ExecutionCursorExtensions
{
    /// <summary>Creates a cursor over the given algorithm execution.</summary>
    public static ExecutionCursor ToCursor(this IAlgorithm algorithm, string input, AlgorithmCancellation cancellation, bool verbose = false)
        => new(algorithm, input, cancellation, verbose);

    /// <summary>Creates a typed cursor over the given algorithm execution.</summary>
    /// <remarks>
    /// Preferred over the non-generic overload for algorithms that implement both interfaces,
    /// which would otherwise be ambiguous.
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static ExecutionCursor<IAlgorithmSnapshot<TOutput>> ToCursor<TInput, TOutput>(
        this IAlgorithm<TInput, TOutput> algorithm, TInput input, AlgorithmCancellation cancellation, bool verbose = false)
    {
        ArgumentNullException.ThrowIfNull(algorithm);
        return new ExecutionCursor<IAlgorithmSnapshot<TOutput>>(() => algorithm.Run(input, cancellation, verbose));
    }
}
