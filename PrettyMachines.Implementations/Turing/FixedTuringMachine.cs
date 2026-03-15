namespace PrettyMachines.Implementations.Turing;

public abstract class FixedTuringMachine<TSymbol> : TuringMachine
    where TSymbol : IEquatable<TSymbol>
{
    /// <summary>Execute one step.</summary>
    /// <returns><c>True</c>, if execution can be continued.</returns>
    /// <exception cref="InvalidOperationException">Turing machine wasn't properly initialized.</exception>
    public bool NextStep(IMachineTape<TSymbol> tape) => NextStep(tape, out _);

    /// <summary>Execute one step.</summary>
    /// <returns><c>True</c>, if execution can be continued.</returns>
    /// <exception cref="InvalidOperationException">Turing machine wasn't properly initialized.</exception>
    public abstract bool NextStep(IMachineTape<TSymbol> tape, out TuringMachineAction<TSymbol>? appliedAction);
}