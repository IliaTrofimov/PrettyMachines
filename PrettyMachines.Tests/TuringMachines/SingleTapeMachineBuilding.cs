using PrettyMachines.Implementations.Data;
using PrettyMachines.Implementations.Turing;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.TuringMachines;

public class SingleTapeMachineBuilding(ITestOutputHelper output)
{
    [Fact]
    public void CreateMachine_Success()
    {
        var builder = new TuringMachineBuilder<char>();
        var q0 = builder.AddState();
        var q1 = builder.AddState("State 2");
        var q2 = builder.AddState("Terminal State 3", true);
        var q3 = builder.AddTerminalState();
        
        builder.AddInstruction(
            q1.AcceptExact('x'),
            TuringMachineAction.PrintAndMoveRight(q2, 'X')
        );
        builder.AddInstruction(
            q0.AcceptExact('y'),
            TuringMachineAction.PrintAndMoveRight(q3, 'Y')
        );
        
        var machine = builder.Build();
        
        Assert.Same(machine, q0.Machine);
        Assert.Same(machine, q1.Machine);
        Assert.Same(machine, q2.Machine);
        Assert.Same(machine, q3.Machine);
        Assert.True(q0.Id >= 0, $"Expected q0.Id >= 0, but q0.Id = {q0.Id}");
        Assert.True(q1.Id > q0.Id, "q1.Id > q0.Id");
        Assert.True(q2.Id > q1.Id, "q2.Id > q1.Id");
        Assert.True(q3.Id > q2.Id, "q3.Id > q2.Id");
    }
    
    [Fact]
    public void CreateMachine_DoNotAddSymbolWhenNotPrintingOrReading()
    {
        var table = new InstructionsTable<int>();
        var q0 = new TuringMachineState(0);
        var q1 = new TuringMachineState(1);

        table.AddInstruction(q0.AcceptExact(10), TuringMachineAction.PrintAndMoveRight(q1, 20));
        table.AddInstruction(q0.AcceptExact(50), TuringMachineAction.MoveLeft<int>(q1));
        table.AddInstruction(q0.AcceptAny<int>(), TuringMachineAction.MoveLeft<int>(q1));
        table.AddInstruction(q0.AcceptEmpty<int>(), TuringMachineAction.MoveLeft<int>(q1));
        table.AddInstruction(q0.AcceptNotEmpty<int>(), TuringMachineAction.MoveLeft<int>(q1));

        Assert.Equivalent(new[]{10, 20, 50}, table.Alphabet);
    }
    
    [Fact]
    public void CreateMachine_HaltState()
    {
        var table = new InstructionsTable<char>();
        var q0 = new TuringMachineState(0);
        var q1 = new TuringMachineState(1, "halt", true);

        table.AddInstruction(q0.AcceptExact('x'), TuringMachineAction.PrintAndMoveRight(q1, 'X'));
        table.AddInstruction(q0.AcceptExact('y'), TuringMachineAction.PrintAndMoveRight(TuringMachineState.Halt, 'Y'));
        
        Assert.Equivalent(new[]{'x', 'X', 'y', 'Y'}, table.Alphabet);
        Assert.Equal(2, table.StatesCount);

        var machine = new SingleTapeTuringMachine<char>(q0, table);
        
        var tape1 = new TuringMachineTape<char>("xx");
        var steps1 = machine.Run(tape1);
        Assert.Equal(1, steps1);
        Assert.Equal("Xx", tape1.Print());
        
        machine.ResetState();
        var tape2 = new TuringMachineTape<char>("yy");
        var steps2 = machine.Run(tape2);
        Assert.Equal(1, steps2);
        Assert.Equal("Yy", tape2.Print());
    }

