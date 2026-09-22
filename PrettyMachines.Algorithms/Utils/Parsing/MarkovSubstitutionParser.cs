using PrettyMachines.Markov;


namespace PrettyMachines.Utils.Parsing;

/// <summary>
/// Text parser that creates <see cref="Substitution"/> objects.
/// </summary>
public sealed class MarkovSubstitutionParser
{
    private const string TerminalArrow = "=>";
    private const string NonTerminalArrow = "->";


    /// <summary>Parses a quoted substitution rule like <c>'a' -> 'b'</c>.</summary>
    /// <param name="text">Text that contains a quoted pattern, an arrow and a quoted replacement.</param>
    /// <param name="quote">Quoting character used around pattern and replacement.</param>
    /// <returns>Parsed substitution rule.</returns>
    /// <exception cref="FormatException">Arrow separator is missing.</exception>
    public Substitution ParseQuoted(string text, char quote = '\'')
    {
        ArgumentNullException.ThrowIfNull(text);

        var (left, right, isTerminal) = Split(text, quote, trim: true);
        return new Substitution(Unquote(left, quote), Unquote(right, quote), isTerminal);
    }


    /// <summary>Parses an unquoted substitution rule like <c>a -> b</c>.</summary>
    /// <param name="text">Text that contains a pattern, an arrow and a replacement.</param>
    /// <returns>Parsed substitution rule.</returns>
    /// <exception cref="FormatException">Arrow separator is missing.</exception>
    public Substitution ParseUnquoted(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var (left, right, isTerminal) = Split(text, quote: null, trim: false);
        return new Substitution(left, right, isTerminal);
    }


    private static (string Left, string Right, bool IsTerminal) Split(string text, char? quote, bool trim)
    {
        var inQuote = false;

        for (var i = 0; i < text.Length - 1; i++)
        {
            var current = text[i];
            if (quote is not null && current == quote.Value)
            {
                inQuote = !inQuote;
                continue;
            }

            if (inQuote || current is not ('-' or '=') || text[i + 1] != '>')
                continue;

            var isTerminal = current == '=';
            var left = text[..i];
            var right = text[(i + 2)..];
            return trim
                ? (left.Trim(), right.Trim(), isTerminal)
                : (left, right, isTerminal);
        }

        throw new FormatException($"Substitution text '{text}' does not contain an arrow separator ('{NonTerminalArrow}' or '{TerminalArrow}').");
    }

    private static string Unquote(string text, char quote)
    {
        return text.Length >= 2 && text[0] == quote && text[^1] == quote
            ? text[1..^1]
            : text;
    }
}
