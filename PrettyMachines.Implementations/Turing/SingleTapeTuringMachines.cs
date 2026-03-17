namespace PrettyMachines.Implementations.Turing;

public static class SingleTapeTuringMachines
{
    /// <summary>
    /// Creates Turing machine that accepts single binary number and outputs incremented number.
    /// </summary>
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
    /// Creates Turing machine that swaps symbolA and symbolB in the tape.
    /// </summary>
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