    [Fact]
    public void CreateMachine_NoDuplicateStatesAndSymbols()
    {
        var table = new InstructionsTable<int>();
        var q0 = new TuringMachineState(0);
        var q1 = new TuringMachineState(1);

        for (var num = 0; num <= 8; num++)
            table.AddInstruction(q0.AcceptExact(num), TuringMachineAction.Print(q1, num + 1));
        table.AddInstruction(q0.AcceptExact(9), TuringMachineAction.PrintAndMoveLeft(q1, 0));
        table.AddInstruction(q1.AcceptAny<int>(), TuringMachineAction.PrintAndHalt(1));
        
        var instructions = table.Instructions.ToArray();
        output.WriteLine(string.Join('\n', instructions));
        Assert.Equal(11, instructions.Length);
        Assert.Equal(10, table.AlphabetLength);
        Assert.Equal(2, table.StatesCount);
        Assert.Equal(11, table.InstructionsCount);
        Assert.Equivalent(Enumerable.Range(0, 10), table.Alphabet);
    }
    
    [Fact]
    public void CreateMachine_CustomComparer()
    {
        var table = new InstructionsTable<int>(new MyComparer());
        var q0 = new TuringMachineState(0);
        var q1 = new TuringMachineState(1);

        for (var num = 0; num <= 20; num++)
            table.AddInstruction(q0.AcceptExact(num), TuringMachineAction.Print(q1, num + 1));
        table.AddInstruction(q0.AcceptExact(9), TuringMachineAction.PrintAndMoveLeft(q1, 0));
        table.AddInstruction(q1.AcceptAny<int>(), TuringMachineAction.PrintAndHalt(1));
        
        var instructions = table.Instructions.ToArray();
        output.WriteLine(string.Join('\n', instructions));
        Assert.Equal(11, instructions.Length);
        Assert.Equal(10, table.AlphabetLength);
        Assert.Equivalent(Enumerable.Range(0, 10), table.Alphabet);
    }
    
    private sealed class MyComparer : IEqualityComparer<int>
    {
        public bool Equals(int x, int y) => x % 10 == y % 10; 
        public int GetHashCode(int obj) => obj % 10;
    }
    
    
    [Fact]
    public void BuildingStates_InitialStateMustNotBeTerminal()
    {
        var builder = new TuringMachineBuilder<char>();
        var q0 = builder.AddState();
        var q1 = builder.AddTerminalState();
        
        var ex = Assert.Throws<ArgumentException>(()=> 
            builder.AddInstruction(
                q1.AcceptExact('x'),
                TuringMachineAction.PrintAndMoveRight(q0, 'X')
            )
        );
        Assert.Equal("initialState", ex.ParamName);
    }
    
    [Fact]
    public void BuildingStates_CannotAddStateFromOtherMachine()
    {
        var builder1 = new TuringMachineBuilder<char>();
        var q0 = builder1.AddState();
        builder1.AddInstruction(q0.AcceptAny<char>());
        builder1.Build();
        
        var builder2 = new TuringMachineBuilder<char>();
        var q1 = builder2.AddState();
        
        var ex1 = Assert.Throws<ArgumentException>(() => 
            builder2.AddInstruction(q0.AcceptAny<char>(), TuringMachineAction.MoveLeft<char>(q1))
        );
        var ex2 = Assert.Throws<ArgumentException>(() => 
            builder2.AddInstruction(q1.AcceptAny<char>(), TuringMachineAction.MoveLeft<char>(q0))
        );
        
        output.WriteLine($"ex1: {ex1.Message}");
        output.WriteLine($"ex2: {ex2.Message}");

        Assert.Equal("condition", ex1.ParamName);
        Assert.Equal("action", ex2.ParamName);
    }
    
    [Fact]
    public void CreateMachine_CannotSetInitialStateFromOtherMachine()
    {
        var builder1 = new TuringMachineBuilder<char>().SetInitialStateId(1);
        var q1 = builder1.AddState();
        builder1.AddInstruction(q1.AcceptAny<char>());
        builder1.Build();
        
        var builder2 = new TuringMachineBuilder<char>().SetInitialStateId(2);
        var q2 = builder2.AddState();
        builder2.AddInstruction(q2.AcceptAny<char>());
        
        Assert.Throws<InvalidOperationException>(() => 
            builder2.Build(q1)
        );

        var machine = builder2.Build();
        var ex2 = Assert.Throws<ArgumentException>(() => 
            machine.CurrentState = q1
        );
        Assert.Equal("value", ex2.ParamName);
    }
    
