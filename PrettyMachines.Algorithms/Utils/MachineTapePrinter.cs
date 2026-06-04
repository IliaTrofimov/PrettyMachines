using System.Text;
using PrettyMachines.Algorithms.Turing;


namespace PrettyMachines.Algorithms.Utils;

/// <summary>
/// Set of methods for printing <see cref="MachineTape"/> as text.
/// </summary>
public static class MachineTapePrinter
{
    /// <summary>Prints <see cref="MachineTape"/> cells into a string.</summary>
    public static string Print(MachineTape tape)
    {
        var length = tape.Sum(c => c?.Length ?? 0);
        var builder = new StringBuilder(length);
        Print(builder, tape);
        return builder.ToString();
    }
    
    /// <summary>Prints <see cref="MachineTape"/> cells into given <see cref="StringBuilder"/> object.</summary>
    public static void Print(StringBuilder builder, MachineTape tape)
    {
        foreach (var cell in tape.EnumerateCells(trimEmptyCells: true))
            builder.Append(cell);
    }
    
    /// <summary>Prints <see cref="MachineTape"/> cells into given stream.</summary>
    public static void Print(Stream stream, MachineTape tape)
    {
        using var writer = new StreamWriter(stream, leaveOpen: true);
        foreach (var cell in tape.EnumerateCells(trimEmptyCells: true))
            writer.Write(cell);
    }
}