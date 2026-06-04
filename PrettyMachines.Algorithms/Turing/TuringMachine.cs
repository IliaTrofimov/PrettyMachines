using System.Runtime.CompilerServices;
using System.Text;
using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Utils;


namespace PrettyMachines.Algorithms.Turing;

/// <summary>Implementation of the Turing machine with single tape.</summary>
/// <seealso href="https://en.wikipedia.org/wiki/Turing_machine"/>
public class TuringMachine : IAlgorithm<MachineTape>, IAlgorithm<string>
{
    private TuringMachineState _initialState;
    private readonly InstructionsTable instructions;

    /// <inheritdoc cref="IAlgorithm{TData}"/> 
    public string? Name { get; }

    /// <summary>Gets or sets the starting state of this algorithm.</summary>
    public TuringMachineState InitialState
    {
        get => _initialState;
        set
        {
            if (!instructions.States.Contains(value))
                throw new ArgumentException($"Initial state '{value}' does not exist in the TuringMachines", nameof(value));
            _initialState = value;
        }
    }

    /// <summary>
    /// Indicates that this Turing machine uses restricted alphabet and
    /// all unknown symbols will cause algorithm to fail.
    /// </summary>
    public bool HasStrictAlphabet { get; init; }

    /// <summary>Gets readonly version of the instructions used for this machine.</summary>
    public IReadOnlyInstructionsTable Instructions => instructions;
    

    /// <summary>Creates a new builder instance for constructing Turing machines.</summary>
    /// <param name="name">Optional name for the algorithm.</param>
    /// <returns>A builder for fluent configuration.</returns>
    public static ITuringMachineBuilder Create(string? name = null) => new TuringMachineBuilder(name);

    
    /// <summary>
    /// Initialize new algorithm defined by Turing machine with given set of instructions and initial state.
    /// </summary>
    /// <param name="instructions">Set of instruction that define this machine.</param>
    /// <param name="strictAlphabet">Indicates whether this machine will use restricted alphabet or not.</param>
    /// <param name="initialState">
    /// Algorithm will start with this state.
    /// If this value is <c>null</c>, than first state from <paramref name="instructions"/> will be chosen.
    /// </param>
    public TuringMachine(InstructionsTable instructions, bool strictAlphabet = false, TuringMachineState? initialState = null)
        : this(null, instructions, strictAlphabet, initialState)
    {
    }

    /// <param name="name">Name of this algorithm.</param>
    /// <inheritdoc cref="TuringMachine(InstructionsTable,bool,TuringMachineState?)"/>
    public TuringMachine(string? name, InstructionsTable instructions, bool strictAlphabet = false, TuringMachineState? initialState = null)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        if (instructions.RulesCount == 0)
            throw new ArgumentException("Turing machine must have at least 1 rule.", nameof(instructions));
       
        Name = name;
        this.instructions = instructions;
        HasStrictAlphabet = strictAlphabet;
        
