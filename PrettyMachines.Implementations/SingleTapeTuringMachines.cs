using PrettyMachines.Algorithms.Turing;


namespace PrettyMachines.Implementations;

/// <summary>
/// Predefined Turing machines.
/// </summary>
public static class SingleTapeTuringMachines
{
    /// <summary>
    /// Creates Turing machine that increments single non-negative binary number.
    /// </summary>
    /// <example>
    /// Input:  <c>10111_</c> (cursor at index 0);
    /// Output: <c>11000_</c> (cursor at index 0).
    /// </example>
    public static TuringMachine Create_BinaryIncrementMachine(string emptySymbol = "_")
    {
        return TuringMachine.Create("Binary increment")
            .WithAlphabet(["0", "1"], emptySymbol)
            .WithBlankSymbol(emptySymbol)
            .WithStringComparer(StringComparison.CurrentCultureIgnoreCase)
            .AddInitialState("Find number ending", out var q0)
            .AddState("Increment one bit", out var q1)
            .AddState("Find number start", out var q2)
            .BuildRules(builder => builder
                .AddRule(q0, SymbolMatch.NotEmpty, q0, null, TapeMovement.Right)
                .AddRule(q0, SymbolMatch.Empty,    q1, null, TapeMovement.Left)
                .AddRule(q1, "0",                  q2, "1")
                .AddRule(q1, "1",                  q0, "0",  TapeMovement.Left)
                .AddRule(q2, SymbolMatch.NotEmpty, q0, null, TapeMovement.Right)
                .AddHalt(q1, SymbolMatch.Empty, "1", TapeMovement.Right)
                .AddHalt(q2, SymbolMatch.Empty)
            );
    }
}