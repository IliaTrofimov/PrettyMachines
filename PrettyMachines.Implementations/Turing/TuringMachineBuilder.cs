

using System.Diagnostics;


namespace PrettyMachines.Implementations.Turing;

/// <summary>Class that helps create <see cref="TuringMachine{TSymbol}"/>.</summary>
/// <typeparam name="TSymbol">Data type of the tape's cells.</typeparam>
[DebuggerDisplay("States {instructions.StatesCount}, instructions {instructions.InstructionsCount}, alphabet length {alphabet.Count}")]
public class TuringMachineBuilder<TSymbol> where TSymbol : IEquatable<TSymbol>
{
    private readonly EqualityComparer<TSymbol> symbolsComparer;
    private readonly InstructionsTable<TSymbol> instructions;
    private readonly HashSet<TSymbol> alphabet;
    private readonly TSymbol? emptySymbol;
    
    private int currentStateId;

    
    /// <summary>Create builder.</summary>
    public TuringMachineBuilder(TSymbol? emptySymbol = default, EqualityComparer<TSymbol>? symbolsComparer = null)
    {
        this.symbolsComparer = symbolsComparer ?? EqualityComparer<TSymbol>.Default;
        instructions = new InstructionsTable<TSymbol>(this.symbolsComparer);
        alphabet = new HashSet<TSymbol>(this.symbolsComparer);
        this.emptySymbol = emptySymbol;
    }

    /// <summary>Create builder with given initial alphabet.</summary>
    public TuringMachineBuilder(ICollection<TSymbol> alphabet, TSymbol? emptySymbol = default, EqualityComparer<TSymbol>? symbolsComparer = null)
    {
        this.symbolsComparer = symbolsComparer ?? EqualityComparer<TSymbol>.Default;
        instructions = new InstructionsTable<TSymbol>(this.symbolsComparer);
        this.alphabet = new HashSet<TSymbol>(alphabet, this.symbolsComparer);
        this.alphabet.Remove(emptySymbol);
        this.emptySymbol = emptySymbol;
    }

    /// <summary>Create Turing machine with all added states, instructions and symbols in the alphabet.</summary>
    public TuringMachine<TSymbol> Build()
    {
        return new TuringMachine<TSymbol>(instructions, alphabet);
    }
    
    /// <summary>Create Turing machine with all added states, instructions and symbols in the alphabet with given initial state.</summary>
    /// <exception cref="ArgumentException">Initial state must be attached to created machine.</exception>
    public TuringMachine<TSymbol> Build(TuringMachineState initialState)
    {
        return new TuringMachine<TSymbol>(initialState, instructions, alphabet);
    }

    
    /// <summary>Adds new unnamed non-terminating state.</summary>
    public TuringMachineState AddState() => AddState(null);
    
    /// <summary>Adds new unnamed terminating state.</summary>
    public TuringMachineState AddTerminalState() => AddState(null, true);
    
    /// <summary>Adds new named state that can be terminating or not.</summary>
    public TuringMachineState AddState(string? name, bool isTerminal = false)
    {
        return new TuringMachineState(currentStateId++, name, isTerminal);
    }
    
    
    /// <summary>Adds new instruction with given condition that will stop this machine. Initial state must be attached to this object.</summary>
    /// <returns>This Turing machine reference.</returns>
    public TuringMachineBuilder<TSymbol> AddTerminatingInstruction(TuringMachineCondition<TSymbol> condition)
    {
        return AddInstruction(condition, new TuringMachineAction<TSymbol>(TuringMachineState.DefaultTerminalState));    
    }
    
    /// <summary>Adds new instruction with given condition that will stop this machine. Initial state must be attached to this object.</summary>
    /// <returns>This Turing machine reference.</returns>
    public TuringMachineBuilder<TSymbol> AddTerminatingInstruction(TuringMachineCondition<TSymbol> condition, TSymbol printedSymbol)
    {
        return AddInstruction(condition, new TuringMachineAction<TSymbol>(TuringMachineState.DefaultTerminalState, printedSymbol));    
    }

    /// <summary>Adds new instruction with given condition and action. Initial and next states must be attached to this object.</summary>
    /// <returns>This Turing machine reference.</returns>
    public TuringMachineBuilder<TSymbol> AddInstruction(TuringMachineCondition<TSymbol> condition, TuringMachineAction<TSymbol> action)
    {
        if (condition.InitialState.Machine != null)
            throw new ArgumentException($"{nameof(condition.InitialState)} is already attached to another Turing machine", nameof(condition));
        if (action.NextState.Machine != null)
            throw new ArgumentException($"{nameof(action.NextState)} is already attached to another Turing machine", nameof(action));

        if (condition.Mode == SymbolAcceptance.ExactValue)
        {
            if (symbolsComparer.Equals(emptySymbol, condition.ScannedSymbol))
            {
                // fix acceptance mode
                condition = new TuringMachineCondition<TSymbol>(condition.InitialState, SymbolAcceptance.EmptyValue);
            }
            else
            {
                alphabet.Add(condition.ScannedSymbol!);
            }
        }

        if (action.ShouldPrintSymbol && !symbolsComparer.Equals(emptySymbol, action.PrintedSymbol))
        {
            alphabet.Add(action.PrintedSymbol!);
        }
        
        instructions.AddInstruction(condition, action);
        return this;
    }
}