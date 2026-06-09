using System.Text;
using PrettyMachines.Algorithms.Turing;


namespace PrettyMachines.Algorithms.Utils;

public static class PrintingExtensions
{
    /// <summary>Converts movement enum into single character representation.</summary>
    public static char ToChar(this TapeMovement movement) => movement switch
    {
        TapeMovement.None  => 'N',
        TapeMovement.Left  => 'L',
        TapeMovement.Right => 'R',
        _                  => '?'
    };

    /// <summary>
    /// Appends given value surrounded with quotes.
    /// </summary>
    internal static StringBuilder AppendQuoted(this StringBuilder builder, string? value, char quote)
    {
        return builder.Append(quote).Append(value).Append(quote);
    }
}