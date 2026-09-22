using System.Diagnostics;


namespace PrettyMachines.Utils;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Property)]
[DebuggerDisplay("AlgorithmName = {Name}")]
public sealed class AlgorithmNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}