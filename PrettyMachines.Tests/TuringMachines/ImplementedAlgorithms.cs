using PrettyMachines.Implementations.Data;
using PrettyMachines.Implementations.Serialization;
using PrettyMachines.Implementations.Turing;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.TuringMachines;

public class ImplementedAlgorithms(ITestOutputHelper output)
{
    [Fact]
    public void BinIncrement_Empty()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryIncrementMachine('_');
        Test(machine, "_", "1");
    }
    
    [Fact]
    public void BinIncrement_Zero()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryIncrementMachine('_');
        Test(machine, "0", "1", 5);
    }
    
    [Fact]
    public void BinIncrement_LargeZero()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryIncrementMachine('_');
        Test(machine, "000000000000000000000", "000000000000000000001");
    }
    
    [Fact]
    public void BinIncrement_One()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryIncrementMachine('_');
        output.WriteLine(TuringMachineSerializer.ToListView(machine.Instructions));
        Test(machine, "1", "10");
    }
    
    [Fact]
    public void BinIncrement_OneWithLeadingZeros()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryIncrementMachine('_');
        Test(machine, "00001", "00010");
    }
    
    [Fact]
    public void BinIncrement_Ones()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryIncrementMachine('_');
        Test(machine, "1111111111111111111", "10000000000000000000");
    }
    
    
    [Fact]
    public void BinDecrement_Empty()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryDecrementMachine('_');
        Test(machine, "_", "0");
    }
    
    [Fact]
    public void BinDecrement_Zero()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryDecrementMachine('_');
        Test(machine, "0", "0");
    }
    
    [Fact]
    public void BinDecrement_LargeZero()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryDecrementMachine('_');
        Test(machine, "000000000000000000000", "000000000000000000000");
    }
    
    [Fact]
    public void BinDecrement_One()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryDecrementMachine('_');
        Test(machine, "1", "0");
    }
    
    [Fact]
    public void BinDecrement_OneWithLeadingZeros()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryDecrementMachine('_');
        Test(machine, "00001", "00000");
    }
    
    [Fact]
    public void BinDecrement_Ones()
    {
        var machine = SingleTapeTuringMachines.Create_BinaryDecrementMachine('_');
        Test(machine, "1111111111111111111", "11111111111111111110");
    }

    
    private void Test(SingleTapeTuringMachine<char> machine, string input, string expectedOutput, int? expectedSteps = null)
    {
        output.WriteLine("Input: '{0}'", input);
        
        const string format = "{0,4}| {1,5} | {2,7} | {3,5} | {4,7} | {5}";
        output.WriteLine(format, "step", "state", "scan", "next", "print", "move");

        var tape = new TuringMachineTape<char>(input, '_');
        var actualSteps = machine.Run(tape, (step, symbol, state, action) =>
        {
            if (action != null)
            {
                output.WriteLine(
                    format, 
                    step,
                    state.ToString(true), 
                    symbol.IsEmpty ? "<empty>" : symbol.Symbol.ToString(),
                    action.Value.NextState.ToString(true),
                    !action.Value.ShouldPrintSymbol ? "" : action.Value.PrintedSymbol,
                    action.Value.Movement
                );
            }
            else
            {
                output.WriteLine(
                    format, 
                    step,
                    state.ToString(true), 
                    symbol.IsEmpty ? "<empty>" : symbol.Symbol.ToString(),
                    "n/a",
                    "n/a",
                    "n/a"
                );
            }
            
        });

        var actualOutput = tape.Print();
        output.WriteLine("Output:       '{0}' index at {1}", actualOutput, tape.CurrentIndex);
        output.WriteLine("Output (raw): '{0}' index at {1}", tape.Print(trimEmptyCells: false), tape.CurrentIndex);

        Assert.Equal(expectedOutput, actualOutput);
        if (expectedSteps.HasValue) Assert.Equal(expectedSteps.Value, actualSteps);
    }
}