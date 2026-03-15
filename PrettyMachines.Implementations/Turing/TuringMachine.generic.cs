using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


namespace PrettyMachines.Implementations.Turing;


/// <summary>Represents Turing machine.</summary>
/// <typeparam name="TSymbol">Data type of the tape's cells.</typeparam>
[DebuggerDisplay("States {States.Count,nq}, alphabet length {alphabet.Count,nq}")]
public class TuringMachine<TSymbol> : FixedTuringMachine<TSymbol>
    where TSymbol : IEquatable<TSymbol>
{
    private readonly IEqualityComparer<TSymbol> symbolsComparer;
    private readonly InstructionsTable<TSymbol> instructions;
    private readonly HashSet<TSymbol> alphabet;
    private readonly bool hasFixedAlphabet;
    private readonly TSymbol? emptySymbol;
    
    public TuringMachineState? CurrentState { get; set; }

    
    /// <summary>Creates new Turing machine without specifying alphabet.</summary>
    /// <remarks>Alphabet will be determined from added instructions later.</remarks>
    /// <param name="symbolsComparer">Object that will test equality of symbols and provide hash codes.</param>
    public TuringMachine(IEqualityComparer<TSymbol>? symbolsComparer = null)
    {
        this.symbolsComparer ??= EqualityComparer<TSymbol>.Default;
        instructions = new InstructionsTable<TSymbol>(this.symbolsComparer);
        hasFixedAlphabet = false;
        alphabet = new HashSet<TSymbol>(this.symbolsComparer);
        emptySymbol = default;
    }
    
    /// <summary>Creates new Turing machine with specified alphabet.</summary>
    /// <remarks>All instructions must have symbols specified in given alphabet.</remarks>
    /// <param name="alphabet">
    /// Set of predetermined symbols that machine can read and understand. Empty symbol can be omitted from this list.
    /// Scanning any not-empty symbol that wasn't listed in this alphabet will cause machine to stop.
    /// </param>
    /// <param name="symbolsComparer">Object that will test equality of symbols and provide hash codes.</param>
    ///  <exception cref="ArgumentException">Alphabet cannot be empty.</exception>
    public TuringMachine(ICollection<TSymbol> alphabet, IEqualityComparer<TSymbol>? symbolsComparer = null, TSymbol? emptySymbol = default)
    {
        if (alphabet.Count == 0)
            throw new ArgumentException("Turing machine's alphabet must not be empty.");

        this.symbolsComparer ??= EqualityComparer<TSymbol>.Default;
        instructions = new InstructionsTable<TSymbol>(this.symbolsComparer);
        hasFixedAlphabet = false;
        this.alphabet = new HashSet<TSymbol>(alphabet, this.symbolsComparer);
        this.alphabet.Remove(emptySymbol);
        this.emptySymbol = emptySymbol;
    }

    /// <inheritdoc/> 
    public override int GetNewStateIndex()
    {
        return States.Count;
    }

    /// <summary>Adds new unnamed non-terminating state.</summary>
    public TuringMachineState AddState() => AddState(null);
    
    /// <summary>Adds new unnamed terminating state.</summary>
    public TuringMachineState AddTerminalState() => AddState(null, true);
    
    /// <summary>Adds new named state that can be terminating or not.</summary>
    public TuringMachineState AddState(string? name, bool isTerminal = false)
    {
        var state = new TuringMachineState(this, name, isTerminal);
        States.Add(state);
        return state;
    }
    
    /// <summary>Adds new instruction with given condition that will stop this machine. Initial state must be attached to this object.</summary>
    /// <returns>This Turing machine reference.</returns>
    /// <exception cref="ArgumentException">Initial state must be attached to this object</exception>
    public TuringMachine<TSymbol> AddTerminatingInstruction(TuringMachineCondition<TSymbol> condition)
    {
        return AddInstruction(condition, new TuringMachineAction<TSymbol>(TuringMachineState.DefaultTerminalState));    
    }
    
    /// <summary>Adds new instruction with given condition that will stop this machine. Initial state must be attached to this object.</summary>
    /// <returns>This Turing machine reference.</returns>
    /// <exception cref="ArgumentException">Initial state must be attached to this object</exception>
    public TuringMachine<TSymbol> AddTerminatingInstruction(TuringMachineCondition<TSymbol> condition, TSymbol printedSymbol)
    {
        return AddInstruction(condition, new TuringMachineAction<TSymbol>(TuringMachineState.DefaultTerminalState, printedSymbol));    
    }

    /// <summary>Adds new instruction with given condition and action. Initial and next states must be attached to this object.</summary>
    /// <returns>This Turing machine reference.</returns>
    /// <exception cref="ArgumentException">Initial and next states must be attached to this object</exception>
    public TuringMachine<TSymbol> AddInstruction(TuringMachineCondition<TSymbol> condition, TuringMachineAction<TSymbol> action)
    {
        if (condition.InitialState.Machine is not TuringMachine<TSymbol> m1 || !ReferenceEquals(m1, this))
            throw new ArgumentException($"{nameof(condition.InitialState)} isn't attached to this Turing machine.", nameof(condition));

        if (action.NextState.Id != TuringMachineState.DefaultTerminalState.Id)
        {
            if (action.NextState.Machine is not TuringMachine<TSymbol> m2 || !ReferenceEquals(m2, this))
                throw new ArgumentException($"{nameof(action.NextState)} isn't attached to this Turing machine.", nameof(action));
        }

        if (hasFixedAlphabet)
        {
            if (condition.Mode == SymbolAcceptance.ExactValue && !alphabet.Contains(condition.ScannedSymbol!))
                throw new ArgumentException($"{nameof(condition.ScannedSymbol)} does not listed in the initial alphabet.", nameof(condition));
            if (action.ShouldPrintSymbol && !alphabet.Contains(action.PrintedSymbol!))
                throw new ArgumentException($"{nameof(action.PrintedSymbol)} does not listed in the initial alphabet.", nameof(action));
        }
        else
        {
            if (condition.Mode == SymbolAcceptance.ExactValue && !symbolsComparer.Equals(emptySymbol, condition.ScannedSymbol))
                alphabet.Add(condition.ScannedSymbol!);
            if (action.ShouldPrintSymbol && !symbolsComparer.Equals(emptySymbol, action.PrintedSymbol))
                alphabet.Add(action.PrintedSymbol!);
        }

        if (condition.Mode == SymbolAcceptance.ExactValue && symbolsComparer.Equals(emptySymbol, condition.ScannedSymbol))
        {
            // fix acceptance mode
            condition = new TuringMachineCondition<TSymbol>(condition.InitialState, SymbolAcceptance.EmptyValue);
        }
        
        CurrentState ??= condition.InitialState;
        instructions.AddInstruction(condition, action);
        return this;
    }
    
    /// <summary>Execute one step.</summary>
    /// <returns><c>True</c>, if execution can be continued.</returns>
    /// <exception cref="InvalidOperationException">Turing machine wasn't properly initialized.</exception>
    public override bool NextStep(IMachineTape<TSymbol> tape, [NotNullWhen(true)] out TuringMachineAction<TSymbol>? appliedAction)
    {
        if (CurrentState == null)
            throw new InvalidOperationException("The Turing Machine was not initialized.");
        
        var hasValue = tape.ReadSymbol(out var symbol);
        appliedAction = instructions.FindInstruction(CurrentState, !hasValue, symbol);
        
        if (appliedAction == null)
            return false;
        
        CurrentState = appliedAction.NextState;

        if (appliedAction.ShouldPrintSymbol)
            tape.PutSymbol(appliedAction.PrintedSymbol!);
        
        if (CurrentState.IsTerminal)
            return false;
        
        tape.MoveHead(appliedAction.Movement);
        return true;
    }
}