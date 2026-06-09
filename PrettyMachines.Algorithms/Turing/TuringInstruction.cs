namespace PrettyMachines.Algorithms.Turing;

/// <summary>
/// Represents single instruction from Turing machine as 5-tuple of
/// initial state, scanned symbol, next state, printed symbol and tape movement.
/// </summary>
public readonly struct TuringInstruction
{
    /// <summary>
    /// 
    /// </summary>
    public required TuringMachineState InitialState { get; init; }
    public required FuzzyKey<string> ScannedSymbol { get; init; }
    public required TuringMachineState NextState { get; init; }
    public required string? PrintedSymbol { get; init; }
    public required TapeMovement Movement { get; init; }

    /// <summary>
    /// Deconstruct into tuple <c>(qi, Sj, qm, Sk/E/N, L/R/N)</c>. 
    /// </summary>
    /// <remarks>
    /// This convention differs from original Turing's definition and was adopted by
    /// Minsky (1967), Stone (1972), Hopcroft and Ullman (1979).
    /// </remarks>
    public void Deconstruct(out TuringMachineState initialState,
                            out FuzzyKey<string> scannedSymbol,
                            out TuringMachineState nextState,
                            out string? printedSymbol,
                            out TapeMovement movement)
    {
        initialState = InitialState;
        scannedSymbol = ScannedSymbol;
        nextState = NextState;
        printedSymbol = PrintedSymbol;
        movement = Movement;
    }
}