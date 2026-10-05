using System.Runtime.CompilerServices;
using System.Text;
using PrettyMachines.Abstract;
using PrettyMachines.Automata;


namespace PrettyMachines.FSM;

/// <summary>Implementation of the deterministic finite-state machine (or automaton).</summary>
/// <seealso href="https://en.wikipedia.org/wiki/Finite-state_machine"/>
public class FiniteStateMachine : IAlgorithm, IAlgorithm<string, string>
{
    private AutomatonState? _initialState;
    private readonly FsmTransitionTable table;


    /// <inheritdoc cref="IAlgorithm{TInput,TOutput}"/>
    public string? Name { get; }

    /// <summary>Gets or sets the starting state of this machine.</summary>
    public AutomatonState InitialState
    {
        get => _initialState!;
        set
        {
            if (!table.States.Contains(value))
                throw new ArgumentException($"Initial state '{value}' does not exist in the FiniteStateMachine", nameof(value));
            _initialState = value;
        }
    }

    /// <summary>
    /// Indicates that this machine uses restricted alphabet and
    /// all unknown symbols will cause algorithm to fail.
    /// </summary>
    public bool HasStrictAlphabet { get; init; }

    /// <summary>Gets the ordered collection of transitions used by this machine.</summary>
    public IReadOnlyList<AutomatonInstruction<AutomatonState, char, FsmAction>> Transitions => table.Rules;

    /// <summary>Gets the output token produced when the input is accepted.</summary>
    public string AcceptedOutput { get; init; } = "A";

    /// <summary>Gets the output token produced when the input is rejected.</summary>
    public string RejectedOutput { get; init; } = "R";


    /// <summary>Creates a new builder instance for constructing finite state machines.</summary>
    /// <param name="name">Optional name for the algorithm.</param>
    /// <returns>A builder for fluent configuration.</returns>
    public static IFiniteStateMachineBuilder Create(string? name = null) => new FiniteStateMachineBuilder(name);

    /// <summary>
    /// Initializes new algorithm defined by a finite state machine with given set of transitions and initial state.
    /// </summary>
    /// <param name="transitions">Set of transitions that define this machine.</param>
    /// <param name="strictAlphabet">Indicates whether this machine will use restricted alphabet or not.</param>
    /// <param name="initialState">
    /// Algorithm will start with this state.
    /// If this value is <c>null</c>, than first state from <paramref name="transitions"/> will be chosen.
    /// </param>
    public FiniteStateMachine(FsmTransitionTable transitions, bool strictAlphabet = false, AutomatonState? initialState = null)
        : this(null, transitions, strictAlphabet, initialState)
    {
    }

    /// <param name="name">Name of this algorithm.</param>
    /// <inheritdoc cref="FiniteStateMachine(FsmTransitionTable,bool,AutomatonState?)"/>
    public FiniteStateMachine(string? name, FsmTransitionTable transitions, bool strictAlphabet = false, AutomatonState? initialState = null)
    {
        ArgumentNullException.ThrowIfNull(transitions);
        if (transitions.States.Count == 0)
            throw new ArgumentException("Finite state machine must have at least 1 state.", nameof(transitions));

        Name = name;
        table = transitions;
        HasStrictAlphabet = strictAlphabet;

        if (initialState == null)
            _initialState = transitions.States.OrderBy(s => s.Id).First();
        else
            InitialState = initialState;
    }


    /// <inheritdoc/>
    public bool ValidateInput(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Length == 0 || !HasStrictAlphabet)
            return true;

