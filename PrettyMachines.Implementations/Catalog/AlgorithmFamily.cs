namespace PrettyMachines.Implementations.Catalog;

/// <summary>
/// Describes a family of built-in algorithms declared by a single static factory class.
/// </summary>
public sealed class AlgorithmFamily
{
    /// <summary>Initializes a new algorithm family.</summary>
    /// <param name="id">Family identifier equal to the declaring static class name.</param>
    /// <param name="name">Human-readable family name.</param>
    /// <param name="algorithms">Algorithms that belong to this family, ordered by name.</param>
    public AlgorithmFamily(string id, string name, IReadOnlyList<AlgorithmDescriptor> algorithms)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(algorithms);

        Id = id;
        Name = name;
        Algorithms = algorithms;
    }

    /// <summary>Gets the family identifier equal to the declaring static class name (for example <c>TuringMachines</c>).</summary>
    public string Id { get; }

    /// <summary>Gets the human-readable family name (for example <c>Turing machines</c>).</summary>
    public string Name { get; }

    /// <summary>Gets the algorithms that belong to this family.</summary>
    public IReadOnlyList<AlgorithmDescriptor> Algorithms { get; }

    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({Algorithms.Count})";
}
