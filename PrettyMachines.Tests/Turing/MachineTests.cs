using PrettyMachines.Implementations.Turing;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.Turing;

public class MachineTests(ITestOutputHelper output)
{
    [Fact]
    public void CreateMachine_Default()
    {
        var machine = new TuringMachine<char>(['x', 'y', 'z', 'X', 'Y', 'Z']);
        var q1 = machine.AddState();
        var q2 = machine.AddState("State 2");
        var q3 = machine.AddState("Terminal State 3", true);
        var q4 = machine.AddTerminalState();
        
        machine.AddInstruction(
            TuringMachine.AcceptsExact(q1, 'x'),
            TuringMachine.PrintsAndMovesRight(q2, 'X')
        );
        machine.AddInstruction(
            TuringMachine.AcceptsExact(q2, 'y'),
            TuringMachine.PrintsAndMovesRight(q3, 'Y')
        );
        
        Assert.Same(machine, q1.Machine);
        Assert.Same(machine, q2.Machine);
        Assert.Same(machine, q3.Machine);
        Assert.Same(machine, q4.Machine);
        Assert.True(q1.Id > 0, $"Expected q1.Id > 0, but q1.Id = {q1.Id}");
        Assert.True(q2.Id > q1.Id, "q2.Id > q1.Id");
        Assert.True(q3.Id > q2.Id, "q3.Id > q2.Id");
        Assert.True(q4.Id > q3.Id, "q4.Id > q3.Id");
    }
    
    [Fact]
    public void CannotAddStatesFromOtherMachine()
    {
        var machine1 = new TuringMachine<char>(['x', 'y', 'z', 'X', 'Y', 'Z']);
        var q1 = machine1.AddState();
        
        var machine2 = new TuringMachine<char>(['x', 'y', 'z', 'X', 'Y', 'Z']);
        var q2 = machine2.AddState();

        var ex1 = Assert.Throws<ArgumentException>(() => 
            machine1.AddInstruction(TuringMachine.AcceptsAny<char>(q1), TuringMachine.Prints(q2, 'x'))
        );
        var ex2 = Assert.Throws<ArgumentException>(() => 
            machine2.AddInstruction(TuringMachine.AcceptsAny<char>(q1), TuringMachine.Prints(q2, 'x'))
        );
        
        Assert.Equal("action", ex1.ParamName);
        Assert.Equal("condition", ex2.ParamName);
    }
    
    [Fact]
    public void CreateMachine_FailWithEmptyAlphabet()
    {
        Assert.Throws<ArgumentException>(() => new TuringMachine<char>([]));
    }

    [Fact]
    public void Example_Concatenation()
    {
        const char emptySymbol = '_';
        
        var machine = new TuringMachine<char>(['1', '0'], null, emptySymbol);
        var q0 = machine.AddState();
        var q1 = machine.AddState();
        var q2 = machine.AddState();
        machine
            .AddInstruction(TuringMachine.AcceptsExact(q0, '0'), TuringMachine.MovesRight<char>(q0))
            .AddInstruction(TuringMachine.AcceptsExact(q0, '1'), TuringMachine.MovesRight<char>(q0))
            .AddInstruction(TuringMachine.AcceptsExact(q0, '_'), TuringMachine.MovesLeft<char>(q1))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '0'), TuringMachine.Prints(q2, '1'))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '1'), TuringMachine.PrintsAndMovesLeft(q1, '0'))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '_'), TuringMachine.PrintsAndStops('1'))
            .AddInstruction(TuringMachine.AcceptsExact(q2, '0'), TuringMachine.MovesLeft<char>(q2))
            .AddInstruction(TuringMachine.AcceptsExact(q2, '1'), TuringMachine.MovesLeft<char>(q2))
            .AddInstruction(TuringMachine.AcceptsExact(q2, '_'), TuringMachine.Stops<char>());
        
        machine.CurrentState = q0;
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
    public void Example_ConcatenationShortSyntax()
    {
        const char emptySymbol = '_';
        
        var machine = new TuringMachine<char>(['1', '0'], null, emptySymbol);
        var q0 = machine.AddState();
        var q1 = machine.AddState();
        var q2 = machine.AddState();
        machine
            .AddInstruction(TuringMachine.AcceptsNotEmpty<char>(q0), TuringMachine.MovesRight<char>(q0))
            .AddInstruction(TuringMachine.AcceptsEmpty<char>(q0),    TuringMachine.MovesLeft<char>(q1))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '0'),     TuringMachine.Prints(q2, '1'))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '1'),     TuringMachine.PrintsAndMovesLeft(q1, '0'))
            .AddInstruction(TuringMachine.AcceptsExact(q1, '_'),     TuringMachine.PrintsAndStops('1'))
            .AddInstruction(TuringMachine.AcceptsNotEmpty<char>(q2), TuringMachine.MovesLeft<char>(q2))
            .AddTerminatingInstruction(TuringMachine.AcceptsExact(q2, '_'));
        
        machine.CurrentState = q0;
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
    
}