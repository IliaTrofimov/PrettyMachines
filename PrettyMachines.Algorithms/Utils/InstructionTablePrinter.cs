using System.Text;
using PrettyMachines.Algorithms.Turing;


namespace PrettyMachines.Algorithms.Utils;

/// <summary>
/// Set of methods for printing <see cref="IReadOnlyInstructionsTable"/> as text.
/// </summary>
public static class InstructionTablePrinter
{
    /// <summary>Prints given instructions as formatted text table.</summary>
    public static string PrintTable(IReadOnlyInstructionsTable table)
    {
        var cellWidth = table.Alphabet.Max(s => s?.Length ?? 0);
        var size = cellWidth * (table.Alphabet.Count + 1) * (table.States.Count + 1);
        var builder = new StringBuilder(size);
        PrintTable(builder, table);
        return builder.ToString();
    }
    
    /// <summary>Prints given instructions as formatted text table into the <see cref="StringBuilder"/> object.</summary>
    public static void PrintTable(StringBuilder builder, IReadOnlyInstructionsTable table)
    {
        PrintTable(table, 
            printLn:  () => builder.AppendLine(),
            printTxt: (s) => builder.Append(s),
            printFmt: (fmt, s) => builder.AppendFormat(fmt, s)
        );
    }
    
    /// <summary>Prints given instructions as formatted text table into the stream object.</summary>
    public static void PrintTable(Stream stream, IReadOnlyInstructionsTable table)
    {
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        {
            PrintTable(table, 
                printLn:  () => writer.WriteLine(),
                printTxt: (s) => writer.Write(s),
                printFmt: (fmt, s) => writer.Write(fmt, s)
            );   
        }
    }
    
    /// <summary>Prints given instructions as formatted list.</summary>
    public static string PrintList(IReadOnlyInstructionsTable table)
    {
        var cellWidth = table.Alphabet.Max(s => s?.Length ?? 0);
        var size = cellWidth * (table.Alphabet.Count + 1) * (table.States.Count + 1);
        var builder = new StringBuilder(size);
        PrintList(builder, table);
        return builder.ToString();
    }
    
    /// <summary>Prints given instructions as formatted list into the <see cref="StringBuilder"/> object.</summary>
    public static void PrintList(StringBuilder builder, IReadOnlyInstructionsTable table)
    {
        PrintList(table, 
            printLn:  () => builder.AppendLine(),
            printTxt: (s) => builder.Append(s),
            printFmt: (fmt, s) => builder.AppendFormat(fmt, s)
        );
    }
    
    /// <summary>Prints given instructions as formatted list into the stream object.</summary>
    public static void PrintList(Stream stream, IReadOnlyInstructionsTable table)
    {
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        {
            PrintList(table, 
                printLn:  () => writer.WriteLine(),
                printTxt: (s) => writer.Write(s),
                printFmt: (fmt, s) => writer.Write(fmt, s)
            );   
        }
    }
    
    /// <summary>Prints given instructions as CSV string.</summary>
    public static string PrintCsv(IReadOnlyInstructionsTable table)
    {
        var cellWidth = table.Alphabet.Max(s => s?.Length ?? 0);
        var size = cellWidth * (table.Alphabet.Count + 1) * (table.States.Count + 1);
        var builder = new StringBuilder(size);
        PrintCsv(builder, table);
        return builder.ToString();
    }
    
    /// <summary>Prints given instructions as CSV into the <see cref="StringBuilder"/> object.</summary>
    public static void PrintCsv(StringBuilder builder, IReadOnlyInstructionsTable table)
    {
        PrintCsv(table, s => builder.Append(s));
    }
    
    /// <summary>Prints given instructions as CSV into the stream object.</summary>
    public static void PrintCsv(Stream stream, IReadOnlyInstructionsTable table)
    {
        using var writer = new StreamWriter(stream, leaveOpen: true);
        PrintCsv(table, s => writer.Write(s));
    }
    
