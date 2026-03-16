using PrettyMachines.Implementations.Turing;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.Turing;

public class MachineTests(ITestOutputHelper output)
{
    [Fact]
    public void CreateMachine_Default()
    {
        var builder = new TuringMachineBuilder<char>(['x', 'y', 'z', 'X', 'Y', 'Z']);
        var q0 = builder.AddState();
        var q1 = builder.AddState("State 2");
        var q2 = builder.AddState("Terminal State 3", true);
        var q3 = builder.AddTerminalState();
        
        builder.AddInstruction(
            TuringMachine.AcceptsExact(q1, 'x'),
            TuringMachine.PrintsAndMovesRight(q2, 'X')
        );
        builder.AddInstruction(
            TuringMachine.AcceptsExact(q0, 'y'),
            TuringMachine.PrintsAndMovesRight(q3, 'Y')
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
                TuringMachine.AcceptsExact(q1, 'x'),
                TuringMachine.PrintsAndMovesRight(q0, 'X')
            )
        );
        Assert.Equal("initialState", ex.ParamName);
    }
    
    [Fact]
    public void CannotAddStatesFromOtherMachine()
    {
        var builder1 = new TuringMachineBuilder<char>();
        var q1 = builder1.AddState();
        builder1.AddTerminatingInstruction(TuringMachine.AcceptsAny<char>(q1));
        builder1.Build();
        
        var builder2 = new TuringMachineBuilder<char>();
        var q2 = builder2.AddState();
        
        var ex1 = Assert.Throws<ArgumentException>(() => 
            builder2.AddInstruction(TuringMachine.AcceptsAny<char>(q1), TuringMachine.MovesLeft<char>(q2))
        );
        var ex2 = Assert.Throws<ArgumentException>(() => 
            builder2.AddInstruction(TuringMachine.AcceptsAny<char>(q2), TuringMachine.MovesLeft<char>(q1))
        );
        
        Assert.Equal("action", ex1.ParamName);
        Assert.Equal("condition", ex2.ParamName);
    }
    
    [Fact]
    public void CannotSetStatesFromOtherMachine()
    {
        var builder1 = new TuringMachineBuilder<char>();
        var q1 = builder1.AddState();
        builder1.AddTerminatingInstruction(TuringMachine.AcceptsAny<char>(q1));
        builder1.Build();
        
        var builder2 = new TuringMachineBuilder<char>();
        var q2 = builder2.AddState();
        builder1.AddTerminatingInstruction(TuringMachine.AcceptsAny<char>(q2));
        
        var ex1 = Assert.Throws<ArgumentException>(() => 
            builder2.Build(q1)
        );
        Assert.Equal("initialState", ex1.ParamName);

        var machine = builder2.Build(q2);
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
        
        var builder = new TuringMachineBuilder<char>(['1', '0'], emptySymbol);
        var q0 = builder.AddState();
        var q1 = builder.AddState();
        var q2 = builder.AddState();
        builder
            .AddInstruction(TuringMachine.AcceptsExact(q0, '0'), TuringMachine.MovesRight<char>(q0))
            .AddInstruction(TuringMachine.AcceptsExact(q0, '1'), TuringMachine.MovesRight<char>(q0))
            .AddInstruction(TuringMachine.AcceptsExact(q0, '_'), TuringMachine.MovesLeft<char>(q1))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '0'), TuringMachine.Prints(q2, '1'))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '1'), TuringMachine.PrintsAndMovesLeft(q1, '0'))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '_'), TuringMachine.PrintsAndStops('1'))
            .AddInstruction(TuringMachine.AcceptsExact(q2, '0'), TuringMachine.MovesLeft<char>(q2))
            .AddInstruction(TuringMachine.AcceptsExact(q2, '1'), TuringMachine.MovesLeft<char>(q2))
            .AddInstruction(TuringMachine.AcceptsExact(q2, '_'), TuringMachine.Stops<char>());
        
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
            .AddInstruction(TuringMachine.AcceptsNotEmpty<char>(q0), TuringMachine.MovesRight<char>(q0))
            .AddInstruction(TuringMachine.AcceptsEmpty<char>(q0),    TuringMachine.MovesLeft<char>(q1))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '0'),     TuringMachine.Prints(q2, '1'))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '1'),     TuringMachine.PrintsAndMovesLeft(q1, '0'))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '_'),     TuringMachine.PrintsAndStops('1'))
            .AddInstruction(TuringMachine.AcceptsNotEmpty<char>(q2), TuringMachine.MovesLeft<char>(q2))
            .AddTerminatingInstruction(TuringMachine.AcceptsExact(q2, '_'));
        
        var machine = builder.Build(q0);
        var tape = new TuringMachineTape<char>("1011", emptySymbol);
        output.WriteLine($"INITIAL '{tape.JoinAsString(trimEmptyCells: false)}'");

        var steps = 0;
        foreach (var step in machine.RunVerbose(tape))
        {
            output.WriteLine($"{step.state} scan '{step.cell}' -> {step.action}");
            steps++;
        }
        output.WriteLine($"RESULT  '{tape.JoinAsString(trimEmptyCells: false)}'");

        var result = tape.JoinAsString();
        Assert.Equal("1100", result);
        Assert.Equal(11, steps);
    }
    
}