
using System.Diagnostics;


namespace PrettyMachines.Implementations.Turing;

public class TuringMachine<TSymbol> : TuringMachine
    where TSymbol : IEquatable<TSymbol>
{
    private readonly InstructionsTable<TSymbol> instructions;
    private readonly HashSet<TSymbol>? alphabet;
    private TuringMachineState currentState, initialState;
    
    
    /// <summary>Create new Turing machine with configured instructions table.</summary>
    /// <remarks>Machine can scan any symbol <typeparamref name="TSymbol"/>.</remarks>
    public TuringMachine(InstructionsTable<TSymbol> instructions)
    {
        if (instructions.StatesCount == 0)
            throw new ArgumentException("Must have at least one state", nameof(instructions));
        if (instructions.InstructionsCount == 0)
            throw new ArgumentException("Must have at least one instruction", nameof(instructions));
        
        foreach (var state in instructions.States) state.Attach(this);
        this.instructions = instructions;
        initialState = currentState = instructions.States.MinBy(s => s.Id)!;
    }
    
    /// <summary>Create new Turing machine with configured instructions table and fixed alphabet.</summary>
    /// <remarks>Any scanned symbol <typeparamref name="TSymbol"/> that isn't present in the alphabet will cause an exception.</remarks>
    public TuringMachine(InstructionsTable<TSymbol> instructions, HashSet<TSymbol> alphabet) : this(instructions)
    {
        if (alphabet.Count == 0)
            throw new ArgumentException("Alphabet must not be empty.");
        this.alphabet = alphabet;
    }

    public TuringMachine(TuringMachineState initialState, InstructionsTable<TSymbol> instructions) 
        : this(instructions)
    {
        this.initialState = CurrentState = initialState;
    }
    
    public TuringMachine(TuringMachineState initialState, InstructionsTable<TSymbol> instructions, HashSet<TSymbol> alphabet) 
        : this(instructions, alphabet)
    {
        this.initialState = CurrentState = initialState;
    }
    
    
    /// <summary>
    /// If <c>true</c>, than scanning any symbol that isn't present in the alphabet will cause an <see cref="InvalidOperationException"/>.
    /// Does nothing when alphabet wasn't passed to the constructor.
    /// </summary>
    public bool HasStrictAlphabet { get; set; }
    
    /// <summary>Get or set current machine's state. State must be attached to this Turing machine object.</summary>
    /// <exception cref="ArgumentException">New state wasn't attached to this object.</exception>
    public TuringMachineState CurrentState
    {
        get => currentState;
        set
        {
            ValidateState(value);
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
    /// <exception cref="InvalidOperationException">Cannot scan unknown symbols with <see cref="HasStrictAlphabet"/> enabled.</exception>
    public bool NextStep(IMachineTape<TSymbol> tape) => NextStep(tape, out _);
    
    /// <summary>Executes one step with given tape. Outputs action that was selected this time.</summary>
    /// <inheritdoc cref="NextStep(PrettyMachines.Implementations.Turing.IMachineTape{TSymbol})"/>
    public bool NextStep(IMachineTape<TSymbol> tape, out TuringMachineAction<TSymbol>? appliedAction)
    {
        var hasValue = tape.ReadSymbol(out var symbol);
        if (hasValue && HasStrictAlphabet && alphabet?.Contains(symbol!) == false)
        {
            throw new InvalidOperationException($"Symbol '{symbol}' wasn't present in the alphabet. " +
                                                $"Cannot scan unknown symbols with {nameof(HasStrictAlphabet)} enabled.");
        }
        
        appliedAction = instructions.FindInstruction(currentState, !hasValue, symbol);
        if (appliedAction == null)
            return false;
        if (appliedAction.ShouldPrintSymbol)
            tape.PutSymbol(appliedAction.PrintedSymbol!);
        
        currentState = appliedAction.NextState;

        if (currentState.IsTerminal)
            return false;
        
        tape.MoveHead(appliedAction.Movement);
        return true;
    }

    public int Run(IMachineTape<TSymbol> tape, int maxSteps = 1000)
    {
        var steps = 1;
        while (NextStep(tape) && steps != maxSteps)
            steps++;
        return steps;
    }
    
    public IEnumerable<(TapeSymbol<TSymbol>? cell, TuringMachineState state, TuringMachineAction<TSymbol>? action)> RunVerbose(IMachineTape<TSymbol> tape, int maxSteps = 1000)
    {
        var steps = 1;
        var shouldContinue = true;
        
        while (shouldContinue && steps != maxSteps)
        {
            steps++;
            var cell = tape.ReadSymbol();
            var state = CurrentState;
            shouldContinue = NextStep(tape, out var action);
            yield return (cell, state, action);
        }
    }
    
    
    [StackTraceHidden]
    private void ValidateState(TuringMachineState value)
    {
        if (value.Machine is not TuringMachine<TSymbol> m || !ReferenceEquals(m, this))
            throw new ArgumentException($"Cannot set state {value} that is not attached to this Turing machine.", nameof(value));
    }
}