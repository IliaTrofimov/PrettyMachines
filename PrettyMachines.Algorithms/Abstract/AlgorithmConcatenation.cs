using System.Diagnostics;

namespace PrettyMachines.Algorithms.Abstract;

/// <summary>
/// Sequence of multiple independent algorithms executed one after another one.
/// </summary>
/// <typeparam name="TAlg">Type of the used algorithm.</typeparam>
/// <typeparam name="TData">Data type for the algorithms.</typeparam>
[DebuggerDisplay("Concatenation of {Algorithms.Count} algorithms")]
public class AlgorithmConcatenation<TAlg, TData> : IAlgorithm<TData>
    where TAlg : IAlgorithm<TData>
{
    private readonly List<TAlg> algorithms;

    /// <summary>Gets the list of algorithms executed in order.</summary>
    public IReadOnlyList<TAlg> Algorithms => algorithms;
    
    /// <inheritdoc/> 
    public string? Name { get; set; }
    
    /// <summary>Initializes a new concatenation of algorithms.</summary>
    /// <param name="algorithms">The algorithms to execute in sequence.</param>
    public AlgorithmConcatenation(IEnumerable<TAlg>? algorithms = null)
    {
        this.algorithms = algorithms?.ToList() ?? [];
    }
    
    /// <summary>Initializes a new concatenation of two algorithms.</summary>
    /// <exception cref="ArgumentNullException">Thrown when algorithms are null.</exception>
    public AlgorithmConcatenation(TAlg algorithmA, TAlg algorithmB)
    {
        ArgumentNullException.ThrowIfNull(algorithmA);
        ArgumentNullException.ThrowIfNull(algorithmB);
        algorithms = [algorithmA, algorithmB];
    }

    /// <summary>Concatenates two sequences of algorithms.</summary>
    /// <returns>Modified version of the left object!</returns>
    public static AlgorithmConcatenation<TAlg, TData> operator+(AlgorithmConcatenation<TAlg, TData> concatA, AlgorithmConcatenation<TAlg, TData> concatB)
    {
        concatA.algorithms.AddRange(concatB.Algorithms);
        return concatA;
    }
    
    /// <summary>Concatenates sequence of algorithms with a single algorithm.</summary>
    /// <returns>Modified <see cref="AlgorithmConcatenation{TAlg, TData}"/> object!</returns>
    public static AlgorithmConcatenation<TAlg, TData> operator+(AlgorithmConcatenation<TAlg, TData> concat, TAlg algorithm)
    {
        concat.algorithms.Add(algorithm);
        return concat;
    }
    
    
    /// <inheritdoc cref="IAlgorithm{TData}.Execute"/>
    /// <summary>Executes multiple algorithms sequentially, feeding the output of each as input to the next.</summary> 
    public AlgorithmResult<TData> Execute(TData input, AlgorithmCancellation cancellation, bool verbose = false)
    {
        if (algorithms.Count == 0)
            throw new InvalidOperationException("No algorithms were added.");
        
        long steps = 0;
        var tempOutput = input;
        var termination = TerminationStatus.Aborted;
        List<string>? trace = verbose ? [] : null;
        
        for (var i = 0; i < Algorithms.Count; i++)
        {
            var algorithm = Algorithms[i];
            var result = algorithm.Execute(tempOutput, cancellation, verbose);
            steps += result.Steps;
            tempOutput = result.Output;
            termination = result.Termination;
            
            trace?.Add(CreateAlgorithmStartTrace(i));
            trace?.AddRange(result.Trace);

            if (result.Termination != TerminationStatus.Success)
            {
                trace?.Add(CreateAlgorithmFailTrace(i, termination));
                break;
            }
        }
        
        return new AlgorithmResult<TData>(termination, tempOutput, steps, trace);
    }

    private string CreateAlgorithmStartTrace(int index)
    {
        var algorithm = algorithms[index];
        return string.IsNullOrWhiteSpace(algorithm.Name) 
            ? $"// Algorithm {index} started"
            : $"// Algorithm {index} '{algorithm.Name}' started";
    }
    
    private string CreateAlgorithmFailTrace(int index, TerminationStatus termination)
    {
        var algorithm = algorithms[index];
        return string.IsNullOrWhiteSpace(algorithm.Name) 
            ? $"// Algorithm {index} finished with {termination}"
            : $"// Algorithm {index} '{algorithm.Name}' finished with {termination}";
    }
}