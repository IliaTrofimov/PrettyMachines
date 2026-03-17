

using System.Diagnostics;
using PrettyMachines.Implementations.Data;


namespace PrettyMachines.Implementations.Turing;

/// <summary>Class that helps create <see cref="SingleTapeTuringMachine{TSymbol}"/>.</summary>
/// <typeparam name="TSymbol">Data type of the tape's cells.</typeparam>
[DebuggerDisplay("States {instructions.StatesCount}, instructions {instructions.InstructionsCount}, alphabet length {alphabet.Count}")]
public class TuringMachineBuilder<TSymbol> where TSymbol : IEquatable<TSymbol>
{
    private readonly EqualityComparer<TSymbol> symbolsComparer;
    private readonly InstructionsTable<TSymbol> instructions;
    private readonly TSymbol? emptySymbol;
    
    private int currentStateId;

    
    /// <summary>Create builder.</summary>
    public TuringMachineBuilder(TSymbol? emptySymbol = default, EqualityComparer<TSymbol>? symbolsComparer = null)
    {
        this.symbolsComparer = symbolsComparer ?? EqualityComparer<TSymbol>.Default;
        instructions = new InstructionsTable<TSymbol>(this.symbolsComparer);
        this.emptySymbol = emptySymbol;
    }

    /// <summary>Create Turing machine with all added states, instructions and symbols in the alphabet.</summary>
    public SingleTapeTuringMachine<TSymbol> Build()
    {
        return new SingleTapeTuringMachine<TSymbol>(instructions);
    }
    
    /// <summary>Create Turing machine with all added states, instructions and symbols in the alphabet with given initial state.</summary>
    /// <exception cref="ArgumentException">Initial state must be attached to created machine.</exception>
    public SingleTapeTuringMachine<TSymbol> Build(TuringMachineState initialState)
    {
        return new SingleTapeTuringMachine<TSymbol>(initialState, instructions);
    }