    [Fact]
    public void CreateMachine_CannotCreateWithNoStates()
    {
        var table = new InstructionsTable<char>();
        Assert.Equal(0, table.InstructionsCount);
        Assert.Equal(0, table.StatesCount);

        var ex = Assert.Throws<ArgumentException>(() => new SingleTapeTuringMachine<char>(table));
        Assert.Equal("instructions", ex.ParamName);
        Assert.Contains("state", ex.Message);
    }
    
    [Fact]
    public void CreateMachine_CanCreateWithEmptyAlphabet()
    {
        var q0 = new TuringMachineState(0, null);
        var q1 = new TuringMachineState(1, null);
        var q2 = new TuringMachineState(2, null);

        var table = new InstructionsTable<char>();
        table.AddInstruction(q0.AcceptNotEmpty<char>(), TuringMachineAction.MoveLeft<char>(q0));
        table.AddInstruction(q0.AcceptEmpty<char>(), TuringMachineAction.MoveLeft<char>(q1));
        table.AddInstruction(q0.AcceptAny<char>(), TuringMachineAction.MoveLeft<char>(q2));
        
        Assert.Equal(3, table.InstructionsCount);
        Assert.Equal(3, table.StatesCount);
        Assert.Equal(0, table.AlphabetLength);

        var machine = new SingleTapeTuringMachine<char>(q0, table);
    }
    
    [Fact]
    public void StrictAlphabetEnabled_FailWithUnknownSymbol()
    {
        var builder = new TuringMachineBuilder<char>();
        var q0 = builder.AddState();
        
        builder.AddInstruction(q0).AcceptExact('.').PrintAndMoveRight('x');
        builder.AddInstruction(q0).AcceptExact('x').MoveRight();
        builder.AddInstruction(q0).AcceptEmpty().Halt();
        
        var machine = builder.Build();
        machine.StrictAlphabet = true;
        
        var ex1 = Assert.Throws<InvalidOperationException>(() => builder.Instructions.FindInstruction(q0, false, 'y'));
        output.WriteLine($"ex1: {ex1}");
        Assert.Contains("'y' wasn't present in the alphabet", ex1.Message);
        
        var ex2 = Assert.Throws<InvalidOperationException>(() => machine.Run(new TuringMachineTape<char>("..xxy")));
        Assert.Contains("'y' wasn't present in the alphabet", ex2.Message);
        output.WriteLine($"ex2: {ex2}");
    }
    
    [Fact]
    public void StrictAlphabetDisabled_SoftStopWithUnknownSymbol()
    {
        var builder = new TuringMachineBuilder<char>();
        var q0 = builder.AddState();
        
        builder.AddInstruction(q0).AcceptExact('.').PrintAndMoveRight('x');
        builder.AddInstruction(q0).AcceptExact('x').MoveRight();
        builder.AddInstruction(q0).AcceptEmpty().Halt();
        
        var machine = builder.Build();
        machine.StrictAlphabet = false;
        
        var action = builder.Instructions.FindInstruction(q0, false, 'y');
        Assert.Null(action);
        
        var steps = machine.Run(new TuringMachineTape<char>("..xxy"));
        Assert.Equal(5, steps);
    }
    
