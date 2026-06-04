namespace PrettyMachines.Algorithms.Turing;

/// <summary>
/// Represents single instruction from Turing machine as 5-tuple of
/// initial state, scanned symbol, next state, printed symbol and tape movement.
/// </summary>
public readonly struct TuringMachineInstruction
{
    public required TuringMachineState InitialState { get; init; }
    public required FuzzyKey<string> ScannedSymbol { get; init; }
    public required TuringMachineState NextState { get; init; }
    public required string? PrintedSymbol { get; init; }
    public required TapeMovement Movement { get; init; }
}