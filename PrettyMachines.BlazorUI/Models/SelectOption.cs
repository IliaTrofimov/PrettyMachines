namespace PrettyMachines.BlazorUI.Models;

/// <summary>An option shown by <see cref="Controls.SelectDropdown{TValue}"/>.</summary>
public sealed record SelectOption<TValue>
{
    /// <summary>Gets the value applied to the bound field when this option is picked.</summary>
    public required TValue Value { get; init; }

    /// <summary>Gets the compact text shown on the collapsed trigger.</summary>
    public required string ShortText { get; init; }

    /// <summary>Gets the full text shown for this option in the expanded menu.</summary>
    public required string FullText { get; init; }
}
