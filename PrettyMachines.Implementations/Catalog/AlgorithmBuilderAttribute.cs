using PrettyMachines.Abstract;


namespace PrettyMachines.Implementations.Catalog;

/// <summary>
/// This attributes marks static functions that create instances of pre-made <see cref="IAlgorithm"/>s.
/// </summary>
/// <param name="name">Optional custom name of the algorithm.</param>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class AlgorithmBuilderAttribute(string? name = null) : Attribute
{
	/// <summary>
	/// Optional custom name of the algorithm.
	/// If not provided, then algorithm will be named with its <see cref="IAlgorithm.Name"/> 
	/// or factory method's name.
	/// </summary>
	public string? Name => name;

	/// <summary>
	/// Optional example input text for this algorithm.
	/// </summary>
	public string? ExampleInput { get; init; }
}