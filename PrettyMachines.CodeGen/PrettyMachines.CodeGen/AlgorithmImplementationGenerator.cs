using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


namespace PrettyMachines.CodeGen;

/// <summary>
/// Source generator that builds
/// </summary>
[Generator]
public class MarkovAlgorithmImplementationGenerator : IIncrementalGenerator
{
    private static readonly string AttributeName = nameof(AlgorithmImplAttribute);
    private static readonly string AttributeNameShort = nameof(AlgorithmImplAttribute).Replace("Attribute", "");
    
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    { 
        var targetMethods = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsMethodWithAttribute, 
                transform: GetMethodInfo)
            .Where(method => method is not null);
        
        var compilationAndMethods = context.CompilationProvider.Combine(targetMethods.Collect()); 
        context.RegisterSourceOutput(compilationAndMethods, (ctx, source) => Execute(ctx, source.Left, source.Right));
    }

    private static bool IsMethodWithAttribute(SyntaxNode node, CancellationToken ct)
    {
        return node is MethodDeclarationSyntax method && method.AttributeLists.Any();
    }

    private static MethodInfo? GetMethodInfo(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var attribute = methodDeclaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .FirstOrDefault(attr =>
            {
                var name = attr.Name.ToString();
                return name == AttributeNameShort || name == AttributeName;
            });

        if (attribute == null)
            return null;
            
        var templateText = "";
        if (attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression is LiteralExpressionSyntax literal)
            templateText = literal.Token.ValueText;

        // Get method signature info
        var methodName = methodDeclaration.Identifier.Text;
        var returnType = methodDeclaration.ReturnType.ToString();
        var modifiers = string.Join(" ", methodDeclaration.Modifiers.Select(m => m.Text));
            
        // Get containing class and namespace
        var classDeclaration = methodDeclaration.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        var namespaceDeclaration = methodDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

        return new MethodInfo
        {
            MethodName = methodName,
            ReturnType = returnType,
            Parameters = [],
            Modifiers = modifiers,
            ContainingClass = classDeclaration?.Identifier.Text ?? "UnknownClass",
            Namespace = namespaceDeclaration?.Name.ToString() ?? "UnknownNamespace",
            TemplateText = templateText
        };
    }
    
    private static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<MethodInfo?> methods)
    {
        var groupedMethods = methods
            .Where(m => m is not null)
            .GroupBy(m => new { m!.Namespace, m.ContainingClass });

        foreach (var group in groupedMethods)
        {
            var sourceBuilder = new StringBuilder();
                
            sourceBuilder.AppendLine( "// Auto-generated source code");
            sourceBuilder.AppendLine($"namespace {group.Key.Namespace}");
            sourceBuilder.AppendLine( "{");
                
            // Generate partial class
            sourceBuilder.AppendLine($"    public partial class {group.Key.ContainingClass}");
            sourceBuilder.AppendLine( "    {");

            foreach (var method in group)
            {
                if (string.IsNullOrWhiteSpace(method!.TemplateText))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "MG001",
                            "Empty template",
                            "Method '{0}' has empty template text",
                            "MethodGenerator",
                            DiagnosticSeverity.Warning,
                            true),
                        Location.None,
                        method.MethodName));
                    continue;
                }
                
                
                
                GenerateMethodBody(sourceBuilder, method!);
            }

            sourceBuilder.AppendLine("    }");
            sourceBuilder.AppendLine("}");

            context.AddSource(
                $"{group.Key.ContainingClass}_GeneratedMethods.cs",
                SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }

    private static void GenerateMethodBody(StringBuilder builder, MethodInfo method)
    {
        var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
        var modifiers = method.Modifiers.Replace("partial", "").Trim();
        if (!string.IsNullOrEmpty(modifiers))
            modifiers += " ";

        builder.AppendLine($"        {modifiers}partial {method.ReturnType} {method.MethodName}({parameters})");
        builder.AppendLine("        {");
            
        // Generate method body from template text
        var bodyLines = GenerateBodyFromTemplate(method.TemplateText, method);
        foreach (var line in bodyLines)
        {
            builder.AppendLine($"            {line}");
        }
            
        builder.AppendLine("        }");
        builder.AppendLine();
    }

    private static List<string> GenerateBodyFromTemplate(string template, MethodInfo method)
    {
        var lines = new List<string>();
        return lines;
    }
}