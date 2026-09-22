namespace PrettyMachines.Implementations.Catalog;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class AlgorithmBuilderAttribute : Attribute
{
	public required string Name { get; init; }
	public string? Description { get; init; }
}