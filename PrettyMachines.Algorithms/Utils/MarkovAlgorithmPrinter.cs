using System.Text;
using PrettyMachines.Algorithms.Markov;
using PrettyMachines.Algorithms.Utils.Printing;


namespace PrettyMachines.Algorithms.Utils;

/// <summary>
/// Set of methods for printing <see cref="MarkovAlgorithm"/> as text.
/// </summary>
public static class MarkovAlgorithmPrinter
{
    public const char DefaultQuote = '\"';
    public const char DefultCsvSeparator = ',';
    public const string TerminalArrowQuoted = " => ";
    public const string NonTerminalArrowQuoted = " -> ";
    public const string TerminalArrow = "=>";
    public const string NonTerminalArrow = "->";

    /// <summary>Prints given algorithm instructions as formatted text table.</summary>
    public static string PrintFormatted(MarkovAlgorithm algorithm, char quote = DefaultQuote)
    {
        var size = algorithm.Rules.Sum(r => r.Pattern.Length + r.Replacement.Length) + 9;
        var builder = new StringBuilder(size);
        PrintFormatted(builder, algorithm, quote);
        return builder.ToString();
    }
    
    /// <summary>Prints given algorithm instructions as formatted text table into the <see cref="StringBuilder"/> object.</summary>
    public static void PrintFormatted(StringBuilder builder, MarkovAlgorithm algorithm, char quote = DefaultQuote)
    {
        PrintFormatted(algorithm, new StringBuilderOutput(builder), quote);
    }
    
    /// <summary>Prints given algorithm instructions as formatted text table into the stream object.</summary>
    public static void PrintFormatted(Stream stream, MarkovAlgorithm algorithm, char quote = DefaultQuote, Encoding? encoding = null)
    {
        using var output = new StreamOutput(stream, leaveOpen: true, encoding: encoding ?? Encoding.UTF8);
        PrintFormatted(algorithm, output, quote);
    }
    
    /// <summary>Prints given algorithm instructions as CSV string.</summary>
    public static string PrintCsv(MarkovAlgorithm algorithm, char quote = DefaultQuote, char sep = DefultCsvSeparator)
    {
        var size = algorithm.Rules.Sum(r => r.Pattern.Length + r.Replacement.Length) + 9;
        var builder = new StringBuilder(size);
        PrintCsv(builder, algorithm, quote, sep);
        return builder.ToString();
    }
    
    /// <summary>Prints given algorithm instructions as CSV into the <see cref="StringBuilder"/> object.</summary>
    public static void PrintCsv(StringBuilder builder, MarkovAlgorithm algorithm, char quote = DefaultQuote, char sep = DefultCsvSeparator)
    {
        PrintCsv(algorithm, new StringBuilderOutput(builder), quote, sep);
    }
    
    /// <summary>Prints given algorithm instructions as CSV into the stream object.</summary>
    public static void PrintCsv(Stream stream, MarkovAlgorithm algorithm, char quote = DefaultQuote, char sep = DefultCsvSeparator, Encoding? encoding = null)
    {
        var output = new StreamOutput(stream, leaveOpen: true, encoding: encoding ?? Encoding.UTF8);
        PrintCsv(algorithm, output, quote, sep);
    }
    
    private static void PrintFormatted(MarkovAlgorithm algorithm, TextOutput output, char quote)
    {
        int maxPatternLen = 0, maxReplacementLen = 0;
        foreach (var rule in algorithm.Rules)
        {
            maxPatternLen = int.Max(maxPatternLen, rule.Pattern.Length);
            maxReplacementLen = int.Max(maxReplacementLen, rule.Replacement.Length);
        }
        
        var columnWriter = new ColumnWriter(output, ' ', quote);

        PrintHeader(algorithm, output, quote);

        var (termArrow, arrow) = quote == default
            ? (TerminalArrow, NonTerminalArrow)
            : (TerminalArrowQuoted, NonTerminalArrowQuoted);
        
        for (var i = 0; i < algorithm.Rules.Count; i++)
        {
            var rule = algorithm.Rules[i];
            var comment = algorithm.GetRuleComment(i);

            columnWriter.PrintQuoted(rule.Pattern, maxPatternLen);
            output.Print(rule.IsTerminal ? termArrow : arrow);
            columnWriter.PrintQuoted(rule.Replacement, maxReplacementLen);

            if (!string.IsNullOrWhiteSpace(comment))
            {
                output.Print(" // ");
                output.Print(comment);
            }

            if (i != algorithm.Rules.Count - 1)
            {
                output.PrintLine();
            }
        }
    }
    
    private static void PrintCsv(MarkovAlgorithm algorithm, TextOutput output, char quote, char sep)
    {
        PrintHeader(algorithm, output, quote);
        
        for (var i = 0; i < algorithm.Rules.Count; i++)
        {
            var rule = algorithm.Rules[i];

            output.PrintQuoted(rule.Pattern, quote);
            output.Print(sep);
            output.PrintQuoted(rule.Replacement, quote);
            output.Print(sep);
            output.PrintQuoted(rule.IsTerminal.ToString(), quote);
            output.Print(sep);
            output.PrintQuoted(algorithm.GetRuleComment(i), quote);

            if (i != algorithm.Rules.Count - 1)
            {
                output.PrintLine();
            }
        }
    }

    private static void PrintHeader(MarkovAlgorithm algorithm, TextOutput output, char quote)
    {
        if (quote == default)
            quote = DefaultQuote;
        
        if (!string.IsNullOrWhiteSpace(algorithm.Name))
        {
            output.Print("//name: ");
            output.PrintQuoted(algorithm.Name, quote);
            output.PrintLine();
        }
        
        if (algorithm.Alphabet?.Count > 0)
        {
            output.Print("//alphabet: ");
            output.PrintQuoted(string.Join("", algorithm.Alphabet), quote);
            output.PrintLine();
        }
        
        if (algorithm.Markers?.Count > 0)
        {
            output.Print("//markers: ");
            output.PrintQuoted(string.Join("", algorithm.Markers), quote);
            output.PrintLine();
        }
    }
}