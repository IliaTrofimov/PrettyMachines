namespace PrettyMachines.BlazorUI.Models;

public sealed class NavItem(string title, bool isExpanded = false)
{
    public string Title { get; } = title;
    public bool IsExpanded { get; set; } = isExpanded;
    public IReadOnlyCollection<string> SubItems { get; init; } = [];
}