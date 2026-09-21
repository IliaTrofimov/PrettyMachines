using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PrettyMachines.Algorithms.Abstract;


namespace PrettyMachines.Implementations.Catalog;

/// <summary>
/// Discovers the built-in algorithms declared by the static factory classes of this assembly.
/// </summary>
public static class AlgorithmCatalog
{
    private const string FactoryPrefix = "Create_";
    private const string MachineSuffix = "Machine";


    /// <summary>
    /// Reflects the <see cref="PrettyMachines.Implementations"/> assembly and returns all built-in algorithms
    /// grouped by their declaring static factory class.
    /// </summary>
    /// <returns>Families ordered by display name; algorithms ordered by display name.</returns>
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(TuringMachines))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(MarkovAlgorithms))]
    public static IReadOnlyList<AlgorithmFamily> Discover()
    {
        var assembly = typeof(TuringMachines).Assembly;

        var descriptors = assembly
            .GetTypes()
            .Where(IsPublicStaticClass)
            .Where(type => !string.Equals(type.Namespace, typeof(AlgorithmCatalog).Namespace, StringComparison.Ordinal))
            .SelectMany(DiscoverFamily)
            .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(d => d.Id, StringComparer.Ordinal)
            .ToList();

        return descriptors
            .GroupBy(d => d.FamilyId)
            .Select(group => new AlgorithmFamily(
                group.Key,
                CleanFamilyName(group.Key),
                group.OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(d => d.Id, StringComparer.Ordinal)
                     .ToList()))
            .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.Id, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>Finds a descriptor by family and algorithm identifiers.</summary>
    /// <param name="familyId">Family identifier (declaring type name).</param>
    /// <param name="algorithmId">Algorithm identifier (factory method name).</param>
    /// <returns>The matching descriptor, or <c>null</c> when it does not exist.</returns>
    public static AlgorithmDescriptor? Find(string? familyId, string? algorithmId)
    {
        if (string.IsNullOrEmpty(familyId) || string.IsNullOrEmpty(algorithmId))
            return null;

        return Discover()
            .FirstOrDefault(f => string.Equals(f.Id, familyId, StringComparison.Ordinal))
            ?.Algorithms.FirstOrDefault(a => string.Equals(a.Id, algorithmId, StringComparison.Ordinal));
    }

    /// <summary>Creates an algorithm instance using the factory's default optional parameters.</summary>
    /// <param name="descriptor">Descriptor of the algorithm to create.</param>
    /// <returns>A newly created algorithm instance.</returns>
    /// <exception cref="InvalidOperationException">The factory has a required parameter or returned an unexpected type.</exception>
    public static IAlgorithm Create(AlgorithmDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var parameters = descriptor.Factory.GetParameters();
        var arguments = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (!parameter.IsOptional)
                throw new InvalidOperationException(
                    $"Factory '{descriptor.Id}' has a required parameter '{parameter.Name}' and cannot be invoked from the catalog.");

            arguments[i] = GetDefaultArgument(parameter);
        }

        var created = descriptor.Factory.Invoke(null, arguments);
        return created as IAlgorithm
            ?? throw new InvalidOperationException(
                $"Factory '{descriptor.Id}' returned '{created?.GetType().Name ?? "null"}' which is not an {nameof(IAlgorithm)}.");
    }

    /// <summary>Converts a factory method name into a human-readable display name.</summary>
    /// <param name="methodName">Factory method name (for example <c>Create_BinaryIncrementMachine</c>).</param>
    /// <returns>Display name (for example <c>Binary increment</c>).</returns>
    public static string CleanAlgorithmName(string methodName)
    {
        ArgumentNullException.ThrowIfNull(methodName);

        var name = methodName;
        if (name.StartsWith(FactoryPrefix, StringComparison.Ordinal))
            name = name[FactoryPrefix.Length..];
        if (name.EndsWith(MachineSuffix, StringComparison.Ordinal) && name.Length > MachineSuffix.Length)
            name = name[..^MachineSuffix.Length];

        var words = SplitPascalCase(name);
        if (words.Count == 0)
            return methodName;

        words[0] = NormalizeWord(words[0], upperFirst: true);
        for (var i = 1; i < words.Count; i++)
            words[i] = NormalizeWord(words[i], upperFirst: false);

        return string.Join(' ', words);
    }

    /// <summary>Converts a declaring type name into a human-readable family name.</summary>
    /// <param name="typeName">Declaring type name (for example <c>TuringMachines</c>).</param>
    /// <returns>Family name (for example <c>Turing machines</c>).</returns>
    public static string CleanFamilyName(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var words = SplitPascalCase(typeName);
        if (words.Count == 0)
            return typeName;

        for (var i = 1; i < words.Count; i++)
            words[i] = NormalizeWord(words[i], upperFirst: false);

        return string.Join(' ', words);
    }


    private static bool IsPublicStaticClass(Type type)
    {
        return type is { IsClass: true, IsAbstract: true, IsSealed: true, IsPublic: true, IsGenericTypeDefinition: false };
    }

    private static IEnumerable<AlgorithmDescriptor> DiscoverFamily(Type type)
    {
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        foreach (var method in methods)
        {
            if (method.IsSpecialName || method.IsGenericMethodDefinition)
                continue;
            if (!typeof(IAlgorithm).IsAssignableFrom(method.ReturnType))
                continue;

            yield return new AlgorithmDescriptor(
                method.Name,
                CleanAlgorithmName(method.Name),
                type.Name,
                method.ReturnType,
                method);
        }
    }

    private static object? GetDefaultArgument(ParameterInfo parameter)
    {
        var value = parameter.DefaultValue;
        return value is DBNull or Missing ? Type.Missing : value;
    }

    private static string NormalizeWord(string word, bool upperFirst)
    {
        if (word.Length == 0)
            return word;

        var first = upperFirst ? char.ToUpperInvariant(word[0]) : char.ToLowerInvariant(word[0]);
        return first + word[1..].ToLowerInvariant();
    }

    private static List<string> SplitPascalCase(string value)
    {
        var words = new List<string>();
        if (string.IsNullOrEmpty(value))
            return words;

        var start = 0;
        for (var i = 1; i < value.Length; i++)
        {
            var current = value[i];
            var previous = value[i - 1];
            var isBoundary = char.IsUpper(current) &&
                             (!char.IsUpper(previous) || (i + 1 < value.Length && char.IsLower(value[i + 1])));

            if (!isBoundary)
                continue;

            words.Add(value[start..i]);
            start = i;
        }

        words.Add(value[start..]);
        return words.Where(w => w.Length > 0).ToList();
    }
}
