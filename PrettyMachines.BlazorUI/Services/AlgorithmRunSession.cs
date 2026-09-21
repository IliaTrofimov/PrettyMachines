using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Turing;


namespace PrettyMachines.BlazorUI.Services;

/// <summary>Stateful, step-by-step execution session over the lazy <see cref="IAlgorithm.Run"/> sequence.</summary>
public sealed class AlgorithmRunSession : IDisposable
{
    private readonly List<string> trace = [];
    private IAlgorithm? algorithm;
    private IEnumerator<IAlgorithmSnapshot>? enumerator;
    private CancellationTokenSource? cancellation;


    /// <summary>Gets the accumulated trace lines.</summary>
    public IReadOnlyList<string> Trace => trace;

    /// <summary>Gets the current output rendered as text.</summary>
    public string Output { get; private set; } = "";

    /// <summary>Gets the current tape when the algorithm is a Turing machine, or <c>null</c> otherwise.</summary>
    public IReadOnlyTape? Tape { get; private set; }

    /// <summary>Gets the current termination status.</summary>
    public TerminationStatus Status { get; private set; } = TerminationStatus.Unknown;

    /// <summary>Gets the number of executed steps.</summary>
    public long Steps { get; private set; }

    /// <summary>Gets a value indicating whether a session has been started.</summary>
    public bool IsStarted => algorithm is not null;

    /// <summary>Gets a value indicating whether execution has finished or has not been started.</summary>
    public bool IsFinished { get; private set; } = true;

    /// <summary>Gets a value indicating whether execution is paused and can be advanced.</summary>
    public bool IsActive => enumerator is not null && !IsFinished;

    /// <summary>Gets a value indicating whether the current input belongs to the algorithm's class.</summary>
    public bool IsValidInput { get; private set; } = true;

    /// <summary>Gets an error message produced while starting or advancing the session, or <c>null</c>.</summary>
    public string? Error { get; private set; }


    /// <summary>Starts a new session for the given algorithm and input.</summary>
    /// <param name="algorithm">Algorithm to execute.</param>
    /// <param name="input">Initial input.</param>
    /// <param name="maxSteps">Maximum number of steps. Values below one are clamped to one.</param>
    public void Start(IAlgorithm algorithm, string input, uint maxSteps)
    {
        ArgumentNullException.ThrowIfNull(algorithm);

        Reset();
        this.algorithm = algorithm;
        Input = input ?? "";
        MaxSteps = Math.Max(1u, maxSteps);
        IsValidInput = algorithm.ValidateInput(Input);
        Output = Input;
        IsFinished = false;
        Error = null;

        cancellation = new CancellationTokenSource();
        var bounds = new AlgorithmCancellation(MaxSteps, cancellation.Token);

        try
        {
            enumerator = algorithm.Run(Input, bounds, verbose: true).GetEnumerator();
            Advance();
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    /// <summary>Gets the input used by the current session.</summary>
    public string Input { get; private set; } = "";

    /// <summary>Gets the step limit used by the current session.</summary>
    public uint MaxSteps { get; private set; } = 1000;

    /// <summary>Advances the session by a single step.</summary>
    public void Step()
    {
        if (enumerator is null || IsFinished)
            return;

        try
        {
            Advance();
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    /// <summary>Advances the session until it finishes.</summary>
    public void Run()
    {
        while (enumerator is not null && !IsFinished)
            Step();
    }

    /// <summary>Requests cancellation and stops the session.</summary>
    public void Stop()
    {
        cancellation?.Cancel();
        IsFinished = true;
        enumerator?.Dispose();
        enumerator = null;
        if (Status == TerminationStatus.Unknown && algorithm is not null)
            Status = TerminationStatus.Aborted;
    }

    /// <summary>Stops the session and clears all state.</summary>
    public void Reset()
    {
        cancellation?.Cancel();
        cancellation?.Dispose();
        enumerator?.Dispose();
        cancellation = null;
        enumerator = null;
        algorithm = null;
        trace.Clear();
        Output = "";
        Input = "";
        Steps = 0;
        Status = TerminationStatus.Unknown;
        IsFinished = true;
        Error = null;
        IsValidInput = true;
        Tape = null;
    }

    /// <inheritdoc/>
    public void Dispose() => Reset();


    private void Advance()
    {
        if (enumerator is null)
            return;

        if (!enumerator.MoveNext())
        {
            IsFinished = true;
            if (Status == TerminationStatus.Unknown)
                Status = TerminationStatus.Aborted;
            return;
        }

        Apply(enumerator.Current);
    }

    private void Apply(IAlgorithmSnapshot snapshot)
    {
        if (snapshot.TraceLine is not null)
            trace.Add(snapshot.TraceLine);

        Output = snapshot.Output;
        Tape = (snapshot as IAlgorithmSnapshot<IReadOnlyTape>)?.Output;
        Steps = snapshot.Steps;
        Status = snapshot.Termination;

        if (snapshot.IsFinished)
            IsFinished = true;
    }

    private void Fail(Exception exception)
    {
        Error = exception.Message;
        Status = TerminationStatus.Aborted;
        IsFinished = true;
    }
}
