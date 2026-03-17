using PrettyMachines.Implementations.Data;
using PrettyMachines.Implementations.Turing;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.Turing;

public class MachineTests(ITestOutputHelper output)
{
    [Fact]
    public void CreateMachine_Default()
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
    public void InitialStateMustBeNonTerminal()
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
    public void CannotAddStatesFromOtherMachine()
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
    public void CannotSetStatesFromOtherMachine()
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
    public void CreateMachine_FailWithEmptyAlphabet()
    {
        Assert.Throws<ArgumentException>(() => new TuringMachineBuilder<char>().Build());
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


        var result = tape.JoinAsString();
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
        output.WriteLine($"INITIAL '{tape.JoinAsString(trimEmptyCells: false)}'");

        var steps = machine.Run(tape, (i, cell, state, action) =>
        {
            output.WriteLine($"{i:d3} | >> {state}: '{cell.Symbol}' -> {action}");
        });
        output.WriteLine($"RESULT  '{tape.JoinAsString(trimEmptyCells: false)}'");

        var result = tape.JoinAsString();
        Assert.Equal("1100", result);
        Assert.Equal(11, steps);
    }
    
    [Fact]
    public void Example_Example_BinaryIncrementShortSyntax2()
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
        output.WriteLine($"INITIAL '{tape.JoinAsString(trimEmptyCells: false)}'");

        var steps = machine.Run(tape, (i, cell, state, action) =>
        {
            output.WriteLine($"{i:d3} | >> {state}: '{cell.Symbol}' -> {action}");
        });
        output.WriteLine($"RESULT  '{tape.JoinAsString(trimEmptyCells: false)}'");

        var result = tape.JoinAsString();
        Assert.Equal("1100", result);
        Assert.Equal(11, steps);
    }

    [Fact]
    public void StrictAlphabet_FailWithUnknownSymbol()
    {
        var builder = new TuringMachineBuilder<char>();
        var q0 = builder.AddState();
        
        builder.AddInstruction(q0).AcceptExact('0').PrintAndMoveRight('1');
        builder.AddInstruction(q0).AcceptExact('1').MoveRight();
        builder.AddInstruction(q0).AcceptEmpty().Halt();
        
        var machine = builder.Build();
        
        var count1 = machine.Run(new TuringMachineTape<char>("10012"));
        Assert.Equal(5, count1);
        
        machine.ResetState();
        machine.StrictAlphabet = true;
        Assert.Throws<InvalidOperationException>(() => machine.Run(new TuringMachineTape<char>("10012")));
    }
}