using System.Text;
using System.Text.RegularExpressions;
using PrettyMachines.Algorithms.Abstract;


namespace PrettyMachines.Algorithms.Markov;

[Flags]
public enum MarkovSubstitutionParsingFlags
{
    /// <summary>Default options.</summary>
    Default = 0,
    /// <summary>Wrap pattern and replacement strings with single quotes <c>'</c>.</summary>
    QuoteStrings = 1,
    /// <summary>Use arrow with dot <c>->.</c> instead of double arrow <c>=></c> for terminal rules.</summary>
    UseDottedArrow = 2
}

/// <summary>Provides parsing and printing capabilities for Markov algorithms.</summary>
public sealed class MarkovAlgorithmParser
{
    private static readonly Regex GetHeaderRegex = new(@"^//(?<header>\w+):\s*(?<value>.+)", RegexOptions.Compiled);
    private static readonly Regex QuotedRuleRegex = new(@"^'(?<pattern>.*)'\s*(?<arrow>[\->|=>|\->\.])\s*'(?<replacement>.*)'\s*(//(.+))?", RegexOptions.Compiled);
    private static readonly Regex RuleRegex = new(@"^(?<pattern>.*)(?<arrow>[\->|=>|\->\.])(?<replacement>.*)", RegexOptions.Compiled);
    
    
    public MarkovSubstitutionParsingFlags Options { get; set; }
    
    public void Print(MarkovAlgorithm algorithm, TextWriter textWriter)
    {
        if (!string.IsNullOrWhiteSpace(algorithm.Name))
        {
            textWriter.Write("//name:");
            textWriter.WriteLine(algorithm.Name);
        }
        
        if (algorithm.Alphabet?.Count > 0)
        {
            textWriter.Write("//alphabet:");
            textWriter.WriteLine(string.Join("", algorithm.Alphabet));
        }
        
        if (algorithm.Markers?.Count > 0)
        {
            textWriter.Write("//markers:");
            textWriter.WriteLine(string.Join("", algorithm.Markers));
        }

        foreach (var rule in algorithm.Rules)
        {
            textWriter.WriteLine(rule.ToString(Options));
        }

        textWriter.Flush();
    }

    
    public string CreateSourceCode(string algorithmText)
    {
        MarkovAlgorithm algorithm;
        using (var sr = new StringReader(algorithmText))
        { 
            algorithm = Parse(sr);
        }

        var sourceBuilder = new StringBuilder(
            $"{nameof(MarkovAlgorithm)}.{nameof(MarkovAlgorithm.Create)}(\"{algorithm.Name}\")"
        );

        if (algorithm.Markers?.Count > 0)
        {
            var markersString = string.Join("", algorithm.Markers);
            sourceBuilder.Append(
                $"\n    .{nameof(IMarkovAlgorithmBuilder.WithMarkers)}(\"{markersString}\")"
            );
        }
        
        if (algorithm.Alphabet?.Count > 0)
        {
            var alphabetString = string.Join("", algorithm.Alphabet);
            sourceBuilder.Append(
                $"\n    .{nameof(IMarkovAlgorithmBuilder.WithAlphabet)}(\"{alphabetString}\")"
            );
        }
        
        foreach (var (rule, index) in algorithm.Rules.Select((r, i) => (r, i)))
        {
            sourceBuilder.Append(
                $"\n    .{nameof(IMarkovAlgorithmBuilder.AddRule)}(\"{rule.Pattern}\", \"{rule.Replacement}\""
            );
            
            if (rule.IsTerminal)
            {
                sourceBuilder.Append(", true)");
            }
            else
            {
                sourceBuilder.Append(')');
            }

            var comment = algorithm.GetRuleComment(index);
            if (!string.IsNullOrWhiteSpace(comment))
            {
                sourceBuilder.Append(
                    $".{nameof(IMarkovSubstitutionBuilder.WithComment)}(\"{comment}\")"
                );
            }
        }

        sourceBuilder.AppendLine(";");
        return sourceBuilder.ToString();
    }

    public MarkovAlgorithm Parse(TextReader reader)
    {
        throw new NotImplementedException();
    }
    
}