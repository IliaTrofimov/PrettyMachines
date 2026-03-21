namespace PrettyMachines.Implementations.Turing;

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
    public static SingleTapeTuringMachine<char> Create_BinaryIncrementMachine(char emptySymbol = '_')
    {
        var builder = new TuringMachineBuilder<char>(emptySymbol);
        var q0 = builder.AddState("Find number ending");
        var q1 = builder.AddState("Increment one bit recursively");
        var q2 = builder.AddState("Find number start and stop");

        builder.AddInstruction(q0).AcceptNotEmpty().MoveRight(q0);
        builder.AddInstruction(q0).AcceptEmpty().MoveLeft(q1);
        builder.AddInstruction(q1).AcceptExact('0').Print('1', q2);
        builder.AddInstruction(q1).AcceptExact('1').PrintAndMoveLeft('0', q1);
        builder.AddInstruction(q1).AcceptEmpty().PrintAndHalt('1');
        builder.AddInstruction(q2).AcceptNotEmpty().MoveLeft(q2);
        builder.AddInstruction(q2).AcceptEmpty().Halt();

        return builder.Build(q0);
    }

    /// <summary>
    /// Creates Turing machine that decrements single non-negative binary number.
    /// </summary>
    /// <example>
    /// Input:  <c>100_</c> (cursor at index 0);
    /// Output: <c>011_</c> (cursor at index 0).
    /// </example>
    public static SingleTapeTuringMachine<char> Create_BinaryDecrementMachine(char emptySymbol = '_')
    {
        var builder = new TuringMachineBuilder<char>(emptySymbol);
        var q0 = builder.AddState("Find number ending");
        var q1 = builder.AddState("Decrement one bit recursively");
        var q2 = builder.AddState("Find number start and stop");

        builder.AddInstruction(q0).AcceptEmpty().PrintAndHalt('0');
        builder.AddInstruction(q0).AcceptExact('0').PrintAndHalt('0');
        builder.AddInstruction(q0).AcceptExact('1').PrintAndMoveLeft('0', q1);
        builder.AddInstruction(q1).AcceptExact('0').Print('1', q2);
        builder.AddInstruction(q1).AcceptExact('1').PrintAndMoveLeft('0', q1);
        builder.AddInstruction(q1).AcceptEmpty().PrintAndHalt('1');
        builder.AddInstruction(q2).AcceptNotEmpty().MoveLeft(q2);
        builder.AddInstruction(q2).AcceptEmpty().Halt();

        return builder.Build(q0);
    }
    
    /// <summary>
    /// Creates Turing machine that swaps symbol <i>a</i> and symbol <i>b</i> in the tape.
    /// </summary>
    /// <example>
    /// Input:  <c>abaab_abbb</c> (cursor at index 0);
    /// Output: <c>babba_baaa</c> (cursor at index 0).
    /// </example>
    public static SingleTapeTuringMachine<TSymbol> Create_InvertedSymbolsMachine<TSymbol>(TSymbol symbolA, TSymbol symbolB, TSymbol? emptySymbol = default)
        where TSymbol : IEquatable<TSymbol>
    {
        var builder = new TuringMachineBuilder<TSymbol>(emptySymbol);
        var q0 = builder.AddState("Invert symbols");
        var q1 = builder.AddState("Find start and stop");

        builder.AddInstruction(q0).AcceptExact(symbolA).PrintAndMoveRight(symbolB);
        builder.AddInstruction(q0).AcceptExact(symbolB).PrintAndMoveRight(symbolA);
        builder.AddInstruction(q0).AcceptEmpty().Switch(q1);
        builder.AddInstruction(q1).AcceptNotEmpty().MoveLeft();
        builder.AddInstruction(q1).AcceptEmpty().MoveRightAndHalt();

        return builder.Build(q0);
    }
}