    public TuringMachineBuilder<TSymbol> SetInitialStateId(int stateId)
    {
        if (currentStateId > 0)
            return this;
        if (stateId == TuringMachineState.Halt.Id) 
            throw new ArgumentException("Cannot use reserved state Id.");
        currentStateId = stateId;
        return this;
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

    /// <summary>Start building instruction for given initial state.</summary>
    public TuringMachineConditionBuilder AddInstruction(TuringMachineState initialState)
    {
        return new TuringMachineConditionBuilder(this, initialState);
    }
    
    /// <summary>Adds new instruction that will stop the machine. Initial state must be attached to this object.</summary>
    /// <returns>This Turing machine reference.</returns>
    public TuringMachineBuilder<TSymbol> AddInstruction(TuringMachineCondition<TSymbol> condition)
    {
        return AddInstruction(condition, new TuringMachineAction<TSymbol>(TuringMachineState.Halt));
    }
    
    /// <summary>Adds new instruction with given condition and action. Initial and next states must be attached to this object.</summary>
    /// <returns>This Turing machine reference.</returns>
    public TuringMachineBuilder<TSymbol> AddInstruction(TuringMachineCondition<TSymbol> condition, TuringMachineAction<TSymbol> action)
    {
        if (condition.InitialState.Machine != null)
            throw new ArgumentException($"{nameof(condition.InitialState)} {condition.InitialState} is already attached to other Turing machine", nameof(condition));
        if (action.NextState.Machine != null)
            throw new ArgumentException($"{nameof(action.NextState)} {condition.InitialState} is already attached to other Turing machine", nameof(action));

        if (condition.Mode == SymbolAcceptance.ExactValue && symbolsComparer.Equals(emptySymbol, condition.ScannedSymbol))
        {
            // fix acceptance mode
            condition = new TuringMachineCondition<TSymbol>(condition.InitialState, SymbolAcceptance.EmptyValue);
        }
        
        instructions.AddInstruction(condition, action);
        return this;
    }

    
    public sealed class TuringMachineConditionBuilder
    {
        private readonly TuringMachineBuilder<TSymbol> machineBuilder;
        private readonly TuringMachineState state;
        
        internal TuringMachineConditionBuilder(TuringMachineBuilder<TSymbol> machineBuilder, TuringMachineState state)
        {
            this.machineBuilder = machineBuilder;
            this.state = state;
        }

        /// <summary>This instruction must accept only given symbol.</summary>
        public TuringMachineActionBuilder AcceptExact(TSymbol symbol)
        {
            var condition = new TuringMachineCondition<TSymbol>(state, symbol);
            return new TuringMachineActionBuilder(machineBuilder, condition);
        }
        
        /// <summary>This instruction must accept any not empty symbols.</summary>
        public TuringMachineActionBuilder AcceptNotEmpty()
        {
            var condition = new TuringMachineCondition<TSymbol>(state, SymbolAcceptance.NotEmptyValue);
            return new TuringMachineActionBuilder(machineBuilder, condition);
        }
        
        /// <summary>This instruction must accept only empty symbols.</summary>
        public TuringMachineActionBuilder AcceptEmpty()
        {
            var condition = new TuringMachineCondition<TSymbol>(state, SymbolAcceptance.EmptyValue);
            return new TuringMachineActionBuilder(machineBuilder, condition);
        }
        
        /// <summary>This instruction must accept any symbols.</summary>
        public TuringMachineActionBuilder AcceptAny()
        {
            var condition = new TuringMachineCondition<TSymbol>(state, SymbolAcceptance.AnyValue);
            return new TuringMachineActionBuilder(machineBuilder, condition);
        }
    }

    public sealed class TuringMachineActionBuilder
    {
        private readonly TuringMachineBuilder<TSymbol> machineBuilder;
        private readonly TuringMachineCondition<TSymbol> condition;
        
        internal TuringMachineActionBuilder(TuringMachineBuilder<TSymbol> machineBuilder, TuringMachineCondition<TSymbol> condition)
        {
            this.machineBuilder = machineBuilder;
            this.condition = condition;
        }
        
        /// <summary>
        /// This instruction prints symbol, moves tape to the left and switches state or
        /// keeps initial if <paramref name="nextState"/> is <c>null</c>.
        /// </summary>
        public TuringMachineBuilder<TSymbol> PrintAndMoveLeft(TSymbol symbol, TuringMachineState? nextState = null)
        {
            var action = new TuringMachineAction<TSymbol>(nextState ?? condition.InitialState, symbol, TapeMovement.Left);
            return machineBuilder.AddInstruction(condition, action);
        }
        
        /// <summary>
        /// This instruction prints symbol, moves tape to the right and switches state or
        /// keeps initial if <paramref name="nextState"/> is <c>null</c>.
        /// </summary>
        public TuringMachineBuilder<TSymbol> PrintAndMoveRight(TSymbol symbol, TuringMachineState? nextState = null)
        {
            var action = new TuringMachineAction<TSymbol>(nextState ?? condition.InitialState, symbol, TapeMovement.Right);
            return machineBuilder.AddInstruction(condition, action);
        }
        
        /// <summary>
        /// This instruction prints symbol and switches state or
        /// keeps initial if <paramref name="nextState"/> is <c>null</c>.
        /// </summary>
        public TuringMachineBuilder<TSymbol> Print(TSymbol symbol, TuringMachineState? nextState = null)
        {
            var action = new TuringMachineAction<TSymbol>(nextState ?? condition.InitialState, symbol);
            return machineBuilder.AddInstruction(condition, action);
        }
        
        /// <summary>This instruction prints symbol and stops the machine.</summary>
        public TuringMachineBuilder<TSymbol> PrintAndHalt(TSymbol symbol)
        {
            return Print(symbol, TuringMachineState.Halt);
        }
        
        /// <summary>
        /// This instruction moves tape to the right and switches state or
        /// keeps initial if <paramref name="nextState"/> is <c>null</c>.
        /// </summary>
        public TuringMachineBuilder<TSymbol> MoveRight(TuringMachineState? nextState = null)
        {
            var action = new TuringMachineAction<TSymbol>(nextState ?? condition.InitialState, TapeMovement.Right);
            return machineBuilder.AddInstruction(condition, action);
        }
        
        /// <summary>
        /// This instruction moves tape to the left and switches state or
        /// keeps initial if <paramref name="nextState"/> is <c>null</c>.
        /// </summary>
        public TuringMachineBuilder<TSymbol> MoveLeft(TuringMachineState? nextState = null)
        {
            var action = new TuringMachineAction<TSymbol>(nextState ?? condition.InitialState, TapeMovement.Left);
            return machineBuilder.AddInstruction(condition, action);
        }
        
        /// <summary>This instruction moves tape to the right and stops the machine.</summary>
        public TuringMachineBuilder<TSymbol> MoveRightAndHalt()
        {
            return MoveRight(TuringMachineState.Halt);
        }
        
        /// <summary>This instruction moves tape to the left and stops the machine.</summary>
        public TuringMachineBuilder<TSymbol> MoveLeftAndHalt()
        {
            return MoveLeft(TuringMachineState.Halt);
        }
        
        /// <summary>This instruction stops the machine.</summary>
        public TuringMachineBuilder<TSymbol> Halt()
        {
            var action = new TuringMachineAction<TSymbol>(TuringMachineState.Halt);
            return machineBuilder.AddInstruction(condition, action);
        }
        
        /// <summary>This instruction switches state.</summary>
        public TuringMachineBuilder<TSymbol> Switch(TuringMachineState nextState)
        {
            var action = new TuringMachineAction<TSymbol>(nextState);
            return machineBuilder.AddInstruction(condition, action);
        }
    } 
}