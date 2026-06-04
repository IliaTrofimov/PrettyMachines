using System.Text;
using PrettyMachines.Algorithms.Markov;


namespace PrettyMachines.Algorithms.Utils;

/// <summary>
/// Set of methods for printing <see cref="MarkovAlgorithm"/> as text.
/// </summary>
public static class MarkovAlgorithmPrinter
{
    /// <summary>Prints given algorithm instructions as formatted text table.</summary>
    public static string PrintFormatted(MarkovAlgorithm algorithm)
    {
        var size = algorithm.Rules.Sum(r => r.Pattern.Length + r.Replacement.Length) + 9;
        var builder = new StringBuilder(size);
        PrintFormatted(builder, algorithm);
        return builder.ToString();
    }
    
    /// <summary>Prints given algorithm instructions as formatted text table into the <see cref="StringBuilder"/> object.</summary>
    public static void PrintFormatted(StringBuilder builder, MarkovAlgorithm algorithm)
    {
        PrintList(algorithm, 
            printLn:  () => builder.AppendLine(),
            printTxt: (s) => builder.Append(s),
            printFmt: (fmt, s) => builder.AppendFormat(fmt, s)
        );
    }
    
    /// <summary>Prints given algorithm instructions as formatted text table into the stream object.</summary>
    public static void PrintFormatted(Stream stream, MarkovAlgorithm algorithm)
    {
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        {
            PrintList(algorithm, 
                printLn:  () => writer.WriteLine(),
                printTxt: (s) => writer.Write(s),
                printFmt: (fmt, s) => writer.Write(fmt, s)
            );   
        }
    }
    
    /// <summary>Prints given algorithm instructions as CSV string.</summary>
    public static string PrintCsv(MarkovAlgorithm algorithm)
    {
        var size = algorithm.Rules.Sum(r => r.Pattern.Length + r.Replacement.Length) + 9;
        var builder = new StringBuilder(size);
        PrintCsv(builder, algorithm);
        return builder.ToString();
    }
    
    /// <summary>Prints given algorithm instructions as CSV into the <see cref="StringBuilder"/> object.</summary>
    public static void PrintCsv(StringBuilder builder, MarkovAlgorithm algorithm)
    {
        foreach (var rule in algorithm.Rules)
        {
            builder.Append('\"');
            builder.Append(rule.Pattern);
            builder.Append("\", \"");
            builder.Append('\"');
            builder.Append(rule.Replacement);
            builder.Append("\", \"");
            builder.Append('\"');
            builder.Append(rule.IsTerminal);
            builder.AppendLine("\"");
        }
    }
    
    /// <summary>Prints given algorithm instructions as CSV into the stream object.</summary>
    public static void PrintCsv(Stream stream, MarkovAlgorithm algorithm)
    {
        using var writer = new StreamWriter(stream, leaveOpen: true);
        foreach (var rule in algorithm.Rules)
        {
            writer.Write('\"');
            writer.Write(rule.Pattern);
            writer.Write("\", \"");
            writer.Write('\"');
            writer.Write(rule.Replacement);
            writer.Write("\", \"");
            writer.Write('\"');
            writer.Write(rule.IsTerminal);
            writer.WriteLine('\"');
        }
    }
    
    private static void PrintList(MarkovAlgorithm algorithm,
                                   Action printLn,
                                   Action<string?> printTxt,
                                   Action<string, string?> printFmt)
    {
        int maxPatternLen = 0, maxReplacementLen = 0;
        foreach (var rule in algorithm.Rules)
        {
            maxPatternLen = int.Max(maxPatternLen, rule.Pattern.Length);
            maxReplacementLen = int.Max(maxReplacementLen, rule.Replacement.Length);
        }

        var patternFmt = $"{{0,{maxPatternLen}}}";
        var replacementFmt = $"{{0,{maxReplacementLen}}}";

        foreach (var rule in algorithm.Rules)
        {
            printTxt("'");
            printFmt(patternFmt, rule.Pattern);
            printTxt(rule.IsTerminal ? "' => '" : "' -> '");
            printFmt(replacementFmt, rule.Replacement);
            printTxt("'");
            printLn();
        }
    }
}