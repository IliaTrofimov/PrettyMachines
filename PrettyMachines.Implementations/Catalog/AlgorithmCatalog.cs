using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using PrettyMachines.Abstract;


namespace PrettyMachines.Implementations.Catalog;

/// <summary>
/// Discovers the built-in algorithms declared by the static factory classes of this assembly.
/// </summary>
public static partial class AlgorithmCatalog
{
    private static List<AlgorithmFamily>? Cached = null;

    /// <summary>
    /// Reflects the <see cref="PrettyMachines.Implementations"/> assembly and returns all built-in algorithms
    /// grouped by their declaring static factory class.
    /// </summary>
    /// <returns>Families ordered by display name; algorithms ordered by display name.</returns>
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(TuringMachines))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(MarkovAlgorithms))]
    public static IReadOnlyList<AlgorithmFamily> Discover(bool forceReload = false)
    {
        if (!forceReload && Cached != null)
            return Cached;

        var assembly = typeof(TuringMachines).Assembly;

        var descriptors = assembly
            .GetTypes()
            .Where(type => type.IsClass && type.IsAbstract && type.IsSealed && type.IsPublic && !type.IsGenericTypeDefinition)
            .Where(type => !string.Equals(type.Namespace, typeof(AlgorithmCatalog).Namespace, StringComparison.Ordinal))
            .SelectMany(DiscoverFamily)
            .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(d => d.Id, StringComparer.Ordinal);

        Cached = descriptors
            .GroupBy(d => d.FamilyId)
            .Select(group => new AlgorithmFamily(
                group.Key,
                group.OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(d => d.Id, StringComparer.Ordinal)
                     .ToList()
                )
            )
            .OrderBy(f => f.Id, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.Id, StringComparer.Ordinal)
            .ToList();

        return Cached;
    }

    /// <summary>Finds a descriptor by family and algorithm identifiers.</summary>
    /// <param name="familyId">Family identifier (declaring type name).</param>
    /// <param name="algorithmId">Algorithm identifier (factory method name).</param>
    /// <returns>The matching descriptor, or <c>null</c> when it does not exist.</returns>
    public static AlgorithmDescriptor? Find(string? familyId, string? algorithmId, bool forceReload = false)
    {
        if (string.IsNullOrEmpty(familyId) || string.IsNullOrEmpty(algorithmId))
            return null;

        return Discover(forceReload)
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
            { 
                throw new InvalidOperationException(
                    $"Factory '{descriptor.Id}' has a required parameter '{parameter.Name}' and cannot be invoked from the catalog."    
                );
            }

            arguments[i] = GetDefaultArgument(parameter);
        }

        var created = descriptor.Factory.Invoke(null, arguments);
        return created as IAlgorithm
            ?? throw new InvalidOperationException(
                $"Factory '{descriptor.Id}' returned '{created?.GetType().Name ?? "null"}' which is not an {nameof(IAlgorithm)}."
            );
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
        
            var attribute = method.GetCustomAttributes<AlgorithmBuilderAttribute>().FirstOrDefault();
            
            yield return new AlgorithmDescriptor(
                method.Name,
                attribute?.Name ?? FixMethodName(method.Name),
                type.Name,
                method.ReturnType,
                method,
                attribute?.ExampleInput
            );
        }
    }

    private static string FixMethodName(string methodName)
    {
        methodName = CreatePrefixRegex().Replace(methodName, "");
        methodName = MachineSuffixRegex().Replace(methodName, "");
        return methodName;
    }

    private static object? GetDefaultArgument(ParameterInfo parameter)
    {
        var value = parameter.DefaultValue;
        return value is DBNull or Missing ? Type.Missing : value;
    }

	[GeneratedRegex(@"^Create_")]
	private static partial Regex CreatePrefixRegex();

    [GeneratedRegex(@"Machine$")]
	private static partial Regex MachineSuffixRegex();
    
}
