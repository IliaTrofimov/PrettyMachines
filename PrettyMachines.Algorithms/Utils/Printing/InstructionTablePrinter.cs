using System.Text;
using PrettyMachines.Algorithms.Turing;


namespace PrettyMachines.Algorithms.Utils.Printing;

/// <summary>
/// Set of methods for printing <see cref="TuringMachine "/> as text.
/// </summary>
public static class InstructionTablePrinter
{
    public const char DefaultQuote = '\"';
    public const char DefaultCsvSeparator = ',';
    
    /// <summary>Prints given instructions as formatted text machine .</summary>
    public static string PrintTable(TuringMachine machine, char quote = DefaultQuote)
    {
        var cellWidth = machine.Instructions.Alphabet.Max(s => s?.Length ?? 0);
        var size = cellWidth * (machine.Instructions.Alphabet.Count + 1) * (machine.Instructions.States.Count + 1);
        var builder = new StringBuilder(size);
        PrintTable(builder, machine );
        return builder.ToString();
    }
    
    /// <summary>Prints given instructions as formatted text machine  into the <see cref="StringBuilder"/> object.</summary>
    public static void PrintTable(StringBuilder builder, TuringMachine  machine , char quote = DefaultQuote)
    {
        PrintTable(machine , new StringBuilderOutput(builder), quote);
    }
    
    /// <summary>Prints given instructions as formatted text machine  into the stream object.</summary>
    public static void PrintTable(Stream stream, TuringMachine  machine , char quote = DefaultQuote, Encoding? encoding = null)
    {
        using var output = new StreamOutput(stream, leaveOpen: true, encoding: encoding);
        PrintTable(machine , output, quote);
    }
    
    /// <summary>Prints given instructions as formatted list.</summary>
    public static string PrintList(TuringMachine machine, char quote = DefaultQuote)
    {
        var cellWidth = machine.Instructions.Alphabet.Max(s => s?.Length ?? 0);
        var size = cellWidth * (machine.Instructions.Alphabet.Count + 1) * (machine.Instructions.States.Count + 1);
        var builder = new StringBuilder(size);
        PrintList(builder, machine , quote);
        return builder.ToString();
    }

    /// <summary>Prints given instructions as formatted list into the <see cref="StringBuilder"/> object.</summary>
    public static void PrintList(StringBuilder builder, TuringMachine machine, char quote = DefaultQuote)
    {
        PrintList(machine , new StringBuilderOutput(builder), quote);
    }

    /// <summary>Prints given instructions as formatted list into the stream object.</summary>
    public static void PrintList(Stream stream, TuringMachine machine, char quote = DefaultQuote, Encoding? encoding = null)
    {
        using var output = new StreamOutput(stream, leaveOpen: true, encoding: encoding);
        PrintList(machine , output, quote);
    }
    
    /// <summary>Prints given instructions as CSV string.</summary>
    public static string PrintCsv(TuringMachine machine, char quote = DefaultQuote, char sep = DefaultCsvSeparator)
    {
        var builder = new StringBuilder();
        PrintCsv(builder, machine );
        return builder.ToString();
    }
    
    /// <summary>Prints given instructions as CSV into the <see cref="StringBuilder"/> object.</summary>
    public static void PrintCsv(StringBuilder builder, TuringMachine machine, char quote = DefaultQuote, char sep = DefaultCsvSeparator)
    {
        PrintCsv(machine , new StringBuilderOutput(builder), quote, sep);
    }
    
    /// <summary>Prints given instructions as CSV into the stream object.</summary>
    public static void PrintCsv(Stream stream, TuringMachine machine, char quote = DefaultQuote, char sep = DefaultCsvSeparator, Encoding? encoding = null)
    {
        using var output = new StreamOutput(stream, leaveOpen: true, encoding: encoding);
        PrintCsv(machine , output, quote, sep);
    }
    
