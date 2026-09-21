namespace PrettyMachines.BlazorUI.Models.Drafts;

/// <summary>Mutable, editable definition of a normal Markov algorithm.</summary>
public sealed class MarkovAlgorithmDraft
{
    /// <summary>Gets or sets the algorithm name.</summary>
    public string Name { get; set; } = "New Markov algorithm";

    /// <summary>Gets the editable alphabet characters.</summary>
    public List<char> Alphabet { get; } = [];

    /// <summary>Gets the editable marker characters.</summary>
    public List<char> Markers { get; } = [];

    /// <summary>Gets the substitution rules in order.</summary>
    public List<DraftRule> Rules { get; } = [];
}


/// <summary>Editable Markov substitution rule.</summary>
public sealed class DraftRule
{
    /// <summary>Gets or sets the pattern to search for.</summary>
    public string Pattern { get; set; } = "";

    /// <summary>Gets or sets the replacement text.</summary>
    public string Replacement { get; set; } = "";

    /// <summary>Gets or sets a value indicating whether the algorithm stops after applying this rule.</summary>
    public bool IsTerminal { get; set; }

    /// <summary>Gets or sets an optional comment.</summary>
    public string? Comment { get; set; }
}
