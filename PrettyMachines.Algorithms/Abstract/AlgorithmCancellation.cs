namespace PrettyMachines.Algorithms.Abstract;

/// <summary>
/// Controls the execution bounds of an algorithm, combining step limits and external cancellation.
/// </summary>
public readonly struct AlgorithmCancellation
{
    /// <summary>Default cancellation configuration limiting execution to 1000 steps.</summary>
    public static readonly AlgorithmCancellation Default = new(1000);
    
    
    /// <summary>Initializes cancellation based on an external token with no step limit.</summary>
    /// <param name="token">Token for external cancellation requests.</param>
    public AlgorithmCancellation(CancellationToken token) : this(0, token)
    {
    }
    
    /// <summary>Initializes cancellation with step limit and optional external token.</summary>
    /// <param name="maxSteps">Maximum number of steps allowed. Zero means unlimited.</param>
    /// <param name="token">Token for external cancellation requests.</param>
    /// <exception cref="ArgumentException">Thrown when both maxSteps is zero and cancellation is default.</exception>
    public AlgorithmCancellation(uint maxSteps, CancellationToken token = default)
    {
        if (maxSteps == 0 && token == default)
            throw new ArgumentException("Maximum number of steps must be greater than 0.");
        
        MaxSteps = maxSteps;
        CancellationToken = token;
    }
    
    
    /// <summary>Gets maximum number of steps allowed. Zero means unlimited.</summary>
    public uint MaxSteps { get; }
    
    /// <summary>Gets token for external cancellation requests.</summary>
    public CancellationToken CancellationToken { get; }
    
    /// <summary>Determines whether the algorithm should continue execution.</summary>
    /// <param name="step">Current step number (1-indexed).</param>
    /// <returns><c>True</c> if no cancellation requested and step limit not exceeded.</returns>
    public bool ShouldContinue(uint step)
    {
        return !CancellationToken.IsCancellationRequested && (MaxSteps == 0 || step <= MaxSteps);
    }
}