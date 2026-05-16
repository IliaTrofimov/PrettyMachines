namespace PrettyMachines.Algorithms.Abstract;

/// <summary>Algorithm termination reasons.</summary>
public enum TerminationStatus
{
    /// <summary>Algorithm is not running.</summary>
    Unknown,
    /// <summary>Algorithm has reached one of its final state.</summary>
    Success,
    /// <summary>Algorithm has stopped because none of its instructions were applicable.</summary>
    Stuck,
    /// <summary>Algorithm has stuck in an infinite loop or was canceled from outside.</summary>
    Aborted,
    /// <summary>Algorithm has reached invalid or unknown input.</summary>
    InvalidInput,
}