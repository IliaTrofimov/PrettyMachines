# Pretty Machines

[![Tests](https://github.com/IliaTrofimov/PrettyMachines/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/IliaTrofimov/PrettyMachines/actions/workflows/ci.yml)

A .NET 10 library for building and running automatons like Turing machines and Markov algorithms. You can try pre-made algorithms or create your own in this [demo application](https://iliatrofimov.github.io/PrettyMachines/) (или [версия на русском](https://iliatrofimov.github.io/PrettyMachines/?lang=rus)).

### Features

- Basic abstract interface for all formal algorithms. Each algorithm can be executed step-by-step (using `IEnumerable`) or with one action (from start to the end). 
- Algorithm snapshots carry debug information about each step of the algoritm run.
- Fluent builders for Turing machines, finite state machines and Markov algorithms.
- Turing machine definition is expanded. Machine can scan special symbols like `empty`, `non-empty` or `any`.
- Shared `PrettyMachines.Automata` core: common state, symbol-matching and transition-table primitives reused by Turing and finite state machines.
- Several example algorithms.

### Requirements
[.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) for main projects, [xUnit](https://github.com/xunit/xunit) and [FluentAssertions](https://github.com/fluentassertions/fluentassertions) for unit tests.

## Table of contents

- [Solution layout](#solution-layout)
- [Core concepts](#core-concepts)
- [Quick start: Turing machine](#quick-start-turing-machine)
- [Quick start: finite state machine](#quick-start-finite-state-machine)
- [Quick start: Markov algorithm](#quick-start-markov-algorithm)
- [Execution results](#execution-results)
- [Built-in algorithms](#built-in-algorithms)
- [Printing and parsing](#printing-and-parsing)

## Solution layout

1. `PrettyMachines.Algorithms` - Core library: abstract algorithm model, `Turing`, `Markov` and `Utils` (printing/parsing)
2. `PrettyMachines.Implementations` - Ready-to-use algorithms built on the core library.
3. `PrettyMachines.BlazorUI` - Blazor WebAssembly app for building and executing algorithms.
4. `PrettyMachines.Tests` - xUnit tests for the core library and implementations.

## Core concepts

### Formal algorithm

An algorithm is any well-defined set of instructions that, when followed, terminates after a finite number
of steps and comprises a solution to a given computational problem. This project represents algorithms as
effective methods that satisfy the following:

- They consist of a finite number of exact, finite instructions.
- Applied to a problem from their class, they always terminate and always produce a correct answer.
- Their instructions can be followed rigorously, without requiring ingenuity.

Optionally, an algorithm may be required never to return a result as if it were an answer for inputs from
outside its class. Adding this requirement reduces the set of classes that have an effective method.

### Contract

All algorithms implement `IAlgorithm` (text in, text out) and optionally the strongly-typed
`IAlgorithm<TInput, TOutput>`. Instances are immutable once built, so their instructions cannot change
during execution.

| Member | Purpose |
| --- | --- |
| `Name` | Optional algorithm name. |
| `ValidateInput(input)` | Checks whether an input belongs to the algorithm's class. |
| `Run(input, cancellation, verbose)` | Lazily yields the initial snapshot followed by one snapshot per step. |
| `Execute(input, cancellation, verbose)` | Consumes `Run` and returns the final `AlgorithmResult<T>`. |

Convenience extension: `algorithm.Execute(input, verbose: true)` uses `AlgorithmCancellation.Default`
(1000 steps).

### Execution and cancellation

`AlgorithmCancellation` bounds execution with a maximum step count and/or an external `CancellationToken`.
`AlgorithmCancellation.Default` allows 1000 steps; a step limit of `0` means unlimited.

```csharp
var cancellation = new AlgorithmCancellation(10_000);
var result = machine.Execute("101", cancellation, verbose: true);
```

### Termination status

| Status | Meaning |
| --- | --- |
| `Unknown` | Execution is still running (`Run` only). |
| `Success` | A terminal state or terminal rule was reached. |
| `Stuck` | No instruction was applicable. |
| `Aborted` | Cancelled from outside or trapped in a loop past the step limit. |
| `InvalidInput` | Input is outside the algorithm's class. |

`Run` also lets you inspect intermediate states, which is useful for stepping debuggers and UIs:

```csharp
foreach (var snapshot in machine.Run("101", new AlgorithmCancellation(10_000)))
    Console.WriteLine($"step {snapshot.Steps}: {snapshot.Output} [{snapshot.Termination}]");
```

## Quick start: Turing machine

A [Turing machine](https://en.wikipedia.org/wiki/Turing_machine) is a mathematical model of computation describing an abstract machine that manipulates symbols on a strip of tape according to a table of rules. Despite the model's simplicity, it is capable of implementing any computer algorithm. Machine is defined by an alphabet (optionally strict), a blank symbol, a set of states with one initial state, and a transition table.

```csharp
using PrettyMachines.Abstract;
using PrettyMachines.Turing;
using PrettyMachines.Utils.Printing;

var machine = TuringMachine.Create("Toggle first bit")
    .WithAlphabet("0", "1") // strict alphabet; unknown symbols fail the machine
    .WithBlankSymbol("_")
    .AddInitialState("scan", out var q0)
    .AddTerminalState("done", out var qDone)
    .BuildRules(rules => rules
        .AddRule(q0, "0", qDone, "1", TapeMovement.Right)
        .AddRule(q0, "1", qDone, "0", TapeMovement.Right));

var result = machine.Execute("101", new AlgorithmCancellation(10_000), verbose: true);

Console.WriteLine($"{result.Output} ({result.Termination} after {result.Steps} steps)");
Console.WriteLine(InstructionTablePrinter.PrintTable(machine));
```

Rules can also reference states by name (`rules.AddRule("scan", "0", "done", ...)`), and a rule that reads
`SymbolMatch.Empty`, `SymbolMatch.NotEmpty` or `SymbolMatch.Any` matches a whole class of cells:

```csharp
using PrettyMachines.Automata; // SymbolMatch, FuzzyKey and FuzzyKeyComparer live here
using PrettyMachines.Turing;

rules.AddRule(q0, SymbolMatch.NotEmpty, q0, null, TapeMovement.Right)
     .AddHalt(q0, SymbolMatch.Empty, "1"); // AddHalt targets TuringMachineState.Halt
```

The same machine typed over the tape (no input mutation of the caller's tape):

```csharp
var tape = new MachineTape(new[] { "1", "0", "1" }, blankSymbol: "_");
AlgorithmResult<IReadOnlyTape> tapeResult = machine.Execute(tape, new AlgorithmCancellation(10_000));
```

## Quick start: finite state machine

A deterministic [finite state machine](https://en.wikipedia.org/wiki/Finite-state_machine) (DFA) is
defined by an alphabet, a set of states with one initial state, accepting (terminal) states, and at most
one transition per `(state, symbol)`. The machine consumes the whole input; after the input is exhausted
it accepts when the current state is accepting, otherwise it rejects.

```csharp
using PrettyMachines.Abstract;
using PrettyMachines.Automata;
using PrettyMachines.FSM;

var dfa = FiniteStateMachine.Create("Even number of ones")
    .WithAlphabet('0', '1')                 // strict alphabet; unknown symbols yield InvalidInput
    .AddTerminalState("even", out var qEven) // accepting state; the first state is the initial one
    .AddState("odd", out var qOdd)
    .BuildRules(rules => rules
        .AddRule(qEven, '0', qEven)
        .AddRule(qEven, '1', qOdd)
        .AddRule(qOdd, '0', qOdd)
        .AddRule(qOdd, '1', qEven));

var result = dfa.Execute("1101", new AlgorithmCancellation(1000), verbose: true);
Console.WriteLine($"{result.Output} ({result.Termination} after {result.Steps} steps)");
```

- Accepting/final states are declared with `AddTerminalState`/`AddState(..., isTerminal: true)`.
  Reaching one does **not** stop the run, and transitions out of accepting states are allowed.
- `AddRule(from, SymbolMatch.Any, to)` is a catch-all ("otherwise") transition. `SymbolMatch.Empty` and
  `SymbolMatch.NotEmpty` are not valid for finite state machines and throw `AlgorithmException`.
- States can also be referenced by name: `rules.AddRule("even", '1', "odd")`.
- Acceptance produces `"A"`, rejection produces `"R"`; both tokens are configurable with
  `WithOutput("accepted", "rejected")`.

You can also use implicit states creation shortcut:

```csharp
var dfa = FiniteStateMachine.Create()
    .AddState("one", out var qOne)
    .BuildRules(rules => rules
        .AddRule(qOne, '1', "qOther") // "qOther" will add new state
        .AddRule("qOther", 'x', qOne));
```

Several transitions between 2 states can be created with `char` array like this:

```csharp
var dfa = FiniteStateMachine.Create()
    .BuildRules(rules => rules
        // this method...
        .AddRule("qDigits", new char[] {'0', '1', '2', ... }, "qOther"))
        // is equalto these 3 operations:
        .AddRule("qDigits", '0', "qOther")
        .AddRule("qDigits", '1', "qOther")
        .AddRule("qDigits", '2', "qOther"));
```

## Quick start: Markov algorithm

A [Markov algorithm](https://en.wikipedia.org/wiki/Markov_algorithm) is a string rewriting system that uses grammar-like rules to operate on strings of symbols. Markov algorithms have been shown to be Turing-complete, which means that they are suitable as a general model of computation and can represent any mathematical expression from its simple notation. Markov algorithms are named after the Soviet mathematician Andrey Markov, Jr. Algorithm applies the first matching substitution rule, replacing the leftmost occurrence of its pattern. A rule marked terminal stops the algorithm after it is applied.

```csharp
using PrettyMachines.Abstract;
using PrettyMachines.Markov;

var algorithm = MarkovAlgorithm.Create("Capitalize")
    .WithAlphabet('a', 'b', 'c')
    .WithMarkers('*')
    .AddRule("a", "A").WithComment("uppercase a")
    .AddRule("b", "B").WithComment("uppercase b")
    .AddRule("c", "C", isTerminal: true).WithComment("uppercase c and stop")
    .Build();

var result = algorithm.Execute("abc", new AlgorithmCancellation(1000));
```

- An empty pattern prepends the replacement (`AddRule("", "$")`).
- An empty replacement deletes the pattern.
- The order of rules matters: the first matching rule wins.
- `WithAlphabet` restricts input; symbols outside the alphabet yield `InvalidInput`.
- `WithMarkers` declares special symbols that rules may produce but input must not contain.

## Execution results

`AlgorithmResult<T>` is the outcome of `Execute`:

| Member | Description |
| --- | --- |
| `Termination` | Final `TerminationStatus`. |
| `Output` | Final output (`string` or `IReadOnlyTape` depending on the overload). |
| `Steps` | Number of steps executed. |
| `Trace` | Step-by-step trace lines (populated when `verbose: true`). |
| `AppliedInstructions` | Numbers of the instructions applied on each step, when available. |

## Built-in algorithms

`PrettyMachines.Implementations` ships ready-made machines. Both `TuringMachines` and `MarkovAlgorithms`
expose static factories with a common naming scheme.

| Concept | Turing machine | Markov algorithm |
| --- | --- | --- |
| Brackets grammar | `Create_BracketsGrammar` | `Create_BracketsGrammar` |
| Binary increment | `Create_BinaryIncrementMachine` | `Create_BinaryIncrement` |
| Binary decrement | `Create_BinaryDecrementMachine` | `Create_BinaryDecrement` |
| Binary addition | `Create_BinaryAdditionMachine` | `Create_BinaryAddition` |
| Binary subtraction | `Create_BinarySubtractionMachine` | `Create_BinarySubtraction` |
| String concatenation | `Create_StringConcatenationMachine` | `Create_StringConcatenation` |
| String reversal | `Create_StringReversalMachine` | `Create_StringReversal` |
| Busy beaver | `Create_BusyBeaver` | `Create_BusyBeaver` |
| Binary → unary | `Create_BinaryToUnaryConverterMachine` | `Create_BinaryToUnaryConverter` |
| Unary → binary | `Create_UnaryToBinaryConverterMachine` | `Create_UnaryToBinaryConverter` |
| Unary → ternary | `Create_UnaryToTernaryConverterMachine` | `Create_UnaryToTernaryConverter` |
| Decimal → binary | `Create_DecimalToBinaryConverterMachine` | `Create_DecimalToBinaryConverter` |
| Binary → decimal | `Create_BinaryToDecimalConverterMachine` | `Create_BinaryToDecimalConverter` |
| Leading zeros trim | — | `Create_LeadingZerosTrim` |

Operand-based algorithms read input in the form `A+B` / `A-B`; the brackets grammar accepts a string and
outputs the accepted or rejected symbol.

```csharp
using PrettyMachines.Implementations;

var adder = TuringMachines.Create_BinaryAdditionMachine();
var sum = adder.Execute("101+11", new AlgorithmCancellation(100_000)).Output; // "1000"
```

## Printing and parsing

`PrettyMachines.Algorithms.Utils` provides text/CSV output and a rule parser.

- `InstructionTablePrinter.PrintTable(machine)` / `PrintList` / `PrintCsv` — render a Turing machine and
  its instruction table.
- `MarkovAlgorithmPrinter.PrintFormatted(algorithm)` / `PrintCsv` — render Markov rules with comments.
- `MachineTapePrinter.Print(tape)` — render the non-blank tape cells.
- `MarkovSubstitutionParser.ParseQuoted("'a' -> 'b'")` and `ParseUnquoted("a=>b")` — create
  `Substitution` rules; `=>` marks a terminal rule.

Each printer overload accepts a `StringBuilder`, a `Stream`, or returns a `string`.
