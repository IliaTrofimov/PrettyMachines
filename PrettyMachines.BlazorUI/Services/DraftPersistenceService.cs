using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using PrettyMachines.BlazorUI.Models;


namespace PrettyMachines.BlazorUI.Services;

/// <summary>Restores custom drafts from the browser's local storage and saves them automatically after every change.</summary>
public sealed class DraftPersistenceService : IDisposable
{
    private static readonly JsonSerializerOptions StorageOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
    };

    private readonly AlgorithmDraftStore store;
    private readonly IJSRuntime js;

    private bool initialized;
    private bool restoring;
    private bool saving;
    private bool savePending;


    /// <summary>Creates a persistence service over the given draft store.</summary>
    /// <param name="store">In-memory draft store.</param>
    /// <param name="js">JavaScript runtime used to reach local storage.</param>
    public DraftPersistenceService(AlgorithmDraftStore store, IJSRuntime js)
    {
        this.store = store;
        this.js = js;
        store.Changed += OnStoreChanged;
    }


    /// <summary>Restores custom drafts from local storage. Safe to call more than once.</summary>
    public async Task InitializeAsync()
    {
        if (initialized)
            return;

        initialized = true;
        var entries = await js.InvokeAsync<List<StoredDraftEntry>>("loadDraftsFromLocalStoage");

        restoring = true;
        try
        {
            foreach (var entry in entries)
            {
                if (entry.DraftObj.ValueKind != JsonValueKind.Object)
                    continue;

                StoredDraft? stored;
                try
                {
                    stored = entry.DraftObj.Deserialize<StoredDraft>(StorageOptions);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (stored?.ToDraft() is not { } draft || store.Get(draft.Id) is not null)
                    continue;

                store.Add(draft);
            }
        }
        finally
        {
            restoring = false;
        }
    }

    /// <summary>Removes a custom draft from memory and local storage.</summary>
    /// <param name="draft">Draft to remove.</param>
    public async Task DeleteAsync(AlgorithmDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        await js.InvokeVoidAsync("deleteDraftFromLocalStoage", draft.FamilyId, draft.Id);
        store.Remove(draft.Id);
    }


    private void OnStoreChanged()
    {
        if (!initialized || restoring)
            return;

        if (saving)
        {
            savePending = true;
            return;
        }

        _ = SaveAsync();
    }

    private async Task SaveAsync()
    {
        saving = true;
        try
        {
            do
            {
                savePending = false;
                foreach (var draft in store.Drafts)
                    await js.InvokeVoidAsync("saveDraftToLocalStoage", draft.FamilyId, draft.Id, StoredDraft.From(draft));
            }
            while (savePending);
        }
        finally
        {
            saving = false;
        }
    }


    /// <inheritdoc/>
    public void Dispose() => store.Changed -= OnStoreChanged;
}