    [Fact]
    public void Example_BinaryIncrement()
    {
        const char emptySymbol = '_';
        
        var builder = new TuringMachineBuilder<char>(emptySymbol);
        var q0 = builder.AddState();
        var q1 = builder.AddState();
        var q2 = builder.AddState();
        builder
            .AddInstruction(q0.AcceptExact('0'), TuringMachineAction.MoveRight<char>(q0))
            .AddInstruction(q0.AcceptExact('1'), TuringMachineAction.MoveRight<char>(q0))
            .AddInstruction(q0.AcceptExact('_'), TuringMachineAction.MoveLeft<char>(q1))
            .AddInstruction(q1.AcceptExact('0'), TuringMachineAction.Print(q2, '1'))
            .AddInstruction(q1.AcceptExact('1'), TuringMachineAction.PrintAndMoveLeft(q1, '0'))
            .AddInstruction(q1.AcceptExact('_'), TuringMachineAction.PrintAndHalt('1'))
            .AddInstruction(q2.AcceptExact('0'), TuringMachineAction.MoveLeft<char>(q2))
            .AddInstruction(q2.AcceptExact('1'), TuringMachineAction.MoveLeft<char>(q2))
            .AddInstruction(q2.AcceptExact('_'), TuringMachineAction.Halt<char>());
        
        var machine = builder.Build(q0);
        var tape = new TuringMachineTape<char>("1011", emptySymbol);

        var steps = 1;
        output.WriteLine($"{steps:d3} | INITIAL {machine.CurrentState}");

        while (machine.NextStep(tape, out var action))
        {
            steps++;
            output.WriteLine($"{steps:d3} | {action} -> {machine.CurrentState}");
        }
        
        output.WriteLine($"{steps:d3} | FINAL {machine.CurrentState}");


        var result = tape.Print();
        Assert.Equal("1100", result);
        Assert.Equal(11, steps);
    }
    
    [Fact]
    public void Example_Example_BinaryIncrementShortSyntax()
    {
        const char emptySymbol = '_';
        
        var builder = new TuringMachineBuilder<char>(emptySymbol);
        var q0 = builder.AddState();
        var q1 = builder.AddState();
        var q2 = builder.AddState();
        builder
            .AddInstruction(q0.AcceptNotEmpty<char>(), TuringMachineAction.MoveRight<char>(q0))
            .AddInstruction(q0.AcceptEmpty<char>(),    TuringMachineAction.MoveLeft<char>(q1))
            .AddInstruction(q1.AcceptExact('0'),       TuringMachineAction.Print(q2, '1'))
            .AddInstruction(q1.AcceptExact('1'),       TuringMachineAction.PrintAndMoveLeft(q1, '0'))
            .AddInstruction(q1.AcceptExact('_'),       TuringMachineAction.PrintAndHalt('1'))
            .AddInstruction(q2.AcceptNotEmpty<char>(), TuringMachineAction.MoveLeft<char>(q2))
            .AddInstruction(q2.AcceptExact('_'));
        
        var machine = builder.Build(q0);
        var tape = new TuringMachineTape<char>("1011", emptySymbol);
        var steps = machine.Run(tape);
        var result = tape.Print();
        Assert.Equal("1100", result);
        Assert.Equal(11, steps);
    }
    
    [Fact]
    public void Example_Example_BinaryIncrementChainSyntax()
    {
        const char emptySymbol = '_';
        
        var builder = new TuringMachineBuilder<char>(emptySymbol);
        var q0 = builder.AddState();
        var q1 = builder.AddState();
        var q2 = builder.AddState();

        builder.AddInstruction(q0).AcceptNotEmpty().MoveRight(q0);
        builder.AddInstruction(q0).AcceptEmpty().MoveLeft(q1);
        builder.AddInstruction(q1).AcceptExact('0').Print('1', q2);
        builder.AddInstruction(q1).AcceptExact('1').PrintAndMoveLeft('0', q1);
        builder.AddInstruction(q1).AcceptEmpty().PrintAndHalt('1');
        builder.AddInstruction(q2).AcceptNotEmpty().MoveLeft(q2);
        builder.AddInstruction(q2).AcceptEmpty().Halt();

        var machine = builder.Build(q0);
        var tape = new TuringMachineTape<char>("1011", emptySymbol);
        var steps = machine.Run(tape);
        var result = tape.Print();
        Assert.Equal("1100", result);
        Assert.Equal(11, steps);
    }
}