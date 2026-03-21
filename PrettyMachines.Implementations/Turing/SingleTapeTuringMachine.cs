using System.Diagnostics;
using PrettyMachines.Implementations.Data;


namespace PrettyMachines.Implementations.Turing;

/// <summary>Turing Machine with one head that can read single 1d tape.</summary>
/// <typeparam name="TSymbol">Input cell data type.</typeparam>
[DebuggerDisplay("Current state {CurrentState?.ToString(true)}, states {instructions?.StatesCount}, symbols {instructions?.AlphabetLength}, instructions {instructions?.InstructionsCount}")]
public class SingleTapeTuringMachine<TSymbol> : TuringMachine
    where TSymbol : IEquatable<TSymbol>
{
    private readonly InstructionsTable<TSymbol> instructions;
    private readonly TuringMachineState initialState;
    private TuringMachineState currentState;
    
    
    /// <summary>Create new Turing machine with configured instructions table. First added state will be used as initial.</summary>
    /// <remarks>States and instructions must not be empty. All states must not be attached to other machine.</remarks>
    public SingleTapeTuringMachine(InstructionsTable<TSymbol> instructions) 
        : this(instructions.States.MinBy(s => s.Id)!, instructions)
    {
    }
    
    /// <summary>Create new Turing machine with configured instructions table.</summary>
    /// <inheritdoc cref="SingleTapeTuringMachine{TSymbol}(InstructionsTable{TSymbol})"/>
    public SingleTapeTuringMachine(TuringMachineState initialState, InstructionsTable<TSymbol> instructions) 
    {
        if (instructions.StatesCount == 0)
            throw new ArgumentException("Must have at least one state", nameof(instructions));
        if (instructions.InstructionsCount == 0)
            throw new ArgumentException("Must have at least one instruction", nameof(instructions));
        if (initialState.Machine != null)
            throw new InvalidOperationException($"Cannot add state {initialState} that is already attached to other Turing machine.");
        
        var attachedState = instructions.States.FirstOrDefault(s => s.Machine != null);
        if (attachedState != null)
            throw new InvalidOperationException($"Cannot add state {attachedState} that is already attached to other Turing machine.");
        
        foreach (var state in instructions.States) 
            state.Attach(this);
        
        this.instructions = instructions;
        this.initialState = currentState = initialState;
    }
    
    /// <summary>Get readonly instructions.</summary>
    public InstructionsTable<TSymbol>.Readonly Instructions => new(instructions);

    
    /// <summary>
    /// If <c>true</c>, than will throw <see cref="InvalidOperationException"/> after scanning unknown symbols.
    /// </summary>
    public bool StrictAlphabet
    {
        get => instructions.StrictAlphabet;
        set => instructions.StrictAlphabet = value;
    }
    
    /// <summary>Get or set current machine's state. State must be attached to this Turing machine object.</summary>
    /// <exception cref="ArgumentException">New state wasn't attached to this object.</exception>
    public TuringMachineState CurrentState
    {
        get => currentState;
        set
        {
            if (value.Machine is not SingleTapeTuringMachine<TSymbol> m || !ReferenceEquals(m, this))
                throw new ArgumentException($"Cannot set state {value} that is not attached to this Turing machine.", nameof(value));
            currentState = value;
        }
    }
    
    /// <summary>Reset current state to the first added.</summary>
    public void ResetState()
    {
        CurrentState = initialState;
    }
    
    /// <summary>Executes one step with given tape.</summary>
    /// <returns><c>True</c>, if execution can be continued and next state is not terminal, <c>false</c> otherwise.</returns>
    /// <exception cref="InvalidOperationException">Found unknown symbol with StrictAlphabet = true.</exception>
    public bool NextStep(IMachineTape<TSymbol> tape) => NextStep(tape, out _);
    
    /// <summary>Executes one step with given tape. Outputs action that was selected this time.</summary>
    /// <inheritdoc cref="NextStep(IMachineTape{TSymbol})"/>
    public bool NextStep(IMachineTape<TSymbol> tape, out TuringMachineAction<TSymbol>? appliedAction)
    {
        var cell = tape.ReadSymbol();
        
        appliedAction = instructions.FindInstruction(currentState, cell.IsEmpty, cell.Symbol);
        if (!appliedAction.HasValue)
            return false;
        if (appliedAction.Value.ShouldPrintSymbol)
            tape.PutSymbol(appliedAction.Value.PrintedSymbol!);
        
        currentState = appliedAction.Value.NextState;

        if (currentState.IsTerminal)
            return false;
        
        tape.MoveHead(appliedAction.Value.Movement);
        return true;
    }

    /// <summary>Execute steps until terminal state is reached.</summary>
    /// <returns>Amount of executed steps.</returns>
    public int Run(IMachineTape<TSymbol> tape, int maxSteps = 1000) => Run(tape, maxSteps, null);

    /// <summary>Execute steps until terminal state is reached. Each step will invoke given callback.</summary>
    /// <returns>Amount of executed steps.</returns>
    public int Run(IMachineTape<TSymbol> tape,
                   Action<int, TapeSymbol<TSymbol>, TuringMachineState, TuringMachineAction<TSymbol>?>? onStep)
    {
        return Run(tape, 1000, onStep);
    }
    
    /// <summary>Execute steps until terminal state is reached. Each step will invoke given callback.</summary>
    /// <returns>Amount of steps executed.</returns>
    public int Run(IMachineTape<TSymbol> tape, 
                   int maxSteps,
                   Action<int, TapeSymbol<TSymbol>, TuringMachineState, TuringMachineAction<TSymbol>?>? onStep)
    {
        var steps = 0;
        var shouldContinue = true;
        
        while (shouldContinue && steps != maxSteps)
        {
            steps++;
            var cell = tape.ReadSymbol();
            var state = CurrentState;
            shouldContinue = NextStep(tape, out var action);
            onStep?.Invoke(steps, cell, state, action);
        }
        
        return steps;
    }
}