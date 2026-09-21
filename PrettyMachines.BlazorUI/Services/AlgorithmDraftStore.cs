using PrettyMachines.BlazorUI.Models;


namespace PrettyMachines.BlazorUI.Services;

/// <summary>In-memory store of authored algorithm drafts. Contents are lost when the page is refreshed.</summary>
public sealed class AlgorithmDraftStore
{
    private readonly List<AlgorithmDraft> drafts = [];


    /// <summary>Raised after the set of drafts changes.</summary>
    public event Action? Changed;

    /// <summary>Gets the drafts ordered by name.</summary>
    public IReadOnlyList<AlgorithmDraft> Drafts =>
        drafts.OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase).ThenBy(d => d.Id, StringComparer.Ordinal).ToList();


    /// <summary>Finds a draft by identifier.</summary>
    /// <param name="id">Draft identifier or route value.</param>
    /// <returns>The matching draft, or <c>null</c>.</returns>
    public AlgorithmDraft? Get(string? id)
    {
        if (string.IsNullOrEmpty(id))
            return null;
        return drafts.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.Ordinal));
    }

    /// <summary>Adds a new draft to the store.</summary>
    /// <param name="draft">Draft to add.</param>
    public void Add(AlgorithmDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        if (drafts.Any(d => string.Equals(d.Id, draft.Id, StringComparison.Ordinal)))
            return;

        drafts.Add(draft);
        Changed?.Invoke();
    }

    /// <summary>Removes a draft from the store.</summary>
    /// <param name="id">Identifier of the draft to remove.</param>
    public void Remove(string? id)
    {
        if (Get(id) is not { } draft)
            return;

        drafts.Remove(draft);
        Changed?.Invoke();
    }

    /// <summary>Notifies subscribers that a draft's contents changed.</summary>
    public void NotifyChanged() => Changed?.Invoke();
}