    private static void PrintTable(IReadOnlyInstructionsTable table, 
                                   Action printLn, 
                                   Action<string?> printTxt, 
                                   Action<string, string?> printFmt)
    {
        var hasNotEmptyMatch = table.Any(x => x.ScannedSymbol.Match == SymbolMatch.NotEmpty);
        var hasEmptyMatch = table.Any(x => x.ScannedSymbol.Match == SymbolMatch.Empty);
        var hasAnyMatch = table.Any(x => x.ScannedSymbol.Match == SymbolMatch.Any);
        
        var maxSymbolLen = table.Alphabet.Max(s => s?.Length ?? 0);
        var columnWidth = maxSymbolLen + 4 + 6 + 1;
        var columnFmt = $"|{{0,{columnWidth}}}";
        
        printTxt("----");
        foreach (var symbol in table.Alphabet)
            printFmt(columnFmt, symbol ?? "<null>");

        if (hasNotEmptyMatch)
            printFmt(columnFmt, SymbolMatch.NotEmpty.ToString());
        if (hasEmptyMatch)
            printFmt(columnFmt, SymbolMatch.Empty.ToString());
        if (hasAnyMatch)
            printFmt(columnFmt, SymbolMatch.Any.ToString());
        
        printLn();

        foreach (var state in table.States)
        {
            printFmt("{0,4}", state.ToString(true));
            
            foreach (var symbol in table.Alphabet)
            {
                var action = table[state, FuzzyKey<string>.Exact(symbol!)];
                printFmt(columnFmt, action?.ToString(true) ?? "-");
            }   
            
            if (hasNotEmptyMatch)
            {
                var specialAction = table[state, FuzzyKey<string>.NotEmpty];
                printFmt(columnFmt, specialAction?.ToString(true) ?? "-");
            }
            if (hasEmptyMatch)
            {
                var specialAction = table[state, FuzzyKey<string>.Empty];
                printFmt(columnFmt, specialAction?.ToString(true) ?? "-");
            }
            if (hasAnyMatch)
            {
                var specialAction = table[state, FuzzyKey<string>.Any];
                printFmt(columnFmt, specialAction?.ToString(true) ?? "-");
            }

            printLn();
        }
    }
    
    private static void PrintList(IReadOnlyInstructionsTable table,
                                       Action printLn,
                                       Action<string?> printTxt,
                                       Action<string, string?> printFmt)
    {
        var maxLeft = table.Max(x =>
        {
            var symbolLen = x.ScannedSymbol.Match == SymbolMatch.Exact
                ? x.ScannedSymbol.Value?.Length ?? 0
                : 9;
            return symbolLen + 4 + 1;
        });
        var maxRight = table.Max(x =>
        {
            var symbolLen = x.PrintedSymbol?.Length ?? 4;
            return symbolLen + 4 + 1 + 2;
        });

        var leftFmt = $"{{0,-{maxLeft}}}";
        var rightFmt = $"{{0,{maxRight}}}";
        
        foreach (var x in table)
        {
            printFmt(leftFmt, x.InitialState.ToString(false) + " \'" + x.ScannedSymbol + "\'");
            printTxt(" -> ");
            if (x.PrintedSymbol is null)
                printFmt(rightFmt, x.NextState.ToString(false) + " none " + x.Movement.ToChar());
            else
                printFmt(rightFmt, x.NextState.ToString(false) + " \'" + x.PrintedSymbol + "\' " + x.Movement.ToChar());

            printLn();
        }
    }
    
    private static void PrintCsv(IReadOnlyInstructionsTable table,
                                Action<string?> printTxt)
    {
        foreach (var instruction in table)
        {
            printTxt("\"");
            printTxt(instruction.InitialState.ToString(true));
            printTxt("\", \"");
            printTxt(instruction.ScannedSymbol.ToString());
            printTxt("\", \"");
            printTxt(instruction.NextState.ToString(true));
            printTxt("\", \""); 
            printTxt(instruction.PrintedSymbol ?? "<none>");
            printTxt("\", \"");
            printTxt(instruction.Movement.ToString());
            printTxt("\"\n");
        }
    }
}