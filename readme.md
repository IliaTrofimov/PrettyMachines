# PrettyMachines

[![Tests](https://github.com/IliaTrofimov/PrettyMachines/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/IliaTrofimov/PrettyMachines/actions/workflows/ci.yml)

A .NET 10 library for defining, executing and printing formal algorithms like Turing machines and
normal Markov algorithm.

The project represents algorithms as computable (effectively calculable) functions: a finite set of exact
instructions that always terminates and always produces the expected answer for the class of problems it
was built for. Algorithms are defined with fluent builders, executed step by step, and can be rendered as
text/CSV.

You can try this library with this [demo application](https://iliatrofimov.github.io/PrettyMachines/).

## Table of contents

- [Features](#features)
- [Requirements](#requirements)
- [Solution layout](#solution-layout)
- [Building and testing](#building-and-testing)
- [Core concepts](#core-concepts)
- [Quick start: Turing machine](#quick-start-turing-machine)
- [Quick start: Markov algorithm](#quick-start-markov-algorithm)
- [Execution results](#execution-results)
- [Built-in algorithms](#built-in-algorithms)
- [Printing and parsing](#printing-and-parsing)

## Features

- Immutable algorithm definitions built with fluent builders.
- Lazy execution: `Run` yields one immutable snapshot per step; `Execute` consumes the sequence.
- Snapshots expose the step number, termination status, typed output and an optional trace line.
- Bounded execution via step limits and `CancellationToken`.
- Infinite Turing machine tape simulation with left/right/none head movement.
- Symbol matching by exact value, empty/not-empty, or "any" cell.
- Text and CSV printers for machines, instruction tables and tapes.
- Parser for Markov substitution rules (`a -> b`, `a => b`, quoted and unquoted forms).
- Library of ready-to-use example algorithms.
- Blazor WebAssembly playground for running algorithms and inspecting their traces.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).
- Unit tests use xUnit and FluentAssertions.

## Solution layout

| Project | Description |
| --- | --- |
| `PrettyMachines.Algorithms` | Core library: abstract algorithm model, `Turing`, `Markov` and `Utils` (printing/parsing). |
| `PrettyMachines.Implementations` | Ready-to-use algorithms built on the core library. |
| `PrettyMachines.BlazorUI` | Blazor WebAssembly app for building and executing algorithms. |
| `PrettyMachines.Tests` | xUnit tests for the core library and implementations. |

## Building and testing

```bash
dotnet build PrettyMachines.sln
dotnet test
```

Run the Blazor WebAssembly playground:

```bash
dotnet run --project PrettyMachines.BlazorUI
```

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

A [Turing machine](https://en.wikipedia.org/wiki/Turing_machine) is a mathematical model of computation describing an abstract machine that manipulates symbols on a strip of tape according to a table of rules.[ Despite the model's simplicity, it is capable of implementing any computer algorithm. Machine is defined by an alphabet (optionally strict), a blank symbol, a set of states with one initial state, and a transition table.

```csharp
using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Turing;
using PrettyMachines.Algorithms.Utils.Printing;

var machine = TuringMachine.Create("Toggle first bit")
    .WithAlphabet("0", "1")          // strict alphabet; unknown symbols fail the machine
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
using PrettyMachines.Algorithms.Turing;

rules.AddRule(q0, SymbolMatch.NotEmpty, q0, null, TapeMovement.Right)
     .AddHalt(q0, SymbolMatch.Empty, "1");   // AddHalt targets TuringMachineState.Halt
```

The same machine typed over the tape (no input mutation of the caller's tape):

```csharp
var tape = new MachineTape(new[] { "1", "0", "1" }, blankSymbol: "_");
AlgorithmResult<IReadOnlyTape> tapeResult = machine.Execute(tape, new AlgorithmCancellation(10_000));
```

## Quick start: Markov algorithm

A [Markov algorithm](https://en.wikipedia.org/wiki/Markov_algorithm) is a string rewriting system that uses grammar-like rules to operate on strings of symbols. Markov algorithms have been shown to be Turing-complete, which means that they are suitable as a general model of computation and can represent any mathematical expression from its simple notation. Markov algorithms are named after the Soviet mathematician Andrey Markov, Jr. Algorithm applies the first matching substitution rule, replacing the leftmost occurrence of its pattern. A rule marked terminal stops the algorithm after it is applied.

```csharp
using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Markov;

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
var sum = adder.Execute("101+11", new AlgorithmCancellation(100_000)).Output;   // "1000"
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
