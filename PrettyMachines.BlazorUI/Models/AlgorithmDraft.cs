using PrettyMachines.BlazorUI.Models.Drafts;


namespace PrettyMachines.BlazorUI.Models;

/// <summary>In-memory description of an authored algorithm together with its editable definition.</summary>
public sealed class AlgorithmDraft
{
    /// <summary>Gets the unique draft identifier used in routes.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Gets the identifier of the family this draft belongs to.</summary>
    public required string FamilyId { get; init; }

    /// <summary>Gets or sets the display name of the draft.</summary>
    public required string Name { get; set; }

    /// <summary>Gets the identifier of the built-in descriptor this draft was forked from, if any.</summary>
    public string? SourceDescriptorId { get; init; }

    /// <summary>Gets the editable definition (<see cref="TuringMachineDraft"/> or <see cref="MarkovAlgorithmDraft"/>).</summary>
    public required object Definition { get; init; }


    /// <summary>Gets the Turing machine definition when this draft is a Turing machine.</summary>
    public TuringMachineDraft? Turing => Definition as TuringMachineDraft;

    /// <summary>Gets the Markov algorithm definition when this draft is a Markov algorithm.</summary>
    public MarkovAlgorithmDraft? Markov => Definition as MarkovAlgorithmDraft;
}
