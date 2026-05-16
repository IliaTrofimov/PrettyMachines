using System.Collections.Generic;


namespace PrettyMachines.CodeGen;

internal sealed class MethodInfo
{
    public string MethodName { get; set; } = "";
    public string ReturnType { get; set; } = "";
    public List<ParameterInfo> Parameters { get; set; } = new();
    public string Modifiers { get; set; } = "";
    public string ContainingClass { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string TemplateText { get; set; } = "";
}