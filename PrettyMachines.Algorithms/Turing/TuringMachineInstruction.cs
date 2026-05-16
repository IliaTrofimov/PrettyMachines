namespace PrettyMachines.Algorithms.Turing;

public readonly struct TuringMachineInstruction<TSymbol>
{
    public required TuringMachineState InitialState { get; init; }
    public required FuzzyKey<TSymbol> ScannedSymbol { get; init; }
    public required TuringMachineState NextState { get; init; }
    public required TSymbol? PrintedSymbol { get; init; }
    public required TapeMovement Movement { get; init; }
}