using PrettyMachines.Algorithms.Markov;


namespace PrettyMachines.Implementations;

/// <summary>Set of pre-made Markov algorithms.</summary>
public static class MarkovAlgorithms
{
    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that recognizes correct brackets sequences.<br/>
    /// <b>- alphabet:</b> opening and closing brackets of one type from <paramref name="symbols"/>.<br/>
    /// <b>- outputs:</b> <see cref="BracketsGrammarSymbols.Accepted"/> or <see cref="BracketsGrammarSymbols.Rejected"/> from <paramref name="symbols"/>.
    /// </summary>
    public static MarkovAlgorithm Create_BracketsGrammar(BracketsGrammarSymbols? symbols = null)
    {
        symbols ??= new BracketsGrammarSymbols();
        symbols.Validate();
        
        return MarkovAlgorithm.Create("Brackets grammar")
            .WithAlphabet(symbols.Left, symbols.Right)
            .WithMarkers(symbols.Marked, symbols.Accepted, symbols.Rejected)
            // Shrink all matching brackets
            .AddRule($"{symbols.Left}{symbols.Right}",                 symbols.Marked).WithComment("mark pair")
            .AddRule($"{symbols.Marked}{symbols.Marked}",              symbols.Marked).WithComment("collapse markers")
            .AddRule($"{symbols.Left}{symbols.Marked}{symbols.Right}", symbols.Marked).WithComment("mark pair")
            // Reject all not matching
            .AddRule($"{symbols.Left}{symbols.Marked}",                symbols.Rejected, true).WithComment("reject")
            .AddRule($"{symbols.Right}{symbols.Marked}",               symbols.Rejected, true).WithComment("reject")
            .AddRule($"{symbols.Marked}{symbols.Left}",                symbols.Rejected, true).WithComment("reject")
            .AddRule($"{symbols.Marked}{symbols.Right}",               symbols.Rejected, true).WithComment("reject")
            .AddRule($"{symbols.Left}",                                symbols.Rejected, true).WithComment("reject")
            .AddRule($"{symbols.Right}",                               symbols.Rejected, true).WithComment("reject")
            // Clear markers
            .AddRule(symbols.Marked,                                   "").WithComment("collapse markers")
            .AddRule("",                                               symbols.Accepted, true).WithComment("accept")
            .Build();
    }
    
    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that adds 1 to given binary number.<br/>
    /// <b>- alphabet:</b> 0 and 1.<br/>
    /// <b>- outputs:</b> calculated binary number.
    /// </summary>
    public static MarkovAlgorithm Create_BinaryIncrement()
    {
        return MarkovAlgorithm.Create("Binary increment")
            .WithAlphabet('0', '1')
            .WithMarkers('*', '$')
            .AddRule("1*", "*0")
            .AddRule("0*", "1", true)
            .AddRule("*", "1", true)
            .AddRule("$0", "0$").WithComment("Move right")
            .AddRule("$1", "1$").WithComment("Move right")
            .AddRule("$", "*").WithComment("Last digit")
            .AddRule("", "$").WithComment("Place marker")
            .Build();
    }
    
    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that subtracts 1 from given binary number.<br/>
    /// <b>- alphabet:</b> 0 and 1.<br/>
    /// <b>- outputs:</b> calculated binary number.
    /// </summary>
    public static MarkovAlgorithm Create_BinaryDecrement()
    {
        return MarkovAlgorithm.Create("Binary decrement")
            .WithAlphabet('0', '1')
            .WithMarkers('*', '$', '%')
            // decrement
            .AddRule("00*", "*11")
            .AddRule("10*", "*11")
            .AddRule("01*", "00", true)
            .AddRule("10*", "01", true)
            .AddRule("11*", "10", true)
            .AddRule("0*", "0", true)
            .AddRule("$0", "0$").WithComment("Move right")
            .AddRule("$1", "1$").WithComment("Move right")
            .AddRule("$", "*").WithComment("Last digit")
            //
            .AddRule("", "$").WithComment("Place marker")
            .Build();
    }
    
    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that removes all leading zeros except first.<br/>
    /// <b>- alphabet:</b> any symbol.<br/>
    /// <b>- example:</b> "000123" -> "1234" or "000" -> "0".
    /// </summary>
    public static MarkovAlgorithm Create_LeadingZerosTrim()
    {
        var builder = MarkovAlgorithm.Create("Binary decrement")
            .WithAlphabet("0123456789")
            .WithMarkers('|');
        
        for (var i = 1; i <= 9; i++)
            builder.AddRule($"|{i}", i.ToString(), true).WithComment("1st non-zero");
        
        return builder
            .AddRule("|0", "|")
            .AddRule("|", "0", true).WithComment("Input was zero")
            .AddRule("", "|").WithComment("Place marker")
            .Build();
    }
    
    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that converts unary number into binary.<br/>
    /// <b>- alphabet:</b> "|" for unary numbers, 0 and 1 for binary.<br/>
    /// <b>- example:</b> "|||||" (5 items) -> "101".
    /// </summary>
    public static MarkovAlgorithm Create_UnaryToBinaryConverter()
    {
        return MarkovAlgorithm.Create("Unary to binary number converter")
            .WithAlphabet("|01")
            .WithMarkers('#', '*')
            .AddRule("1#", "#0")
            .AddRule("0#", "1")
            .AddRule("#", "1")
            .AddRule("*|", "#*")
            .AddRule("$", "", true)
            .AddRule("", "0*")
            .Build();
    }
    
    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that converts unary number into ternary system.<br/>
    /// <b>- alphabet:</b> "|" for unary numbers, 0, 1 and 2 for ternary.<br/>
    /// <b>- example:</b> "|||||" (5 items) -> "12".
    /// </summary>
    public static MarkovAlgorithm Create_UnaryToTernaryConverter()
    {
        return MarkovAlgorithm.Create("Unary to ternary number converter")
            .WithAlphabet("|01")
            .WithMarkers('#', '*')
            .AddRule("2#", "#0")
            .AddRule("1#", "#0")
            .AddRule("0#", "1")
            .AddRule("#", "1")
            .AddRule("*|", "#*")
            .AddRule("$", "", true)
            .AddRule("", "0*")
            .Build();
    }
}