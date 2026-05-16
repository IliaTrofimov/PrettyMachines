namespace PrettyMachines.Algorithms.Abstract;

/// <summary>Represents the outcome of an algorithm execution.</summary>
public class AlgorithmResult<T>
{
    /// <summary>Gets the termination reason.</summary>
    public TerminationStatus Termination { get; init; }

    /// <summary>Gets the final output string produced by the algorithm.</summary>
    public T Output { get; init; }

    /// <summary>Gets the detailed execution trace.</summary>
    public long Steps { get; init; }

    /// <summary>Additional information about each executed step.</summary>
    public IReadOnlyList<string> Trace { get; init; }

    
    /// <summary>Initializes a new result.</summary>
    /// <param name="termination">How the algorithm terminated.</param>
    /// <param name="output">Final string after execution.</param>
    /// <param name="steps">Total number of steps executed.</param>
    /// <param name="trace">Complete step-by-step execution log.</param>
    public AlgorithmResult(TerminationStatus termination, T output, long steps, IReadOnlyList<string>? trace = null)
    {
        Termination = termination;
        Output = output;
        Steps = steps;
        Trace = trace ?? [];
    }

    /// <summary>Initializes a new result with no steps taken.</summary>
    /// <param name="termination">How the algorithm terminated.</param>
    /// <param name="output">Final string after execution.</param>
    public AlgorithmResult(TerminationStatus termination, T output) : this(termination, output, 0)
    {
    }

    /// <summary>Prints execution trace into string.</summary>
    public string PrintTrace() => string.Join(Environment.NewLine, Trace);

    /// <summary>Prints execution trace into given stream object.</summary>
    public void PrintTrace(Stream stream)
    {
        var writer = new StreamWriter(stream);
        foreach (var trace in Trace)
            writer.WriteLine(trace);
        writer.Flush();
    }
    
    public override string ToString() => $"{Termination} after {Steps} step{(Steps == 1 ? "" : "s")}";
}