    private static void PrintTable(TuringMachine machine, TextOutput output, char quote = DefaultQuote)
    {
        var hasNotEmptyMatch = machine.Instructions.Any(x => x.ScannedSymbol.Match == SymbolMatch.NotEmpty);
        var hasEmptyMatch = machine.Instructions.Any(x => x.ScannedSymbol.Match == SymbolMatch.Empty);
        var hasAnyMatch = machine.Instructions.Any(x => x.ScannedSymbol.Match == SymbolMatch.Any);
        
        var maxSymbolLen = machine.Instructions.Alphabet.Max(s => s?.Length ?? 0);
        var columnWidth = maxSymbolLen + 4 + 6;
       
        PrintHeader(machine, output, quote);

        var columnWriter = new ColumnWriter(output, quoteChar: quote);
        
        output.Print('|');
        columnWriter.Print(" ", 4);
        output.Print('|');

        foreach (var symbol in machine.Instructions.Alphabet)
        {
            columnWriter.Print(symbol ?? "<null>", columnWidth, Alignment.Center);
            output.Print('|');
        }

        if (hasNotEmptyMatch)
        {
            columnWriter.Print(SymbolMatch.NotEmpty.ToString(), columnWidth, Alignment.Center);
            output.Print('|');
        }
        if (hasEmptyMatch)
        {
            columnWriter.Print(SymbolMatch.Empty.ToString(), columnWidth, Alignment.Center);
            output.Print('|');
        }
        if (hasAnyMatch)
        {
            columnWriter.Print(SymbolMatch.Any.ToString(), columnWidth, Alignment.Center);
            output.Print('|');
        }
        
        output.PrintLine();

        var i = 0;
        foreach (var state in machine.Instructions.States)
        {
            output.Print('|');
            columnWriter.Print(state.ToString(), 4, Alignment.Right);
            output.Print('|');

            foreach (var symbol in machine.Instructions.Alphabet)
            {
                var action = machine.Instructions[state, FuzzyKey<string>.Exact(symbol!)];
                columnWriter.Print(action?.ToFormattedString(quote) ?? "-", columnWidth);
                output.Print('|');
            }   
            
            if (hasNotEmptyMatch)
            {
                var specialAction = machine.Instructions[state, FuzzyKey<string>.NotEmpty];
                columnWriter.Print(specialAction?.ToFormattedString(quote) ?? "-", columnWidth);
                output.Print('|');
            }
            if (hasEmptyMatch)
            {
                var specialAction = machine.Instructions[state, FuzzyKey<string>.Empty];
                columnWriter.Print(specialAction?.ToFormattedString(quote) ?? "-", columnWidth);
                output.Print('|');
            }
            if (hasAnyMatch)
            {
                var specialAction = machine.Instructions[state, FuzzyKey<string>.Any];
                columnWriter.Print(specialAction?.ToFormattedString(quote) ?? "-", columnWidth);
                output.Print('|');
            }

            if (i < machine.Instructions.States.Count - 1)
            {
                output.PrintLine();
            }

            i++;
        }
    }
    
    private static void PrintList(TuringMachine  machine , TextOutput output, char quote)
    {
        /*
        var maxLeft = machine .Max(x =>
        {
            var symbolLen = x.ScannedSymbol.Match == SymbolMatch.Exact
                ? x.ScannedSymbol.Value?.Length ?? 0
                : 9;
            return symbolLen + 4 + 1;
        });
        var maxRight = machine .Max(x =>
        {
            var symbolLen = x.PrintedSymbol?.Length ?? 4;
            return symbolLen + 4 + 1 + 2;
        });
        */
        PrintHeader(machine, output, quote);

        var i = 0;
        foreach (var instruction in machine.Instructions)
        {
            output.Print(instruction.InitialState.ToString());
            output.Print(' ');
            
            if (instruction.ScannedSymbol.Match == SymbolMatch.Exact)
                output.PrintQuoted(instruction.ScannedSymbol.Value, quote);
            else
                output.Print(instruction.ScannedSymbol.ToString());
            
            output.Print(" -> ");
            
            output.Print(instruction.NextState.ToString());
            output.Print(' ');

            if (instruction.PrintedSymbol != null)
            {
                output.PrintQuoted(instruction.PrintedSymbol, quote);
                output.Print(' ');
            }
            
            output.Print(instruction.Movement.ToChar());
            
            if (i < machine.Instructions.RulesCount - 1)
            {
                output.PrintLine();
            }

            i++;
        }
    }
    
    private static void PrintCsv(TuringMachine machine, TextOutput output, char quote, char sep)
    {
        PrintHeader(machine, output, quote);
        
        var i = 0;
        foreach (var instruction in machine.Instructions)
        {
            output.PrintQuoted(instruction.InitialState.ToString(), quote);
            output.Print(sep);
            output.PrintQuoted(instruction.ScannedSymbol.ToString(), quote);
            output.Print(sep);
            output.PrintQuoted(instruction.NextState.ToString(), quote);
            output.Print(sep);
            output.PrintQuoted(instruction.PrintedSymbol ?? "<none>", quote);
            output.Print(sep);
            output.PrintQuoted(instruction.Movement.ToString(), quote);
            
            if (i != machine.Instructions.RulesCount - 1)
            {
                output.PrintLine();
            }

            i++;
        }
    }
    
    private static void PrintHeader(TuringMachine machine, TextOutput output, char quote)
    {
        if (quote == default)
            quote = DefaultQuote;
        
        if (!string.IsNullOrWhiteSpace(machine.Name))
        {
            output.Print("//name: ");
            output.PrintQuoted(machine.Name, quote);
            output.PrintLine();
        }
        
        if (machine.Instructions.Alphabet.Count > 0)
        {
            output.Print("//alphabet-strict: ");
            output.PrintQuoted(machine.HasStrictAlphabet.ToString(), quote);
            output.PrintLine();
            
            if (machine.Instructions.Alphabet.All(c => (c?.Length ?? 0) <= 1))
            {
                output.Print("//alphabet-characters: ");
                output.PrintQuoted(string.Join("", machine.Instructions.Alphabet), quote);
            }
            else
            {
                output.Print("//alphabet-tokens: ");
                foreach (var c in machine.Instructions.Alphabet)
                {
                    output.PrintQuoted(c, quote);
                    output.Print(' ');
                }
            }
            output.PrintLine();   
        }
    }
}