        return input.All(table.Alphabet.Contains);
    }

    /// <summary>Applies single step of this algorithm to the given symbol.</summary>
    /// <param name="state">Current state of this machine.</param>
    /// <param name="symbol">Input symbol to scan.</param>
    /// <param name="action">Outputs the action that was applied.</param>
    /// <returns><c>True</c> if this machine defines an action for given state and symbol.</returns>
    public bool NextStep(AutomatonState state, char symbol, out FsmAction action)
    {
        return table.TryFindAction(state, symbol, out action);
    }

    /// <inheritdoc/>
    public IEnumerable<IAlgorithmSnapshot<string>> Run(string input, AlgorithmCancellation cancellation, bool verbose = false)
    {
        ArgumentNullException.ThrowIfNull(input);
        return RunIterator(input, cancellation, verbose);
    }

    IEnumerable<IAlgorithmSnapshot> IAlgorithm.Run(string input, AlgorithmCancellation cancellation, bool verbose)
    {
        return Run(input, cancellation, verbose);
    }

    /// <inheritdoc/>
    public AlgorithmResult<string> Execute(string input, AlgorithmCancellation cancellation, bool verbose = false)
    {
        return AlgorithmRunner.Execute(Run(input, cancellation, verbose));
    }


    private IEnumerable<IAlgorithmSnapshot<string>> RunIterator(string input, AlgorithmCancellation cancellation, bool verbose)
    {
        if (!ValidateInput(input))
        {
            yield return new FiniteStateMachineSnapshot(0, TerminationStatus.InvalidInput, RejectedOutput, null);
            yield break;
        }

        var state = InitialState;
        var traceBuilder = verbose ? new StringBuilder(30) : null;

        if (input.Length == 0)
        {
            var accepted = state.IsTerminal;
            var trace = verbose ? CreateInitialTrace(traceBuilder!, state) : null;
            yield return new FiniteStateMachineSnapshot(
                0,
                accepted ? TerminationStatus.Success : TerminationStatus.Stuck,
                accepted ? AcceptedOutput : RejectedOutput,
                trace);
            yield break;
        }

        yield return new FiniteStateMachineSnapshot(
            0,
            TerminationStatus.Unknown,
            state.ToString(),
            verbose ? CreateInitialTrace(traceBuilder!, state) : null);

        var position = 0;
        long steps = 0;
        while (cancellation.ShouldContinue((uint)steps + 1))
        {
            var symbol = input[position];

            if (HasStrictAlphabet && !table.Alphabet.Contains(symbol))
            {
                yield return new FiniteStateMachineSnapshot(
                    steps,
                    TerminationStatus.InvalidInput,
                    RejectedOutput,
                    verbose ? CreateErrorTrace(traceBuilder!, state, symbol) : null);
                yield break;
            }

            if (!NextStep(state, symbol, out var action))
            {
                yield return new FiniteStateMachineSnapshot(
                    steps,
                    TerminationStatus.Stuck,
                    RejectedOutput,
                    verbose ? CreateErrorTrace(traceBuilder!, state, symbol) : null);
                yield break;
            }

            var traceLine = verbose ? CreateTrace(traceBuilder!, state, symbol, in action) : null;
            var appliedInstruction = table.IndexOf(state, symbol);
            state = action.NextState;
            position++;
            steps++;

            var finished = position == input.Length;
            var termination = finished
                ? (state.IsTerminal ? TerminationStatus.Success : TerminationStatus.Stuck)
                : TerminationStatus.Unknown;
            var output = finished
                ? (state.IsTerminal ? AcceptedOutput : RejectedOutput)
                : state.ToString();

            yield return new FiniteStateMachineSnapshot(steps, termination, output, traceLine, appliedInstruction);

            if (finished)
                yield break;
        }
    }

    private static string CreateInitialTrace(StringBuilder traceBuilder, AutomatonState state)
    {
        return traceBuilder.Clear()
            .Append(state)
            .ToString();
    }

    private static string CreateTrace(StringBuilder traceBuilder, AutomatonState state, char symbol, in FsmAction action)
    {
        return traceBuilder.Clear()
            .Append(state)
            .Append('\'')
            .Append(symbol)
            .Append('\'')
            .Append(" -> ")
            .Append(action.ToFormattedString('\''))
            .ToString();
    }

    private static string CreateErrorTrace(StringBuilder traceBuilder, AutomatonState state, char symbol)
    {
        return traceBuilder.Clear()
            .Append(state)
            .Append('\'')
            .Append(symbol)
            .Append('\'')
            .Append(" -> ???")
            .ToString();
    }


    #region Builder classes

    private sealed class FiniteStateMachineBuilder(string? algorithmName) : IFiniteStateMachineBuilder
    {
        private IEnumerable<char>? _alphabet;
        private AutomatonState? _initialState;
        private string _acceptedOutput = "A";
        private string _rejectedOutput = "R";
        private readonly List<AutomatonState> _states = [];

        public IFiniteStateMachineBuilder WithAlphabet(IEnumerable<char> alphabetSymbols)
        {
            _alphabet = alphabetSymbols;
            return this;
        }

        public IFiniteStateMachineBuilder WithOutput(string acceptedOutput, string rejectedOutput)
        {
            _acceptedOutput = acceptedOutput;
            _rejectedOutput = rejectedOutput;
            return this;
        }

        public IFiniteStateMachineBuilder AddState(string? name, bool isInitial, bool isTerminal, out AutomatonState state)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var existing = _states.Find(s => s.Name == name);
                if (existing != null)
                {
                    if (existing.IsTerminal != isTerminal)
                        throw new ArgumentException($"State with name '{name}' is already added.");

                    if (isInitial) _initialState = existing;
                    state = existing;
                    return this;
                }
            }

            state = new AutomatonState(_states.Count, string.IsNullOrEmpty(name) ? null : name, isTerminal);
            _states.Add(state);
            if (isInitial) _initialState = state;
            return this;
        }

        public FiniteStateMachine BuildRules(Action<IFiniteStateMachineRuleBuilder> builderFunc)
        {
            try
            {
                var isStrictAlphabet = _alphabet != null;
                var transitions = _alphabet != null
                    ? new FsmTransitionTable(_alphabet)
                    : new FsmTransitionTable();
                foreach (var state in _states)
                    transitions.AddState(state);
                builderFunc(new FiniteStateMachineRuleBuilder(transitions));
                return new FiniteStateMachine(algorithmName, transitions, isStrictAlphabet, _initialState ?? _states[0])
                {
                    AcceptedOutput = _acceptedOutput,
                    RejectedOutput = _rejectedOutput,
                };
            }
            catch (AlgorithmException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error occured while building rules for the {nameof(FiniteStateMachine)}: {ex.Message}", ex);
            }
        }
    }

    private sealed class FiniteStateMachineRuleBuilder(FsmTransitionTable transitions) : IFiniteStateMachineRuleBuilder
    {
        public AutomatonState this[string name]
        {
            get
            {
                var state = transitions.States.FirstOrDefault(s => s.Name == name);
                if (state == null)
                    throw new ArgumentException($"State with name '{name}' does not exist.");
                return state;
            }
        }

        public IFiniteStateMachineRuleBuilder AddRule(AutomatonState from, FuzzyKey<char> scan, AutomatonState to)
        {
            ValidateState(from);
            ValidateState(to);

            if (scan.Match is SymbolMatch.Empty or SymbolMatch.NotEmpty)
                throw new AlgorithmException($"Finite state machine rules do not support '{scan.Match}' symbol match; use 'Exact' or 'Any'.");

            transitions.AddRule(from, in scan, new FsmAction(to));
            return this;
        }

        private void ValidateState(AutomatonState state, [CallerArgumentExpression("state")] string paramName = "")
        {
            if (!transitions.States.Any(s => ReferenceEquals(state, s)))
                throw new ArgumentException($"State {state} does not exist.", paramName);
        }
    }

    #endregion
}
