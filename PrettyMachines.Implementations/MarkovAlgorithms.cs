using PrettyMachines.Algorithms.Markov;
using PrettyMachines.Algorithms.Utils;


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
    /// <b>- outputs:</b> calculated binary number without leading zeros; zero stays zero (saturating).
    /// </summary>
    public static MarkovAlgorithm Create_BinaryDecrement()
    {
        return MarkovAlgorithm.Create("Binary decrement")
            .WithAlphabet('0', '1')
            .WithMarkers('#', '$', '*')
            // Walk to the least significant bit
            .AddRule("$0", "0$").WithComment("Move right")
            .AddRule("$1", "1$").WithComment("Move right")
            .AddRule("$", "*").WithComment("Reached the last digit")
            // Borrow through trailing zeros
            .AddRule("0*", "*1").WithComment("Borrow from the next digit")
            // Borrow reaches the leading one, so it is dropped
            .AddRule("#1*0", "#0").WithComment("Drop leading one")
            .AddRule("#1*1", "#1").WithComment("Drop leading one")
            .AddRule("#1*",  "#0").WithComment("Drop leading one")
            // Flip a one that is not leading
            .AddRule("1*", "0").WithComment("Flip one")
            // Underflow or zero input
            .AddRule("*1", "0").WithComment("Underflow, result is zero")
            .AddRule("*",  "0").WithComment("Input was zero")
            // Trim leading zeros and strip the sentinel
            .AddRule("#00", "#0").WithComment("Trim leading zero")
            .AddRule("#01", "#1").WithComment("Trim leading zero")
            .AddRule("#", "", true).WithComment("Strip sentinel")
            .AddRule("", "#$").WithComment("Place sentinel")
            .Build();
    }

    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that adds two binary numbers written as "A+B".<br/>
    /// <b>- alphabet:</b> 0, 1 and "+".<br/>
    /// <b>- outputs:</b> calculated binary sum without leading zeros.
    /// </summary>
    /// <remarks>Uses repeated decrement of the second operand and increment of the first.</remarks>
    public static MarkovAlgorithm Create_BinaryAddition()
    {
        return MarkovAlgorithm.Create("Binary addition")
            .WithAlphabet('0', '1', '+')
            .WithMarkers('#', '%', '&', '|', '$', '*', '@', '^', '!')
            // Place the left sentinel and the separator
            .AddRule("+", "%&").WithComment("Split operands")
            .AddRule("0%", "%0").WithComment("Move sentinel left")
            .AddRule("1%", "%1").WithComment("Move sentinel left")
            .AddRule("%", "#").WithComment("Sentinel reached the front")
            // Add the carry into the first operand
            .AddRule("0^", "1").WithComment("Carry stopped")
            .AddRule("1^", "^0").WithComment("Carry left")
            .AddRule("#^", "#1").WithComment("Carry out of the number")
            // Bring the borrow marker from the second operand to the first
            .AddRule("0@", "@0").WithComment("Move return marker left")
            .AddRule("1@", "@1").WithComment("Move return marker left")
            .AddRule("|@", "^|").WithComment("Reach the first operand")
            // Decrement the second operand
            .AddRule("0*", "*1").WithComment("Borrow from the next digit")
            .AddRule("1*", "@0").WithComment("Flip one and return")
            // Second operand is exhausted
            .AddRule("|*", "!").WithComment("Nothing left to add")
            .AddRule("!1", "!").WithComment("Erase second operand")
            .AddRule("!0", "!").WithComment("Erase second operand")
            .AddRule("!|", "!").WithComment("Erase separator")
            .AddRule("!&", "!").WithComment("Erase separator")
            .AddRule("!", "").WithComment("Erase tail")
            // Walk to the end of the second operand
            .AddRule("$0", "0$").WithComment("Move right")
            .AddRule("$1", "1$").WithComment("Move right")
            .AddRule("$", "*").WithComment("Reached the last digit")
            // Restart the loop
            .AddRule("|", "&").WithComment("Idle separator")
            .AddRule("&", "|$").WithComment("Start next iteration")
            .AddRule("#", "", true).WithComment("Strip sentinel")
            .Build();
    }

    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that subtracts two binary numbers written as "A-B".<br/>
    /// <b>- alphabet:</b> 0, 1 and "-".<br/>
    /// <b>- outputs:</b> calculated binary difference without leading zeros; zero for underflow (saturating).
    /// </summary>
    /// <remarks>Uses repeated decrement of both operands; saturates when the first operand reaches zero.</remarks>
    public static MarkovAlgorithm Create_BinarySubtraction()
    {
        return MarkovAlgorithm.Create("Binary subtraction")
            .WithAlphabet('0', '1', '-')
            .WithMarkers('#', '%', '&', '|', '$', '*', '@', 'v', '!')
            // Place the left sentinel and the separator
            .AddRule("-", "%&").WithComment("Split operands")
            .AddRule("0%", "%0").WithComment("Move sentinel left")
            .AddRule("1%", "%1").WithComment("Move sentinel left")
            .AddRule("%", "#").WithComment("Sentinel reached the front")
            // Subtract one from the first operand
            .AddRule("1v", "0").WithComment("Borrow stopped")
            .AddRule("0v", "v1").WithComment("Borrow from the next digit")
            .AddRule("#v", "#0!").WithComment("Underflow, result is zero")
            // Bring the borrow marker from the second operand to the first
            .AddRule("0@", "@0").WithComment("Move borrow marker left")
            .AddRule("1@", "@1").WithComment("Move borrow marker left")
            .AddRule("|@", "v|").WithComment("Reach the first operand")
            // Subtract one from the second operand
            .AddRule("0*", "*1").WithComment("Borrow from the next digit")
            .AddRule("1*", "@0").WithComment("Flip one and return")
            // Second operand is exhausted
            .AddRule("|*", "!").WithComment("Nothing left to subtract")
            .AddRule("!1", "!").WithComment("Erase second operand")
            .AddRule("!0", "!").WithComment("Erase second operand")
            .AddRule("!|", "!").WithComment("Erase separator")
            .AddRule("!&", "!").WithComment("Erase separator")
            .AddRule("!", "").WithComment("Erase tail")
            // Walk to the end of the second operand
            .AddRule("$0", "0$").WithComment("Move right")
            .AddRule("$1", "1$").WithComment("Move right")
            .AddRule("$", "*").WithComment("Reached the last digit")
            // Restart the loop
            .AddRule("|", "&").WithComment("Idle separator")
            .AddRule("&", "|$").WithComment("Start next iteration")
            // Trim leading zeros and strip the sentinel
            .AddRule("#00", "#0").WithComment("Trim leading zero")
            .AddRule("#01", "#1").WithComment("Trim leading zero")
            .AddRule("#", "", true).WithComment("Strip sentinel")
            .Build();
    }

    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that concatenates two strings written as "A+B".<br/>
    /// <b>- alphabet:</b> any symbol except "+".<br/>
    /// <b>- outputs:</b> concatenated string "AB".<br/>
    /// <b>- example:</b> "ab+cd" -> "abcd".
    /// </summary>
    public static MarkovAlgorithm Create_StringConcatenation()
    {
        return MarkovAlgorithm.Create("String concatenation")
            .WithMarkers('+')
            .AddRule("+", "", true).WithComment("Remove separator")
            .Build();
    }

    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that mirrors the observable behaviour of
    /// <see cref="TuringMachines.Create_BusyBeaver"/> on binary input.<br/>
    /// <b>- alphabet:</b> 0 and 1.<br/>
    /// <b>- outputs:</b> an empty string for empty input, the input unchanged when it starts with 1,
    /// and a same-length string of ones when it starts with 0.
    /// </summary>
    public static MarkovAlgorithm Create_BusyBeaver()
    {
        return MarkovAlgorithm.Create("Busy beaver")
            .WithAlphabet('0', '1')
            .WithMarkers('%', '#')
            .AddRule("%0", "1#").WithComment("Start filling with ones")
            .AddRule("#0", "1#").WithComment("Fill with ones")
            .AddRule("#1", "1#").WithComment("Skip existing ones")
            .AddRule("%1", "1", true).WithComment("Leading one stays")
            .AddRule("#", "", true).WithComment("Done")
            .AddRule("%", "", true).WithComment("Empty input")
            .AddRule("", "%").WithComment("Place marker")
            .Build();
    }

    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that converts binary number into unary.<br/>
    /// <b>- alphabet:</b> 0 and 1 for binary numbers, "|" for unary ones.<br/>
    /// <b>- outputs:</b> unary representation of the binary input; zero is an empty string.<br/>
    /// <b>- example:</b> "101" -> "|||||".
    /// </summary>
    /// <remarks>Uses repeated decrement of the binary number, appending one unary digit per step.</remarks>
    public static MarkovAlgorithm Create_BinaryToUnaryConverter()
    {
        return MarkovAlgorithm.Create("Binary to unary number converter")
            .WithAlphabet('0', '1')
            .WithMarkers('|', '#', '&', ';', '$', '*', '@', '!')
            // Walk to the least significant bit
            .AddRule("$0", "0$").WithComment("Move right")
            .AddRule("$1", "1$").WithComment("Move right")
            .AddRule("$", "*").WithComment("Reached the last digit")
            // Decrement the binary number
            .AddRule("0*", "*1").WithComment("Borrow from the next digit")
            .AddRule("1*", "@0").WithComment("Flip one and collect unary digit")
            .AddRule("0@", "@0").WithComment("Move collector left")
            .AddRule("1@", "@1").WithComment("Move collector left")
            .AddRule(";@", "|&").WithComment("Append one unary digit")
            // Number is exhausted
            .AddRule(";*", "!").WithComment("Nothing left to convert")
            .AddRule("!0", "!").WithComment("Erase binary number")
            .AddRule("!1", "!").WithComment("Erase binary number")
            .AddRule("!;", "!").WithComment("Erase separator")
            .AddRule("!&", "!").WithComment("Erase separator")
            .AddRule("!", "").WithComment("Erase tail")
            // Restart the loop
            .AddRule(";", "&").WithComment("Idle separator")
            .AddRule("&", ";$").WithComment("Start next iteration")
            .AddRule("#", "", true).WithComment("Strip sentinel")
            .AddRule("", "#&").WithComment("Place sentinel and separator")
            .Build();
    }

    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that converts decimal number into binary.<br/>
    /// <b>- alphabet:</b> decimal digits for the input, 0 and 1 for the binary output.<br/>
    /// <b>- outputs:</b> binary representation of the decimal input.<br/>
    /// <b>- example:</b> "5" -> "101" or "1000" -> "1111101000".
    /// </summary>
    /// <remarks>Converts the decimal number into unary by repeated decrement, then unary into binary.</remarks>
    public static MarkovAlgorithm Create_DecimalToBinaryConverter()
    {
        var builder = MarkovAlgorithm.Create("Decimal to binary number converter")
            .WithAlphabet("0123456789")
            .WithMarkers('|', '#', '&', ';', '$', '*', '@', '!', '^', '%');

        // Walk to the least significant decimal digit
        for (var digit = 0; digit <= 9; digit++)
            builder.AddRule($"${digit}", $"{digit}$").WithComment("Move right");
        builder.AddRule("$", "*").WithComment("Reached the last digit");

        // Decrement the decimal number
        builder.AddRule("0*", "*9").WithComment("Borrow from the next digit");
        for (var digit = 1; digit <= 9; digit++)
            builder.AddRule($"{digit}*", $"@{digit - 1}").WithComment("Decrement and collect unary digit");

        // Move the collector to the unary part
        for (var digit = 0; digit <= 9; digit++)
            builder.AddRule($"{digit}@", $"@{digit}").WithComment("Move collector left");
        builder.AddRule(";@", "|&").WithComment("Append one unary digit");

        // Number is exhausted
        builder.AddRule(";*", "!").WithComment("Nothing left to convert");
        for (var digit = 0; digit <= 9; digit++)
            builder.AddRule($"!{digit}", "!").WithComment("Erase decimal number");
        builder.AddRule("!;", "!").WithComment("Erase separator");
        builder.AddRule("!&", "!").WithComment("Erase separator");
        builder.AddRule("!", "").WithComment("Erase tail");

        // Restart the loop
        builder.AddRule(";", "&").WithComment("Idle separator");
        builder.AddRule("&", ";$").WithComment("Start next iteration");

        // Convert the collected unary number into binary
        builder.AddRule("#", "0%").WithComment("Place binary accumulator");
        builder.AddRule("0^", "1").WithComment("Add one: 0 -> 1");
        builder.AddRule("1^", "^0").WithComment("Carry left: 1 -> 0");
        builder.AddRule("^0", "10").WithComment("Carry out of the number");
        builder.AddRule("^%", "1%").WithComment("Carry out of empty number");
        builder.AddRule("%|", "^%").WithComment("Consume one unary digit");
        builder.AddRule("%", "", true).WithComment("Strip separator");
        builder.AddRule("", "#&").WithComment("Place sentinel and separator");

        return builder.Build();
    }

    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that converts binary number into decimal.<br/>
    /// <b>- alphabet:</b> 0 and 1 for the input, decimal digits for the output.<br/>
    /// <b>- outputs:</b> decimal representation of the binary input.<br/>
    /// <b>- example:</b> "101" -> "5" or "1111101000" -> "1000".
    /// </summary>
    /// <remarks>Repeatedly decrements the binary number and increments a decimal accumulator.</remarks>
    public static MarkovAlgorithm Create_BinaryToDecimalConverter()
    {
        var builder = MarkovAlgorithm.Create("Binary to decimal number converter")
            .WithAlphabet("0123456789")
            .WithMarkers('#', '&', ';', '$', '*', '@', '^', '!');

        // Walk to the least significant bit
        builder.AddRule("$0", "0$").WithComment("Move right");
        builder.AddRule("$1", "1$").WithComment("Move right");
        builder.AddRule("$", "*").WithComment("Reached the last digit");

        // Decrement the binary number
        builder.AddRule("0*", "*1").WithComment("Borrow from the next digit");
        builder.AddRule("1*", "@0").WithComment("Decrement and collect");
        builder.AddRule("0@", "@0").WithComment("Move collector left");
        builder.AddRule("1@", "@1").WithComment("Move collector left");
        builder.AddRule(";@", "^;").WithComment("Carry into the decimal number");

        // Increment the decimal number
        for (var digit = 0; digit <= 8; digit++)
            builder.AddRule($"{digit}^", (digit + 1).ToString()).WithComment("Add one");
        builder.AddRule("9^", "^0").WithComment("Carry left");
        builder.AddRule("#^", "#1").WithComment("Carry out of the number");

        // Binary number is exhausted
        builder.AddRule(";*", "!").WithComment("Nothing left to convert");
        builder.AddRule("!0", "!").WithComment("Erase binary number");
        builder.AddRule("!1", "!").WithComment("Erase binary number");
        builder.AddRule("!;", "!").WithComment("Erase separator");
        builder.AddRule("!&", "!").WithComment("Erase separator");
        builder.AddRule("!", "").WithComment("Erase tail");

        // Restart the loop
        builder.AddRule(";", "&").WithComment("Idle separator");
        builder.AddRule("&", ";$").WithComment("Start next iteration");
        builder.AddRule("#", "", true).WithComment("Strip sentinel");
        builder.AddRule("", "#0&").WithComment("Place accumulator and separator");

        return builder.Build();
    }

    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that reverses a string.<br/>
    /// <b>- alphabet:</b> <paramref name="alphabet"/> (lowercase Latin letters by default).<br/>
    /// <b>- outputs:</b> the input string in reverse order.<br/>
    /// <b>- example:</b> "abc" -> "cba".
    /// </summary>
    /// <remarks>Moves the first character of the remaining input to the left of the accumulated result.</remarks>
    public static MarkovAlgorithm Create_StringReversal(IEnumerable<char>? alphabet = null)
    {
        var symbols = (alphabet ?? "abcdefghijklmnopqrstuvwxyz").Distinct().ToArray();
        if (symbols.Length == 0)
            throw new ArgumentException("Alphabet cannot be empty.", nameof(alphabet));

        var sentinel = '\uE000';
        var markers = new char[symbols.Length];
        var candidate = '\uE001';
        for (var i = 0; i < markers.Length; i++)
        {
            while (symbols.Contains(candidate))
                candidate++;
            markers[i] = candidate++;
        }

        var builder = MarkovAlgorithm.Create("String reversal")
            .WithAlphabet(symbols)
            .WithMarkers([sentinel, ..markers]);

        // Move a marked character left of the sentinel
        for (var i = 0; i < symbols.Length; i++)
            builder.AddRule($"{sentinel}{markers[i]}", $"{markers[i]}{sentinel}").WithComment("Cross sentinel");
        for (var i = 0; i < symbols.Length; i++)
            for (var j = 0; j < symbols.Length; j++)
                builder.AddRule($"{symbols[j]}{markers[i]}", $"{markers[i]}{symbols[j]}").WithComment("Move character left");
        for (var i = 0; i < symbols.Length; i++)
            builder.AddRule(markers[i].ToString(), symbols[i].ToString()).WithComment("Append to the front");

        // Mark the first character of the remaining input
        for (var i = 0; i < symbols.Length; i++)
            builder.AddRule($"{sentinel}{symbols[i]}", $"{sentinel}{markers[i]}").WithComment("Mark first character");

        builder.AddRule(sentinel.ToString(), "", true).WithComment("Strip sentinel");
        builder.AddRule("", sentinel.ToString()).WithComment("Place sentinel");

        return builder.Build();
    }

    /// <summary>
    /// Creates <see cref="MarkovAlgorithm"/> that removes all leading zeros except first.<br/>
    /// <b>- alphabet:</b> any symbol.<br/>
    /// <b>- example:</b> "000123" -> "1234" or "000" -> "0".
    /// </summary>
    public static MarkovAlgorithm Create_LeadingZerosTrim()
    {
        var builder = MarkovAlgorithm.Create("Leading zeros trim")
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
            .WithAlphabet('|')
            .WithMarkers('0', '1', '#', '^')
            .AddRule("0^", "1").WithComment("Add one: 0 -> 1")
            .AddRule("1^", "^0").WithComment("Carry left: 1 -> 0")
            .AddRule("^0", "10").WithComment("Carry out of the number")
            .AddRule("^#", "1#").WithComment("Carry out of empty number")
            .AddRule("#|", "^#").WithComment("Consume one unary digit")
            .AddRule("#", "", true).WithComment("Strip separator")
            .AddRule("", "0#").WithComment("Place accumulator")
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
            .WithAlphabet('|')
            .WithMarkers('0', '1', '2', '#', '^')
            .AddRule("0^", "1").WithComment("Add one: 0 -> 1")
            .AddRule("1^", "2").WithComment("Add one: 1 -> 2")
            .AddRule("2^", "^0").WithComment("Carry left: 2 -> 0")
            .AddRule("^0", "10").WithComment("Carry out of the number")
            .AddRule("^#", "1#").WithComment("Carry out of empty number")
            .AddRule("#|", "^#").WithComment("Consume one unary digit")
            .AddRule("#", "", true).WithComment("Strip separator")
            .AddRule("", "0#").WithComment("Place accumulator")
            .Build();
    }
}