using System.Text.Json;
using PrettyMachines.BlazorUI.Models.Drafts;


namespace PrettyMachines.BlazorUI.Models;

/// <summary>Serializable payload of a custom draft as stored in the browser's local storage.</summary>
public sealed class StoredDraft
{
    /// <summary>Gets or sets the draft identifier.</summary>
    public string Id { get; set; } = "";

    /// <summary>Gets or sets the identifier of the family this draft belongs to.</summary>
    public string FamilyId { get; set; } = "";

    /// <summary>Gets or sets the display name of the draft.</summary>
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the identifier of the built-in descriptor this draft was forked from, if any.</summary>
    public string? SourceDescriptorId { get; set; }

    /// <summary>Gets or sets the Turing machine definition when this draft is a Turing machine.</summary>
    public TuringMachineDraft? Turing { get; set; }

    /// <summary>Gets or sets the Markov algorithm definition when this draft is a Markov algorithm.</summary>
    public MarkovAlgorithmDraft? Markov { get; set; }


    /// <summary>Creates a storage payload from an in-memory draft.</summary>
    /// <param name="draft">Draft to store.</param>
    /// <returns>A serializable payload.</returns>
    public static StoredDraft From(AlgorithmDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        return new StoredDraft
        {
            Id = draft.Id,
            FamilyId = draft.FamilyId,
            Name = draft.Name,
            SourceDescriptorId = draft.SourceDescriptorId,
            Turing = draft.Definition as TuringMachineDraft,
            Markov = draft.Definition as MarkovAlgorithmDraft,
        };
    }

    /// <summary>Converts the payload back into an in-memory draft.</summary>
    /// <returns>The restored draft, or <c>null</c> when the definition or identifiers are missing.</returns>
    public AlgorithmDraft? ToDraft()
    {
        object? definition = Turing ?? (object?)Markov;
        if (definition is null || string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(FamilyId))
            return null;

        return new AlgorithmDraft
        {
            Id = Id,
            FamilyId = FamilyId,
            Name = string.IsNullOrWhiteSpace(Name) ? "Draft" : Name,
            SourceDescriptorId = SourceDescriptorId,
            Definition = definition,
        };
    }
}


/// <summary>Entry returned by the local-storage JavaScript loader.</summary>
public sealed class StoredDraftEntry
{
    /// <summary>Gets or sets the family identifier encoded in the storage key.</summary>
    public string AlgFamilyId { get; set; } = "";

    /// <summary>Gets or sets the draft identifier encoded in the storage key.</summary>
    public string DraftId { get; set; } = "";

    /// <summary>Gets or sets the raw stored draft payload.</summary>
    public JsonElement DraftObj { get; set; }
}