        if (initialState == null)
            _initialState = this.instructions.States.OrderBy(s => s.Id).First();
        else
            InitialState = initialState;
    }
    
    
    /// <summary>Applies single step of this algorithm to the current cell of given tape.</summary>
    /// <param name="state">Current state of this machine.</param>
    /// <param name="tape">Tape with input data.</param>
    /// <param name="action">Outputs the action that was applied.</param>
    /// <returns><c>True</c> if Turing machine defines an action for given state and input.</returns>
    public bool NextStep(TuringMachineState state, MachineTape tape, out TuringMachineAction action)
    {
        if (!instructions.TryFindAction(state, tape.CurrentSymbol, out action))
            return false;

        if (action.PrintedSymbol is not null)
            tape.PutSymbol(action.PrintedSymbol);

        tape.MoveHead(action.Movement);
        return true;
    }
    
    /// <inheritdoc/> 
    public AlgorithmResult<string> Execute(string input, AlgorithmCancellation cancellation, bool verbose = false)
    {
        var tape = new MachineTape(input);
        var result = Execute(tape, cancellation, verbose);
        var resultString = MachineTapePrinter.Print(tape);
        return new AlgorithmResult<string>(result.Termination, resultString, result.Steps, result.Trace);
    }

    /// <inheritdoc cref="IAlgorithm{TData}.Execute"/>
    /// <remarks>Tape object will be modified during execution.</remarks>
    public AlgorithmResult<MachineTape> Execute(MachineTape input, AlgorithmCancellation cancellation, bool verbose = false)
    {
        List<string>? trace = null;
        StringBuilder? traceBuilder = null;
        if (verbose)
        {
            trace = [];
            traceBuilder = new StringBuilder(30);
        }

        uint steps = 0;
        var currentState = InitialState;
        var status = TerminationStatus.Aborted;

        while (cancellation.ShouldContinue(steps++) && !currentState.IsTerminal)
        {
            var symbol = input.CurrentSymbol;
            
            if (HasStrictAlphabet && !instructions.Alphabet.Contains(symbol))
            {
                status = TerminationStatus.InvalidInput;
                trace?.Add(CreateErrorTrace(traceBuilder!, currentState, symbol));
                break;
            }
            if (!NextStep(currentState, input, out var action))
            {
                status = TerminationStatus.Stuck;
                trace?.Add(CreateErrorTrace(traceBuilder!, currentState, symbol));
                break;
            }

            trace?.Add(CreateTrace(traceBuilder!, currentState, symbol, in action));
            currentState = action.NextState;
        }

        if (currentState.IsTerminal)
            status = TerminationStatus.Success;
        
        return new AlgorithmResult<MachineTape>(status, input, steps, trace);
    }

    /// <inheritdoc/> 
    public bool ValidateInput(string input)
    {
        return input.Length == 0 || input.All(c => instructions.Alphabet.Contains(c.ToString()));
    }
    
    /// <inheritdoc/> 
    public bool ValidateInput(MachineTape input)
    {
        return input.Length == 0 || input.All(c => instructions.Alphabet.Contains(c));
    }

    private static string CreateTrace(StringBuilder traceBuilder, TuringMachineState state, string? symbol, in TuringMachineAction action)
    {
        traceBuilder.Clear()
            .Append(state.ToString(true))
            .Append(", '")
            .Append(symbol)
            .Append("' -> ")
            .Append(action.ToString(true));
        return traceBuilder.ToString();
    }
    
    private static string CreateErrorTrace(StringBuilder traceBuilder, TuringMachineState state, string? symbol)
    {
        traceBuilder.Clear()
            .Append(state.Id)
            .Append(", '")
            .Append(symbol)
            .Append("' -> ???");
        return traceBuilder.ToString();
    }

    #region Builder classes

    private sealed class TuringMachineBuilder(string? algorithmName) : ITuringMachineBuilder
    {
        private string? _blankSymbol;
        private IEnumerable<string>? _alphabet;
        private StringComparison _stringComparison = StringComparison.Ordinal;
        private TuringMachineState? _initialState;
        private readonly List<TuringMachineState> _states = [];

        public ITuringMachineBuilder WithStringComparer(StringComparison comparison)
        {
            _stringComparison = comparison;
            return this;
        }
        
        public ITuringMachineBuilder WithBlankSymbol(string? blankSymbol)
        {
            _blankSymbol = blankSymbol;
            return this;
        }
        
        public ITuringMachineBuilder WithAlphabet(IEnumerable<string> alphabetSymbols)
        {
            _alphabet = alphabetSymbols;
            return this;
        }

        public ITuringMachineBuilder AddState(string? name, bool isInitial, bool isTerminal,
                                              out TuringMachineState state)
        {
            if (isInitial && isTerminal)
                throw new ArgumentException("State cannot be both initial and terminal.");

            if (string.IsNullOrEmpty(name))
            {
                state = new TuringMachineState(_states.Count, null, isTerminal);
                _states.Add(state);
                return this;
            }
            
            state = _states.Find(s => s.Name == name);
            if (state == null)
            {
                state = new TuringMachineState(_states.Count, name, isTerminal);
                _states.Add(state);
                if (isInitial) _initialState = state;
                return this;
            }
            else if (state.IsTerminal == isTerminal)
            {
                if (isInitial) _initialState = state;
                return this;
            }
            else
            {
                throw new ArgumentException($"State with name '{name}' is already added.");
            }
        }


        public TuringMachine BuildRules(Action<ITuringMachineRuleBuilder> builderFunc)
        {
            try
            {
                var isStrictAlphabet = _alphabet != null;
                var instructions = new InstructionsTable(_alphabet, _blankSymbol, _stringComparison);
                foreach (var state in _states)
                    instructions.AddState(state);
                builderFunc(new TuringMachineRuleBuilder(instructions));
                return new TuringMachine(algorithmName, instructions, isStrictAlphabet, _initialState ?? _states[0]);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error occured while building rules for the {nameof(TuringMachine)}: {ex.Message}", ex);
            }
        }
    }

    private sealed class TuringMachineRuleBuilder(InstructionsTable instructions) : ITuringMachineRuleBuilder
    {
        public TuringMachineState this[string name] 
        {
            get
            {
                var state = instructions.States.FirstOrDefault(s => s.Name == name);
                if (state == null)
                    throw new ArgumentException($"State with name '{name}' does not exist.");
                return state;   
            }
        }

        public ITuringMachineRuleBuilder AddRule(TuringMachineState initialState,
                                                 FuzzyKey<string> scan,
                                                 TuringMachineState nextState,
                                                 string? print,
                                                 TapeMovement move = TapeMovement.None)
        {
            ValidateState(initialState);
            ValidateState(nextState);

            var action = print == null
                ? new TuringMachineAction(nextState, move)
                : new TuringMachineAction(nextState, print, move);

            instructions.AddRule(initialState, in scan, in action);
            return this;
        }

        private void ValidateState(TuringMachineState state, [CallerArgumentExpression("state")] string paramName = "")
        {
            if (!state.Equals(TuringMachineState.Halt) && !instructions.States.Any(s => ReferenceEquals(state, s)))
                throw new ArgumentException($"State {state} does not exist.", paramName);
        }
    }
    
    #endregion
}