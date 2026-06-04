namespace PrettyMachines.Algorithms.Abstract;

/// <summary>
/// Defines an algorithm that transforms input object of type <typeparamref name="TData"/>.
/// </summary>
/// <typeparam name="TData">Type of the algorithm input and output objects.</typeparam>
public interface IAlgorithm<TData>
{
    /// <summary>Gets the name of the algorithm instance.</summary>
    public string? Name { get; }
    
    /// <summary>Executes algorithm until termination or forced cancellation.</summary>
    /// <param name="input">Initial input object.</param>
    /// <param name="cancellation">Controls when execution should stop.</param>
    /// <param name="verbose">If <c>true</c>, includes full execution trace.</param>
    /// <returns>Result containing final status, output, and optional trace.</returns>
    public AlgorithmResult<TData> Execute(TData input, AlgorithmCancellation cancellation, bool verbose = false);
    
    /// <summary>Verifies that given input is correct.</summary>
    /// <param name="input">Input object.</param>
    /// <returns><c>True</c> if object can be used as input for this algorithm.</returns>
    public bool ValidateInput(TData input);
}


public static class StringComputationalAlgorithmExtensions
{
    /// <summary>Executes algorithm <see cref="AlgorithmCancellation.Default"/> cancellation.</summary>
    /// <inheritdoc cref="IAlgorithm{TData}.Execute(TData, AlgorithmCancellation, bool)"/>
    public static AlgorithmResult<TData> Execute<TData>(this IAlgorithm<TData> algorithm, TData input, bool verbose = false)
    {
        return algorithm.Execute(input, AlgorithmCancellation.Default, verbose);
    }
}
