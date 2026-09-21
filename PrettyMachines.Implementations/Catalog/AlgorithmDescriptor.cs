using System.Reflection;
using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Markov;
using PrettyMachines.Algorithms.Turing;


namespace PrettyMachines.Implementations.Catalog;

/// <summary>
/// Describes a single built-in algorithm that can be created by a static factory method.
/// </summary>
public sealed class AlgorithmDescriptor
{
    /// <summary>Initializes a new descriptor over a public static factory method.</summary>
    /// <param name="id">Stable identifier equal to the factory method name.</param>
    /// <param name="name">Human-readable display name.</param>
    /// <param name="familyId">Identifier of the declaring family (the static class name).</param>
    /// <param name="returnType">Declared return type of the factory method.</param>
    /// <param name="factory">Factory method used to create the algorithm.</param>
    public AlgorithmDescriptor(string id, string name, string familyId, Type returnType, MethodInfo factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(returnType);
        ArgumentNullException.ThrowIfNull(factory);

        Id = id;
        Name = name;
        FamilyId = familyId;
        ReturnType = returnType;
        Factory = factory;
    }

    /// <summary>Gets the stable identifier equal to the factory method name (for example <c>Create_BinaryIncrementMachine</c>).</summary>
    public string Id { get; }

    /// <summary>Gets the human-readable display name (for example <c>Binary increment</c>).</summary>
    public string Name { get; }

    /// <summary>Gets the identifier of the declaring family (for example <c>TuringMachines</c>).</summary>
    public string FamilyId { get; }

    /// <summary>Gets the declared return type of the factory method.</summary>
    public Type ReturnType { get; }

    /// <summary>Gets the factory method used to create the algorithm.</summary>
    public MethodInfo Factory { get; }

    /// <summary>Indicates whether this algorithm is a Turing machine.</summary>
    public bool IsTuring => typeof(TuringMachine).IsAssignableFrom(ReturnType);

    /// <summary>Indicates whether this algorithm is a Markov algorithm.</summary>
    public bool IsMarkov => typeof(MarkovAlgorithm).IsAssignableFrom(ReturnType);

    /// <summary>Creates the algorithm using the factory's default optional parameters.</summary>
    /// <returns>A new algorithm instance.</returns>
    public IAlgorithm Create() => AlgorithmCatalog.Create(this);

    /// <inheritdoc/>
    public override string ToString() => $"{FamilyId}.{Id}";
}
