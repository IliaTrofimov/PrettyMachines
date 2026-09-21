using PrettyMachines.Algorithms.Turing;
using PrettyMachines.BlazorUI.Models.Drafts;


namespace PrettyMachines.BlazorUI.Services.DraftAdapters;

/// <summary>Converts between <see cref="TuringMachine"/> instances and editable <see cref="TuringMachineDraft"/> definitions.</summary>
public static class TuringDraftAdapter
{
    /// <summary>Creates an editable draft from an existing machine.</summary>
    /// <param name="machine">Machine to read.</param>
    /// <returns>A new draft that mirrors the machine.</returns>
    public static TuringMachineDraft From(TuringMachine machine)
    {
        ArgumentNullException.ThrowIfNull(machine);

        var instructions = machine.Instructions;
        var blank = instructions.BlankSymbol;

        var draft = new TuringMachineDraft
        {
            Name = machine.Name ?? "Turing machine",
            HasBlankSymbol = blank is not null,
            BlankSymbol = blank ?? "",
            StrictAlphabet = machine.HasStrictAlphabet,
        };

        foreach (var symbol in instructions.Alphabet
                     .Where(s => s is not null && !string.Equals(s, blank, StringComparison.Ordinal))
                     .Select(s => s!)
                     .Distinct())
        {
            draft.Alphabet.Add(symbol);
        }

        var states = instructions.States
            .Where(state => state.Id != TuringMachineState.Halt.Id)
            .OrderBy(state => state.Id)
            .ToList();

        var indexById = new Dictionary<int, int>();
        for (var i = 0; i < states.Count; i++)
        {
            indexById[states[i].Id] = i;
            draft.States.Add(new DraftState
            {
                Name = states[i].Name ?? "",
                IsTerminal = states[i].IsTerminal,
            });
        }

        if (indexById.TryGetValue(machine.InitialState.Id, out var initialIndex))
            draft.States[initialIndex].IsInitial = true;

        foreach (var instruction in instructions)
        {
            draft.Transitions.Add(new DraftTransition
            {
                StateIndex = indexById.GetValueOrDefault(instruction.InitialState.Id, -1),
                SymbolMatch = instruction.ScannedSymbol.Match,
                SymbolValue = instruction.ScannedSymbol.Value ?? "",
                NextStateIndex = instruction.NextState.Id == TuringMachineState.Halt.Id
                    ? -1
                    : indexById.GetValueOrDefault(instruction.NextState.Id, -1),
                Prints = instruction.PrintedSymbol is not null,
                Print = instruction.PrintedSymbol ?? "",
                Movement = instruction.Movement,
            });
        }

        return draft;
    }

    /// <summary>Builds a machine from an editable draft.</summary>
    /// <param name="draft">Draft to build.</param>
    /// <returns>A new immutable machine instance.</returns>
    /// <exception cref="InvalidOperationException">The draft is not a valid Turing machine.</exception>
    public static TuringMachine ToAlgorithm(TuringMachineDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        Validate(draft);

        var builder = TuringMachine.Create(string.IsNullOrWhiteSpace(draft.Name) ? null : draft.Name.Trim());
        if (draft.StrictAlphabet)
            builder = builder.WithAlphabet(draft.Alphabet);
        builder = builder.WithBlankSymbol(draft.HasBlankSymbol ? draft.BlankSymbol : null);

        var states = new List<TuringMachineState>(draft.States.Count);
        foreach (var state in draft.States)
        {
            var name = string.IsNullOrWhiteSpace(state.Name) ? null : state.Name.Trim();
            builder = builder.AddState(name, state.IsInitial, state.IsTerminal, out var created);
            states.Add(created);
        }

        try
        {
            return builder.BuildRules(rules =>
            {
                foreach (var transition in draft.Transitions)
                {
                    var from = states[transition.StateIndex];
                    var next = transition.NextStateIndex >= 0 && transition.NextStateIndex < states.Count
                        ? states[transition.NextStateIndex]
                        : TuringMachineState.Halt;
                    var print = transition.Prints ? transition.Print : null;

                    if (transition.SymbolMatch == SymbolMatch.Exact)
                        rules.AddRule(from, transition.SymbolValue, next, print, transition.Movement);
                    else
                        rules.AddRule(from, transition.SymbolMatch, next, print, transition.Movement);
                }
            });
        }
        catch (InvalidOperationException exception) when (exception.InnerException is not null)
        {
            throw new InvalidOperationException(exception.InnerException.Message, exception);
        }
    }

    /// <summary>Verifies that a draft can be built into a machine.</summary>
    /// <param name="draft">Draft to validate.</param>
    /// <exception cref="InvalidOperationException">The draft is not a valid Turing machine.</exception>
    public static void Validate(TuringMachineDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (draft.States.Count == 0)
            throw new InvalidOperationException("A Turing machine must declare at least one state.");
        if (draft.Transitions.Count == 0)
            throw new InvalidOperationException("A Turing machine must declare at least one transition rule.");
        if (draft.StrictAlphabet && draft.Alphabet.Count == 0)
            throw new InvalidOperationException("A machine with a strict alphabet must declare at least one symbol.");

        var initialCount = draft.States.Count(state => state.IsInitial);
        if (initialCount != 1)
            throw new InvalidOperationException($"A Turing machine must have exactly one initial state (found {initialCount}).");

        foreach (var state in draft.States)
        {
            if (state.IsInitial && state.IsTerminal)
                throw new InvalidOperationException("A state cannot be both initial and terminal.");
        }

        var duplicateName = draft.States
            .Where(state => !string.IsNullOrWhiteSpace(state.Name))
            .GroupBy(state => state.Name.Trim(), StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateName is not null)
            throw new InvalidOperationException($"State names must be unique; '{duplicateName.Key}' is used more than once.");

        foreach (var transition in draft.Transitions)
        {
            if (transition.StateIndex < 0 || transition.StateIndex >= draft.States.Count)
                throw new InvalidOperationException("A transition references an unknown source state.");
            if (transition.NextStateIndex >= draft.States.Count)
                throw new InvalidOperationException("A transition references an unknown destination state.");
            if (draft.States[transition.StateIndex].IsTerminal)
                throw new InvalidOperationException(
                    $"A transition starts from the terminal state '{draft.States[transition.StateIndex].Name}', which is not allowed.");
        }
    }
}
