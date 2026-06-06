using System.Diagnostics;


namespace PrettyMachines.Algorithms.Utils;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Property)]
[DebuggerDisplay("AlgorithmName = {Name}")]
public sealed class AlgorithmNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}