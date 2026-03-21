using PrettyMachines.Implementations.Turing;


namespace PrettyMachines.Tests.TuringMachines;

public class StatesActionsCondition
{
    [Fact]
    public void State_IdMustNotBeReserved()
    {
        Assert.Throws<ArgumentException>(() => new TuringMachineState(int.MinValue));
    }
    
    [Fact]
    public void State_Equality()
    {
        Assert.NotEqual(new TuringMachineState(0).GetHashCode(), new TuringMachineState(1).GetHashCode());
        Assert.Equal(new TuringMachineState(1, "xxx").GetHashCode(), new TuringMachineState(1, "yyy", true).GetHashCode());

        var q0 = new TuringMachineState(100);
        var q1 = new TuringMachineState(100, "xx", true);
        var q2 = new TuringMachineState(100);
        var q3 = new TuringMachineState(100);

        var machine1 = SingleTapeTuringMachines.Create_BinaryDecrementMachine();
        var machine2 = SingleTapeTuringMachines.Create_BinaryDecrementMachine();

        q0.Attach(machine1);
        q1.Attach(machine1);
        q2.Attach(machine2);
        
        Assert.Equal(q0, q1);
        Assert.NotEqual(q0, q2);
        Assert.NotEqual(q1, q2);
        Assert.NotEqual(q0, q3);
        Assert.NotEqual(q1, q3);
        Assert.NotEqual(q2, q3);
    }

    [Fact]
    public void Condition_ComparisionMode()
    {
        var cond1 = new TuringMachineCondition<char>(new TuringMachineState(1), 'x');
        var cond2 = new TuringMachineCondition<char>(new TuringMachineState(1), SymbolAcceptance.AnyValue);
        var cond3 = new TuringMachineCondition<char>(new TuringMachineState(1), SymbolAcceptance.EmptyValue);
        var cond4 = new TuringMachineCondition<char>(new TuringMachineState(1), SymbolAcceptance.NotEmptyValue);
        
        Assert.Equal(SymbolAcceptance.ExactValue, cond1.Mode);
        Assert.Equal(SymbolAcceptance.AnyValue, cond2.Mode);
        Assert.Equal(SymbolAcceptance.EmptyValue, cond3.Mode);
        Assert.Equal(SymbolAcceptance.NotEmptyValue, cond4.Mode);
        var ex = Assert.Throws<ArgumentException>(() => 
            new TuringMachineCondition<char>(new TuringMachineState(1), SymbolAcceptance.ExactValue)
        );
        Assert.Equal("accepts", ex.ParamName);
    }
    
    [Fact]
    public void Condition_InitialStateMustNotBeTerminal()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new TuringMachineCondition<char>(new TuringMachineState(1, isTerminal: true), 'x')
        );
        Assert.Equal("initialState", ex.ParamName);
    }
}