namespace PrettyMachines.Abstract;

/// <summary>
/// Defines an algorithm that transforms text string.
/// </summary>
public interface IAlgorithm
{
    /// <summary>Gets the name of the algorithm instance.</summary>
    public string? Name { get; }

    /// <summary>Verifies that given input is correct.</summary>
    /// <param name="input">Input object.</param>
    /// <returns><c>True</c> if object can be used as input for this algorithm.</returns>
    public bool ValidateInput(string input);

    /// <summary>Lazily produces the sequence of immutable execution states for the given input.</summary>
    /// <param name="input">Initial input object.</param>
    /// <param name="cancellation">Controls when execution should stop.</param>
    /// <param name="verbose">If <c>true</c>, snapshots carry trace lines.</param>
    /// <returns>Initial snapshot followed by one snapshot per executed step.</returns>
    public IEnumerable<IAlgorithmSnapshot> Run(string input, AlgorithmCancellation cancellation, bool verbose = false);

    /// <summary>Executes algorithm until termination or forced cancellation.</summary>
    /// <param name="input">Initial input object.</param>
    /// <param name="cancellation">Controls when execution should stop.</param>
    /// <param name="verbose">If <c>true</c>, includes full execution trace.</param>
    /// <returns>Result containing final status, output, and optional trace.</returns>
    public AlgorithmResult<string> Execute(string input, AlgorithmCancellation cancellation, bool verbose = false);
}


/// <summary>Convenience execution helpers for <see cref="IAlgorithm"/>.</summary>
public static class AlgorithmExecutionExtensions
{
    /// <summary>Executes algorithm with the default cancellation bounds.</summary>
    /// <param name="algorithm">Algorithm to execute.</param>
    /// <param name="input">Initial input object.</param>
    /// <param name="verbose">If <c>true</c>, includes full execution trace.</param>
    /// <returns>Result containing final status, output, and optional trace.</returns>
    public static AlgorithmResult<string> Execute(this IAlgorithm algorithm, string input, bool verbose = false)
        => algorithm.Execute(input, AlgorithmCancellation.Default, verbose);
}
