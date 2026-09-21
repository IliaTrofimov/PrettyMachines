using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Markov;
using PrettyMachines.Algorithms.Turing;
using PrettyMachines.BlazorUI.Models;
using PrettyMachines.BlazorUI.Models.Drafts;
using PrettyMachines.Implementations;
using PrettyMachines.Implementations.Catalog;


namespace PrettyMachines.BlazorUI.Services.DraftAdapters;

/// <summary>Creates and builds drafts for the algorithm editors.</summary>
public static class DraftAlgorithmFactory
{
    /// <summary>Builds an executable algorithm from a draft.</summary>
    /// <param name="draft">Draft to build.</param>
    /// <returns>An immutable algorithm instance.</returns>
    /// <exception cref="InvalidOperationException">The draft is not valid.</exception>
    public static IAlgorithm Build(AlgorithmDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        return draft.Definition switch
        {
            TuringMachineDraft turing => TuringDraftAdapter.ToAlgorithm(turing),
            MarkovAlgorithmDraft markov => MarkovDraftAdapter.ToAlgorithm(markov),
            _ => throw new InvalidOperationException($"Unsupported draft definition '{draft.Definition.GetType().Name}'.")
        };
    }

    /// <summary>Creates a new blank draft for the given family.</summary>
    /// <param name="familyId">Family identifier (for example <c>TuringMachines</c>).</param>
    /// <returns>A new draft with sensible starter content.</returns>
    /// <exception cref="InvalidOperationException">The family is not supported.</exception>
    public static AlgorithmDraft CreateBlank(string familyId)
    {
        return familyId switch
        {
            nameof(TuringMachines) => new AlgorithmDraft
            {
                FamilyId = familyId,
                Name = "New Turing machine",
                Definition = CreateBlankTuring(),
            },
            nameof(MarkovAlgorithms) => new AlgorithmDraft
            {
                FamilyId = familyId,
                Name = "New Markov algorithm",
                Definition = CreateBlankMarkov(),
            },
            _ => throw new InvalidOperationException($"Family '{familyId}' does not support drafting yet."),
        };
    }

    /// <summary>Creates a draft that is a copy of a built-in algorithm.</summary>
    /// <param name="descriptor">Descriptor of the built-in algorithm.</param>
    /// <returns>A new draft that mirrors the built-in algorithm.</returns>
    /// <exception cref="InvalidOperationException">The built-in algorithm is not editable.</exception>
    public static AlgorithmDraft Fork(AlgorithmDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var algorithm = descriptor.Create();
        var name = algorithm.Name ?? descriptor.Name;

        var definition = algorithm switch
        {
            TuringMachine turing => (object)TuringDraftAdapter.From(turing),
            MarkovAlgorithm markov => MarkovDraftAdapter.From(markov),
            _ => throw new InvalidOperationException($"Algorithm '{descriptor.Id}' does not support drafting yet."),
        };

        if (definition is TuringMachineDraft turingDraft)
            turingDraft.Name = name;
        else if (definition is MarkovAlgorithmDraft markovDraft)
            markovDraft.Name = name;

        return new AlgorithmDraft
        {
            FamilyId = descriptor.FamilyId,
            Name = name,
            SourceDescriptorId = descriptor.Id,
            Definition = definition,
        };
    }


    private static TuringMachineDraft CreateBlankTuring()
    {
        var draft = new TuringMachineDraft { Name = "New Turing machine" };
        draft.Alphabet.AddRange(["0", "1"]);
        draft.States.Add(new DraftState { Name = "Start", IsInitial = true });
        draft.States.Add(new DraftState { Name = "Done", IsTerminal = true });
        draft.Transitions.Add(new DraftTransition
        {
            StateIndex = 0,
            SymbolMatch = SymbolMatch.NotEmpty,
            NextStateIndex = 1,
            Movement = TapeMovement.None,
        });
        return draft;
    }

    private static MarkovAlgorithmDraft CreateBlankMarkov()
    {
        var draft = new MarkovAlgorithmDraft { Name = "New Markov algorithm" };
        draft.Rules.Add(new DraftRule { Pattern = "", Replacement = "", IsTerminal = true, Comment = "Start here" });
        return draft;
    }
}
