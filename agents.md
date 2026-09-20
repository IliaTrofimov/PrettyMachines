# AGENTS.md

PrettyMachines is a .NET 10 library that implements different formal algorithms like Turing machine, Markov algorithm etc. Target `net10.0`, nullable enabled, implicit usings. Unit tests use xUnit library.
Human-facing docs: see `readme.md`.

## Layout

- `PrettyMachines.Algorithms` - core project
   - `Markov` - definitions, builder class for Markov algorithms
   - `Turing` - definitions, builder class for Turing machines
   - `Utils` - common functions that can be used by all other algorithms
   - `Abstract` - core concepts for all algorithms

## Core concepts

### Formal algorithm
Algorithm is any well-defined set of instructions that when followed terminates after a finite number of steps that comprise a solution to a given computational problem. This project represents only algorithms as computable or effectively calculable functions. Formally, a method is called effective to a specific class of problems when it satisfies the following criteria:
  - It consists of a finite number of exact, finite instructions.
  - When it is applied to a problem from its class:
     - It always finishes (terminates) after a finite number of steps.
     - It always produces a correct answer.
  - In principle, it can be done by a human without any aids except writing materials.
  - Its instructions need only to be followed rigorously to succeed. In other words, it requires no ingenuity to succeed.

Optionally, it may also be required that the method never returns a result as if it were an answer when the method is applied to a problem from outside its class. Adding this requirement reduces the set of classes for which there is an effective method.

### Algorithm definition
This project allows users to define their own algorithms and execute them with c# code. Main way to create an algorithm is using a builder class. Created algorithm instance must be immutable so user cannot change its steps during execution. Algorithms can be serialized and deserialized from text files.

All algorithms must inherit base abstract algorithm class.


## Status (WIP)

- [x] Turing machine
  - [x] Infinite machine tape simulation;
  - [x] Builder class;
  - [x] Unit tests;
  - [ ] Example algorithms (need more);
- [x] Markov algorithm
  - [x] Builder class;
  - [x] Unit tests
  - [ ] Example algorithms (need more)
- [ ] Finite state machines
  - [ ] Builder class;
  - [ ] Unit tests;
  - [ ] Examples;
- [ ] Base algorithm class requires some refinements;
- [ ] BlazorUI WebASM application for building and executing all algorithms from this projects;

Do not invent new public API without aligning with `readme.md` architecture.
Prefer extending existing interfaces over new parallel abstractions.

## Conventions

- Public API: XML doc comments (Russian on existing interfaces - stay consistent).
- Async: `Task` + `CancellationToken ct = default` on public methods.
- Interfaces for extensibility; `sealed` on concrete classes when appropriate.
- Minimal diffs - match existing namespace and file layout.
- No unrelated refactors in feature PRs.
- Use "=>" methods only when expression is short and 1-2 lines long.
- Separate constructors from methods, properties and fields with 2 lines. 

## For agents

**Do**
- Read `readme.md` before changing architecture.
- Implement executor/runner logic in `Runners/` using existing step interfaces.
- Use `Debug.WriteLine` or `#if DEBUG` to print brief information about program's state.
- Use `Activity` to create Jaeger traces for steps and important parts of the program.
- Add tests when introducing behavior (xUnit, once test project exists).
- Use `TheoryAttribute` for unit tests if possible to combine simillar test cases.
- Use `ITestOutputHelper` for unit tests to print information for complex cases.

**Don’t**
- Commit `bin/`, `obj/`, `.idea/` (see `.gitignore`).
- Change public interface shapes without updating README and all implementations.
- Add heavy dependencies inside `PrettyEngine.Core/` without discussion.