using System;


namespace PrettyMachines.CodeGen;

[AttributeUsage(AttributeTargets.Method)]
public sealed class AlgorithmImplAttribute(string text) : Attribute
{
    public string Text { get; } = text;
}