namespace PrettyMachines.BlazorUI.Models;

/// <summary>A dropdown-only option whose value cannot be produced by typing free text.</summary>
public sealed record ComboOption
{
    /// <summary>Gets the non-typeable sentinel key identifying this option.</summary>
    public required string Key { get; init; }

    /// <summary>Gets the label shown in the input and the dropdown.</summary>
    public required string Label { get; init; }

    /// <summary>Gets an optional longer description shown under the label in the dropdown.</summary>
    public string? Description { get; init; }
}


/// <summary>Editable combo box value: either free text or one special dropdown option.</summary>
public readonly record struct ComboValue(string? Text, string? SpecialKey)
{
    /// <summary>Gets a value indicating whether a special option (not typeable text) is selected.</summary>
    public bool IsSpecial => SpecialKey is not null;

    /// <summary>Creates a value holding free text.</summary>
    public static ComboValue FromText(string? text) => new(text, null);

    /// <summary>Creates a value holding a special option key.</summary>
    public static ComboValue FromSpecial(string key) => new(null, key);

    /// <inheritdoc />
    public override string ToString() => IsSpecial ? SpecialKey! : Text ?? string.Empty;
}
