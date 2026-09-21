namespace PrettyMachines.Algorithms.Abstract;

/// <summary>
/// Defines an algorithm that transforms input objects of type <typeparamref name="TInput"/>
/// into output objects of type <typeparamref name="TOutput"/>.
/// </summary>
/// <typeparam name="TInput">Type of the algorithm input objects.</typeparam>
/// <typeparam name="TOutput">Type of the algorithm output objects.</typeparam>
/// <remarks>
/// Typed access is optional: every algorithm also implements the type-agnostic <see cref="IAlgorithm"/>.
/// </remarks>
public interface IAlgorithm<TInput, TOutput>
{
    /// <summary>Gets the name of the algorithm instance.</summary>
    public string? Name { get; }

    /// <summary>Verifies that given input is correct.</summary>
    /// <param name="input">Input object.</param>
    /// <returns><c>True</c> if object can be used as input for this algorithm.</returns>
    public bool ValidateInput(TInput input);

    /// <summary>Lazily produces the sequence of immutable execution states for the given input.</summary>
    /// <param name="input">Initial input object.</param>
    /// <param name="cancellation">Controls when execution should stop.</param>
    /// <param name="verbose">If <c>true</c>, snapshots carry trace lines.</param>
    /// <returns>Initial snapshot followed by one snapshot per executed step.</returns>
    public IEnumerable<IAlgorithmSnapshot<TOutput>> Run(TInput input, AlgorithmCancellation cancellation, bool verbose = false);

    /// <summary>Executes algorithm until termination or forced cancellation.</summary>
    /// <param name="input">Initial input object.</param>
    /// <param name="cancellation">Controls when execution should stop.</param>
    /// <param name="verbose">If <c>true</c>, includes full execution trace.</param>
    /// <returns>Result containing final status, output, and optional trace.</returns>
    public AlgorithmResult<TOutput> Execute(TInput input, AlgorithmCancellation cancellation, bool verbose = false);
}
