using PrettyMachines.Turing;


namespace PrettyMachines.Implementations;

/// <summary>
/// Predefined Turing machines.
/// </summary>
public static class TuringMachines
{
    /// <summary>
    /// Creates <see cref="TuringMachine"/> that recognizes correct brackets sequences.<br/>
    /// <b>- alphabet:</b> opening and closing brackets of one type from <paramref name="symbols"/>.<br/>
    /// <b>- outputs:</b> <see cref="BracketsGrammarSymbols.Accepted"/> or <see cref="BracketsGrammarSymbols.Rejected"/> from <paramref name="symbols"/>.
    /// </summary>
    public static TuringMachine Create_BracketsGrammar(BracketsGrammarSymbols? symbols = null)
    {
        symbols ??= new BracketsGrammarSymbols();
        symbols.Validate();

        var left = symbols.Left.ToString();
        var right = symbols.Right.ToString();
        var accepted = symbols.Accepted.ToString();
        var rejected = symbols.Rejected.ToString();
        var mark = symbols.Marked.ToString();
        var empty = "_";
        
        return TuringMachine.Create("Brackets grammar")
            .WithAlphabet("(", ")", "x", accepted, rejected)
            .WithBlankSymbol(empty)
            .AddInitialState("Find opening", out var q0)
            .AddState(out var q1)
            .AddState(out var q2)
            .AddState(out var q3)
            .AddTerminalState("Accepted", out var qA)
            .AddTerminalState("Rejected", out var qR)
            .BuildRules(builder => builder
                .AddRule(q0, "(",               q1, mark, TapeMovement.Right)
                .AddRule(q0, ")",               qR)
                .AddRule(q0, mark,              q0, null, TapeMovement.Right)
                .AddRule(q0, SymbolMatch.Empty, q3, null, TapeMovement.Left)
                
                .AddRule(q1, mark,              q1, null, TapeMovement.Right)
                .AddRule(q1, "(",               q1, null, TapeMovement.Right)
                .AddRule(q1, ")",               q1, mark, TapeMovement.Left)
                .AddRule(q1, SymbolMatch.Empty, qR)
                
                .AddRule(q2, mark,              q2, null, TapeMovement.Left)
                .AddRule(q2, "(",               q1, null, TapeMovement.Left)
                .AddRule(q2, ")",               q1, mark, TapeMovement.Right)
                .AddRule(q2, SymbolMatch.Empty, q0, null, TapeMovement.Right)
                
                .AddRule(q3, mark,                 q3, empty, TapeMovement.Left)
                .AddRule(q3, SymbolMatch.NotEmpty, qR)
                .AddRule(q3, SymbolMatch.Empty,    qA)
            );
    }
    
    /// <summary>
    /// Creates <see cref="TuringMachine"/> that adds 1 to given binary number.<br/>
    /// <b>- alphabet:</b> 0 and 1.<br/>
    /// <b>- outputs:</b> calculated binary number without leading zeros.
    /// </summary>
    public static TuringMachine Create_BinaryIncrementMachine()
    {
        return TuringMachine.Create("Binary increment")
            .WithAlphabet("0", "1")
            .WithBlankSymbol("_")
            .AddInitialState("Scan to LSB", out var q0)
            .AddState("Propagate carry", out var q1)
            .BuildRules(builder => builder
                .AddRule(q0, SymbolMatch.NotEmpty, q0, null, TapeMovement.Right)
                .AddRule(q0, SymbolMatch.Empty,    q1, null, TapeMovement.Left)
                .AddHalt(q1, "0",          "1")
                .AddRule(q1, "1",          q1, "0", TapeMovement.Left)
                .AddHalt(q1, SymbolMatch.Empty, "1")
            );
    }

    /// <summary>
    /// Creates <see cref="TuringMachine"/> that subtracts 1 from given binary number.<br/>
    /// <b>- alphabet:</b> 0 and 1.<br/>
    /// <b>- outputs:</b> calculated binary number without leading zeros; zero stays zero (saturating).
    /// </summary>
    public static TuringMachine Create_BinaryDecrementMachine()
    {
        const string empty = "_";

        return TuringMachine.Create("Binary decrement")
            .WithAlphabet("0", "1")
            .WithBlankSymbol(empty)
            .AddInitialState("Scan to LSB", out var q0)
            .AddState("Find borrow", out var q1)
            .AddState("Fill ones", out var q2)
            .AddState("Rewind", out var q3)
            .AddState("Trim zeros", out var q4)
            .BuildRules(builder => builder
                .AddRule(q0, SymbolMatch.NotEmpty, q0, null, TapeMovement.Right)
                .AddRule(q0, SymbolMatch.Empty,    q1, null, TapeMovement.Left)

                .AddRule(q1, "0", q1, null, TapeMovement.Left)
                .AddRule(q1, "1", q2, "0", TapeMovement.Right)
                .AddHalt(q1, SymbolMatch.Empty)

                .AddRule(q2, "0", q2, "1", TapeMovement.Right)
                .AddRule(q2, SymbolMatch.Empty, q3, null, TapeMovement.Left)

                .AddRule(q3, SymbolMatch.NotEmpty, q3, null, TapeMovement.Left)
                .AddRule(q3, SymbolMatch.Empty,    q4, null, TapeMovement.Right)

                .AddRule(q4, "0", q4, empty, TapeMovement.Right)
                .AddHalt(q4, "1")
                .AddHalt(q4, SymbolMatch.Empty, "0")
            );
    }

    /// <summary>
    /// Creates <see cref="TuringMachine"/> that adds two binary numbers written as "A+B".<br/>
    /// <b>- alphabet:</b> 0, 1 and "+".<br/>
    /// <b>- outputs:</b> calculated binary sum without leading zeros.
    /// </summary>
    /// <remarks>Uses repeated decrement of the second operand and increment of the first.</remarks>
    public static TuringMachine Create_BinaryAdditionMachine()
    {
        const string empty = "_";

        return TuringMachine.Create("Binary addition")
            .WithAlphabet("0", "1", "+")
            .WithBlankSymbol(empty)
            .AddInitialState("Seek second operand", out var q0)
            .AddState("Test second operand", out var q1)
            .AddState("Scan to its end", out var q2)
            .AddState("Decrement second", out var q3)
            .AddState("Go to first", out var q4)
            .AddState("Increment first", out var q5)
            .AddState("Erase second", out var q6)
            .BuildRules(builder => builder
                .AddRule(q0, "0", q0, null, TapeMovement.Right)
                .AddRule(q0, "1", q0, null, TapeMovement.Right)
                .AddRule(q0, "+", q1, null, TapeMovement.Right)

                .AddRule(q1, "0", q1, null, TapeMovement.Right)
                .AddRule(q1, "1", q2, null, TapeMovement.Right)
                .AddRule(q1, SymbolMatch.Empty, q6, null, TapeMovement.Left)

                .AddRule(q2, "0", q2, null, TapeMovement.Right)
                .AddRule(q2, "1", q2, null, TapeMovement.Right)
                .AddRule(q2, SymbolMatch.Empty, q3, null, TapeMovement.Left)

                .AddRule(q3, "1", q4, "0", TapeMovement.Left)
                .AddRule(q3, "0", q3, "1", TapeMovement.Left)

                .AddRule(q4, "0", q4, null, TapeMovement.Left)
                .AddRule(q4, "1", q4, null, TapeMovement.Left)
                .AddRule(q4, "+", q5, null, TapeMovement.Left)

                .AddRule(q5, "1", q5, "0", TapeMovement.Left)
                .AddRule(q5, "0", q0, "1")
                .AddRule(q5, SymbolMatch.Empty, q0, "1")

                .AddRule(q6, "0", q6, empty, TapeMovement.Left)
                .AddRule(q6, "1", q6, empty, TapeMovement.Left)
                .AddHalt(q6, "+", empty, TapeMovement.Left)
            );
    }

    /// <summary>
    /// Creates <see cref="TuringMachine"/> that subtracts two binary numbers written as "A-B".<br/>
    /// <b>- alphabet:</b> 0, 1 and "-".<br/>
    /// <b>- outputs:</b> calculated binary difference without leading zeros; zero for underflow (saturating).
    /// </summary>
    /// <remarks>Uses repeated decrement of both operands; saturates when the first operand reaches zero.</remarks>
    public static TuringMachine Create_BinarySubtractionMachine()
    {
        const string empty = "_";

        return TuringMachine.Create("Binary subtraction")
            .WithAlphabet("0", "1", "-")
            .WithBlankSymbol(empty)
            .AddInitialState("Seek second operand", out var q0)
            .AddState("Test second operand", out var q1)
            .AddState("Scan to its end", out var q2)
            .AddState("Decrement second", out var q3)
            .AddState("Go to first", out var q4)
            .AddState("Test first operand", out var q5)
            .AddState("Scan first operand", out var q6)
            .AddState("Return to its end", out var q7)
            .AddState("Decrement first", out var q8)
            .AddState("Erase second", out var q9)
            .AddState("Saturate", out var q10)
            .AddState("Rewind", out var q11)
            .AddState("Trim zeros", out var q12)
            .BuildRules(builder => builder
                .AddRule(q0, "0", q0, null, TapeMovement.Right)
                .AddRule(q0, "1", q0, null, TapeMovement.Right)
                .AddRule(q0, "-", q1, null, TapeMovement.Right)

                .AddRule(q1, "0", q1, null, TapeMovement.Right)
                .AddRule(q1, "1", q2, null, TapeMovement.Right)
                .AddRule(q1, SymbolMatch.Empty, q9, null, TapeMovement.Left)

                .AddRule(q2, "0", q2, null, TapeMovement.Right)
                .AddRule(q2, "1", q2, null, TapeMovement.Right)
                .AddRule(q2, SymbolMatch.Empty, q3, null, TapeMovement.Left)

                .AddRule(q3, "1", q4, "0", TapeMovement.Left)
                .AddRule(q3, "0", q3, "1", TapeMovement.Left)

                .AddRule(q4, "0", q4, null, TapeMovement.Left)
                .AddRule(q4, "1", q4, null, TapeMovement.Left)
                .AddRule(q4, "-", q5, null, TapeMovement.Left)

                .AddRule(q5, "1", q8)
                .AddRule(q5, "0", q6, null, TapeMovement.Left)

                .AddRule(q6, "0", q6, null, TapeMovement.Left)
                .AddRule(q6, "1", q7)
                .AddRule(q6, SymbolMatch.Empty, q10, "0", TapeMovement.Right)

                .AddRule(q7, "0", q7, null, TapeMovement.Right)
                .AddRule(q7, "1", q7, null, TapeMovement.Right)
                .AddRule(q7, "-", q8, null, TapeMovement.Left)

                .AddRule(q8, "1", q0, "0")
                .AddRule(q8, "0", q8, "1", TapeMovement.Left)

                .AddRule(q9, "0", q9, empty, TapeMovement.Left)
                .AddRule(q9, "1", q9, empty, TapeMovement.Left)
                .AddRule(q9, "-", q11, empty, TapeMovement.Left)

                .AddRule(q11, SymbolMatch.NotEmpty, q11, null, TapeMovement.Left)
                .AddRule(q11, SymbolMatch.Empty, q12, null, TapeMovement.Right)

                .AddRule(q12, "0", q12, empty, TapeMovement.Right)
                .AddHalt(q12, "1")
                .AddHalt(q12, SymbolMatch.Empty, "0")

                .AddRule(q10, SymbolMatch.NotEmpty, q10, empty, TapeMovement.Right)
                .AddHalt(q10, SymbolMatch.Empty)
            );
    }

    /// <summary>
    /// Creates <see cref="TuringMachine"/> that concatenates two strings written as "A+B".<br/>
    /// <b>- alphabet:</b> any symbol except the blank one (the blank symbol is the empty string).<br/>
    /// <b>- outputs:</b> concatenated string "AB".<br/>
    /// <b>- example:</b> "ab+cd" -> "abcd".
    /// </summary>
    public static TuringMachine Create_StringConcatenationMachine()
    {
        return TuringMachine.Create("String concatenation")
            .WithBlankSymbol("")
            .AddInitialState("Find separator", out var q0)
            .AddTerminalState("Done", out var q1)
            .BuildRules(builder => builder
                .AddRule(q0, SymbolMatch.NotEmpty, q0, null, TapeMovement.Right)
                .AddRule(q0, "+", q1, "")
            );
    }

    /// <summary>
    /// Creates <see cref="TuringMachine"/> that converts unary number into binary.<br/>
    /// <b>- alphabet:</b> "|" for unary numbers, 0 and 1 for binary.<br/>
    /// <b>- outputs:</b> binary representation of the unary input.<br/>
    /// <b>- example:</b> "|||||" (5 items) -> "101".
    /// </summary>
    public static TuringMachine Create_UnaryToBinaryConverterMachine()
    {
        const string empty = "_";
        const string separator = "#";

        return TuringMachine.Create("Unary to binary number converter")
            .WithAlphabet("|", "0", "1", separator)
            .WithBlankSymbol(empty)
            .AddInitialState("Go left", out var q0)
            .AddState("Write separator", out var q1)
            .AddState("Write zero", out var q2)
            .AddState("Check next", out var q3)
            .AddState("Consume unary", out var q4)
            .AddState("Shift separator", out var q5)
            .AddState("Skip gap", out var q6)
            .AddState("Increment", out var qInc)
            .AddState("Return", out var qReturn)
            .AddState("Cleanup", out var qClean)
            .BuildRules(builder => builder
                // Build "0#|...|" to the left of the input
                .AddRule(q0, SymbolMatch.Any, q1, null, TapeMovement.Left)
                .AddRule(q1, SymbolMatch.Any, q2, separator, TapeMovement.Left)
                .AddRule(q2, SymbolMatch.Any, q3, "0", TapeMovement.Right)
                // Grab one unary symbol
                .AddRule(q3, separator, q4, null, TapeMovement.Right)
                .AddRule(q4, "|", q5, separator, TapeMovement.Left)
                .AddRule(q4, SymbolMatch.Empty, qClean, null, TapeMovement.Left)
                .AddRule(q5, separator, q6, empty, TapeMovement.Left)
                // Step over the gap back to the least significant bit
                .AddRule(q6, SymbolMatch.Empty, q6, null, TapeMovement.Left)
                .AddRule(q6, "0", qInc)
                .AddRule(q6, "1", qInc)
                // Binary increment
                .AddRule(qInc, "0", qReturn, "1", TapeMovement.Right)
                .AddRule(qInc, "1", qInc, "0", TapeMovement.Left)
                .AddRule(qInc, SymbolMatch.Empty, qReturn, "1", TapeMovement.Right)
                // Return to the separator
                .AddRule(qReturn, SymbolMatch.Empty, qReturn, null, TapeMovement.Right)
                .AddRule(qReturn, "0", qReturn, null, TapeMovement.Right)
                .AddRule(qReturn, "1", qReturn, null, TapeMovement.Right)
                .AddRule(qReturn, separator, q3)
                // Strip the separator
                .AddHalt(qClean, separator, empty)
            );
    }

    /// <summary>
    /// Creates <see cref="TuringMachine"/> that converts unary number into ternary.<br/>
    /// <b>- alphabet:</b> "|" for unary numbers, 0, 1 and 2 for ternary.<br/>
    /// <b>- outputs:</b> ternary representation of the unary input.<br/>
    /// <b>- example:</b> "|||||" (5 items) -> "12".
    /// </summary>
    public static TuringMachine Create_UnaryToTernaryConverterMachine()
    {
        const string empty = "_";
        const string separator = "#";

        return TuringMachine.Create("Unary to ternary number converter")
            .WithAlphabet("|", "0", "1", "2", separator)
            .WithBlankSymbol(empty)
            .AddInitialState("Go left", out var q0)
            .AddState("Write separator", out var q1)
            .AddState("Write zero", out var q2)
            .AddState("Check next", out var q3)
            .AddState("Consume unary", out var q4)
            .AddState("Shift separator", out var q5)
            .AddState("Skip gap", out var q6)
            .AddState("Increment", out var qInc)
            .AddState("Return", out var qReturn)
            .AddState("Cleanup", out var qClean)
            .BuildRules(builder => builder
                // Build "0#|...|" to the left of the input
                .AddRule(q0, SymbolMatch.Any, q1, null, TapeMovement.Left)
                .AddRule(q1, SymbolMatch.Any, q2, separator, TapeMovement.Left)
                .AddRule(q2, SymbolMatch.Any, q3, "0", TapeMovement.Right)
                // Grab one unary symbol
                .AddRule(q3, separator, q4, null, TapeMovement.Right)
                .AddRule(q4, "|", q5, separator, TapeMovement.Left)
                .AddRule(q4, SymbolMatch.Empty, qClean, null, TapeMovement.Left)
                .AddRule(q5, separator, q6, empty, TapeMovement.Left)
                // Step over the gap back to the least significant digit
                .AddRule(q6, SymbolMatch.Empty, q6, null, TapeMovement.Left)
                .AddRule(q6, "0", qInc)
                .AddRule(q6, "1", qInc)
                .AddRule(q6, "2", qInc)
                // Ternary increment
                .AddRule(qInc, "0", qReturn, "1", TapeMovement.Right)
                .AddRule(qInc, "1", qReturn, "2", TapeMovement.Right)
                .AddRule(qInc, "2", qInc, "0", TapeMovement.Left)
                .AddRule(qInc, SymbolMatch.Empty, qReturn, "1", TapeMovement.Right)
                // Return to the separator
                .AddRule(qReturn, SymbolMatch.Empty, qReturn, null, TapeMovement.Right)
                .AddRule(qReturn, "0", qReturn, null, TapeMovement.Right)
                .AddRule(qReturn, "1", qReturn, null, TapeMovement.Right)
                .AddRule(qReturn, "2", qReturn, null, TapeMovement.Right)
                .AddRule(qReturn, separator, q3)
                // Strip the separator
                .AddHalt(qClean, separator, empty)
            );
    }

    /// <summary>
    /// Creates <see cref="TuringMachine"/> that converts binary number into unary.<br/>
    /// <b>- alphabet:</b> 0 and 1 for binary numbers, "|" for unary ones.<br/>
    /// <b>- outputs:</b> unary representation of the binary input; zero is an empty string.<br/>
    /// <b>- example:</b> "101" -> "|||||".
    /// </summary>
    /// <remarks>Uses repeated decrement of the binary number, appending one unary digit per step.</remarks>
    public static TuringMachine Create_BinaryToUnaryConverterMachine()
    {
        const string empty = "_";

        return TuringMachine.Create("Binary to unary number converter")
            .WithAlphabet("0", "1", "|", "#")
            .WithBlankSymbol(empty)
            .AddInitialState("Go left", out var q0)
            .AddState("Write sentinel", out var q1)
            .AddState("Find LSB", out var qFind)
            .AddState("Borrow", out var qBorrow)
            .AddState("Append unary", out var qAppend)
            .AddState("Restart", out var qRestart)
            .AddState("Cleanup", out var qClean)
            .AddState("Cleanup digits", out var qClean2)
            .BuildRules(builder => builder
                // Place a left sentinel before the number
                .AddRule(q0, SymbolMatch.Any, q1, null, TapeMovement.Left)
                .AddRule(q1, SymbolMatch.Any, qFind, "#", TapeMovement.Right)
                // Walk right to the least significant bit
                .AddRule(qFind, "0", qFind, null, TapeMovement.Right)
                .AddRule(qFind, "1", qFind, null, TapeMovement.Right)
                .AddRule(qFind, "|", qBorrow, null, TapeMovement.Left)
                .AddRule(qFind, SymbolMatch.Empty, qBorrow, null, TapeMovement.Left)
                // Subtract one from the binary number
                .AddRule(qBorrow, "1", qAppend, "0", TapeMovement.Right)
                .AddRule(qBorrow, "0", qBorrow, "1", TapeMovement.Left)
                .AddRule(qBorrow, "#", qClean)
                // Append one unary digit at the far right
                .AddRule(qAppend, SymbolMatch.NotEmpty, qAppend, null, TapeMovement.Right)
                .AddRule(qAppend, SymbolMatch.Empty, qRestart, "|", TapeMovement.Left)
                // Return to the new least significant bit
                .AddRule(qRestart, "|", qRestart, null, TapeMovement.Left)
                .AddRule(qRestart, "0", qBorrow)
                .AddRule(qRestart, "1", qBorrow)
                .AddRule(qRestart, "#", qClean)
                // Erase the sentinel and the binary digits, keeping the unary result
                .AddRule(qClean, "#", qClean2, empty, TapeMovement.Right)
                .AddRule(qClean2, "0", qClean2, empty, TapeMovement.Right)
                .AddRule(qClean2, "1", qClean2, empty, TapeMovement.Right)
                .AddHalt(qClean2, "|")
                .AddHalt(qClean2, SymbolMatch.Empty)
            );
    }

    /// <summary>
    /// Creates <see cref="TuringMachine"/> that converts decimal number into binary.<br/>
    /// <b>- alphabet:</b> decimal digits for the input, 0 and 1 for the binary output.<br/>
    /// <b>- outputs:</b> binary representation of the decimal input.<br/>
    /// <b>- example:</b> "5" -> "101" or "1000" -> "1111101000".
    /// </summary>
    /// <remarks>Repeatedly decrements the decimal number and increments a binary accumulator.</remarks>
    public static TuringMachine Create_DecimalToBinaryConverterMachine()
    {
        const string empty = "_";
        const string separator = "&";

        var builder = TuringMachine.Create("Decimal to binary number converter")
            .WithAlphabet("0", "1", "2", "3", "4", "5", "6", "7", "8", "9", separator)
            .WithBlankSymbol(empty)
            .AddInitialState("Go left", out var q0)
            .AddState("Write separator", out var q1)
            .AddState("Write zero", out var q2)
            .AddState("Seek decimal", out var q3)
            .AddState("Walk decimal", out var q4)
            .AddState("Decrement decimal", out var q5)
            .AddState("Return to binary", out var qBack)
            .AddState("Increment binary", out var qInc)
            .AddState("Cleanup", out var qClean)
            .AddState("Cleanup digits", out var qClean2);

        var machine = builder.BuildRules(b => 
        {
            b.AddRule(q0, SymbolMatch.Any, q1, null, TapeMovement.Left)
             .AddRule(q1, SymbolMatch.Any, q2, separator, TapeMovement.Left)
             .AddRule(q2, SymbolMatch.Any, q3, "0", TapeMovement.Right);

            // Move to the decimal number and walk to its last digit
            b.AddRule(q3, "0", q3, null, TapeMovement.Right)
             .AddRule(q3, "1", q3, null, TapeMovement.Right)
             .AddRule(q3, separator, q4, null, TapeMovement.Right);
            for (var digit = 0; digit <= 9; digit++)
                b.AddRule(q4, digit.ToString(), q4, null, TapeMovement.Right);
            b.AddRule(q4, SymbolMatch.Empty, q5, null, TapeMovement.Left);

            // Subtract one from the decimal number
            for (var digit = 1; digit <= 9; digit++)
                b.AddRule(q5, digit.ToString(), qBack, (digit - 1).ToString(), TapeMovement.Left);
            b.AddRule(q5, "0", q5, "9", TapeMovement.Left);
            b.AddRule(q5, separator, qClean);

            // Return to the least significant bit of the binary number
            for (var digit = 0; digit <= 9; digit++)
                b.AddRule(qBack, digit.ToString(), qBack, null, TapeMovement.Left);
            b.AddRule(qBack, separator, qInc, null, TapeMovement.Left);

            // Add one to the binary number
            b.AddRule(qInc, "0", q3, "1")
             .AddRule(qInc, "1", qInc, "0", TapeMovement.Left)
             .AddRule(qInc, SymbolMatch.Empty, q3, "1");

            // Erase the separator and the decimal number
            b.AddRule(qClean, separator, qClean2, empty, TapeMovement.Right);
            for (var digit = 0; digit <= 9; digit++)
                b.AddRule(qClean2, digit.ToString(), qClean2, empty, TapeMovement.Right);
            b.AddHalt(qClean2, SymbolMatch.Empty);
        });

        return machine;
    }

    /// <summary>
    /// Creates <see cref="TuringMachine"/> that converts binary number into decimal.<br/>
    /// <b>- alphabet:</b> 0 and 1 for the input, decimal digits for the output.<br/>
    /// <b>- outputs:</b> decimal representation of the binary input.<br/>
    /// <b>- example:</b> "101" -> "5" or "1111101000" -> "1000".
    /// </summary>
    /// <remarks>Repeatedly decrements the binary number and increments a decimal accumulator.</remarks>
    public static TuringMachine Create_BinaryToDecimalConverterMachine()
    {
        const string empty = "_";
        const string separator = "&";

        var builder = TuringMachine.Create("Binary to decimal number converter")
            .WithAlphabet("0", "1", "2", "3", "4", "5", "6", "7", "8", "9", separator)
            .WithBlankSymbol(empty)
            .AddInitialState("Go left", out var q0)
            .AddState("Write separator", out var q1)
            .AddState("Write zero", out var q2)
            .AddState("Seek binary", out var q3)
            .AddState("Walk binary", out var q4)
            .AddState("Decrement binary", out var q5)
            .AddState("Return to decimal", out var qBack)
            .AddState("Increment decimal", out var qInc)
            .AddState("Cleanup", out var qClean)
            .AddState("Cleanup digits", out var qClean2);

        var machine = builder.BuildRules(b =>
        {
            b.AddRule(q0, SymbolMatch.Any, q1, null, TapeMovement.Left)
             .AddRule(q1, SymbolMatch.Any, q2, separator, TapeMovement.Left)
             .AddRule(q2, SymbolMatch.Any, q3, "0", TapeMovement.Right);

            // Move to the binary number and walk to its last digit
            for (var digit = 0; digit <= 9; digit++)
                b.AddRule(q3, digit.ToString(), q3, null, TapeMovement.Right);
            b.AddRule(q3, separator, q4, null, TapeMovement.Right);
            b.AddRule(q4, "0", q4, null, TapeMovement.Right)
             .AddRule(q4, "1", q4, null, TapeMovement.Right);
            b.AddRule(q4, SymbolMatch.Empty, q5, null, TapeMovement.Left);

            // Subtract one from the binary number
            b.AddRule(q5, "1", qBack, "0", TapeMovement.Left)
             .AddRule(q5, "0", q5, "1", TapeMovement.Left)
             .AddRule(q5, separator, qClean);

            // Return to the least significant decimal digit
            b.AddRule(qBack, "0", qBack, null, TapeMovement.Left)
             .AddRule(qBack, "1", qBack, null, TapeMovement.Left)
             .AddRule(qBack, separator, qInc, null, TapeMovement.Left);

            // Add one to the decimal number
            for (var digit = 0; digit <= 8; digit++)
                b.AddRule(qInc, digit.ToString(), q3, (digit + 1).ToString());
            b.AddRule(qInc, "9", qInc, "0", TapeMovement.Left);
            b.AddRule(qInc, SymbolMatch.Empty, q3, "1");

            // Erase the separator and the binary number
            b.AddRule(qClean, separator, qClean2, empty, TapeMovement.Right);
            b.AddRule(qClean2, "0", qClean2, empty, TapeMovement.Right)
             .AddRule(qClean2, "1", qClean2, empty, TapeMovement.Right);
            b.AddHalt(qClean2, SymbolMatch.Empty);
        });

        return machine;
    }

    /// <summary>
    /// Creates <see cref="TuringMachine"/> that reverses a string.<br/>
    /// <b>- alphabet:</b> <paramref name="alphabet"/> (lowercase Latin letters by default).<br/>
    /// <b>- outputs:</b> the input string in reverse order.<br/>
    /// <b>- example:</b> "abc" -> "cba".
    /// </summary>
    /// <remarks>Moves the first character of the remaining input to the left of the accumulated result.</remarks>
    public static TuringMachine Create_StringReversalMachine(IEnumerable<char>? alphabet = null)
    {
        const string empty = "_";

        var symbols = (alphabet ?? "abcdefghijklmnopqrstuvwxyz").Distinct().Select(c => c.ToString()).ToArray();
        if (symbols.Length == 0)
            throw new ArgumentException("Alphabet cannot be empty.", nameof(alphabet));

        var reserved = Enumerable.Range(0xE100, 0x100).Select(i => ((char)i).ToString())
            .Where(c => !symbols.Contains(c)).ToArray();
        var sentinel = reserved[0];
        var processed = reserved[1];

        var builder = TuringMachine.Create("String reversal")
            .WithAlphabet([..symbols, sentinel, processed])
            .WithBlankSymbol(empty)
            .AddInitialState("Go left", out var q0)
            .AddState("Write sentinel", out var q1)
            .AddState("Loop", out var qLoop)
            .AddState("Scan", out var qScan)
            .AddState("Return", out var qReturn)
            .AddState("Cleanup", out var qClean);

        var carry = new TuringMachineState[symbols.Length];
        var prepend = new TuringMachineState[symbols.Length];
        for (var i = 0; i < symbols.Length; i++)
        {
            builder.AddState($"Carry {i}", out carry[i]);
            builder.AddState($"Prepend {i}", out prepend[i]);
        }

        var machine = builder.BuildRules(b =>
        {
            // Place the sentinel before the input
            b.AddRule(q0, SymbolMatch.Any, q1, null, TapeMovement.Left)
             .AddRule(q1, SymbolMatch.Any, qLoop, sentinel);
            b.AddRule(qLoop, sentinel, qScan, null, TapeMovement.Right);

            // Find the first character that is not moved yet
            b.AddRule(qScan, processed, qScan, null, TapeMovement.Right);
            for (var i = 0; i < symbols.Length; i++)
                b.AddRule(qScan, symbols[i], carry[i], processed, TapeMovement.Left);
            b.AddRule(qScan, SymbolMatch.Empty, qClean, null, TapeMovement.Left);

            for (var i = 0; i < symbols.Length; i++)
            {
                // Carry the character back over the moved characters
                b.AddRule(carry[i], processed, carry[i], null, TapeMovement.Left);
                b.AddRule(carry[i], sentinel, prepend[i], null, TapeMovement.Left);
                // Append the character to the front of the result
                for (var j = 0; j < symbols.Length; j++)
                    b.AddRule(prepend[i], symbols[j], prepend[i], null, TapeMovement.Left);
                b.AddRule(prepend[i], SymbolMatch.Empty, qReturn, symbols[i]);
            }

            // Return to the sentinel for the next character
            for (var i = 0; i < symbols.Length; i++)
                b.AddRule(qReturn, symbols[i], qReturn, null, TapeMovement.Right);
            b.AddRule(qReturn, sentinel, qLoop);

            // Erase the sentinel and the moved characters
            b.AddRule(qClean, processed, qClean, empty, TapeMovement.Left);
            b.AddHalt(qClean, sentinel, empty);
        });

        return machine;
    }

    public static TuringMachine Create_BusyBeaver()
    {
        return TuringMachine.Create("Busy beaver")
            .WithAlphabet("0", "1")
            .AddState("A", out var a)
            .AddState("B", out var b)
            .AddState("C", out var c)
            .BuildRules(builder => builder
                .AddRule(a, "0", b, "1", TapeMovement.Right)
                .AddRule(a, "1", b, "1", TapeMovement.Left)
                .AddRule(b, "0", b, "1", TapeMovement.Left)
                .AddRule(b, "1", b, "1", TapeMovement.Right)
                .AddRule(c, "0", b, "1", TapeMovement.Left)
                .AddHalt(c, "1")
            );
    }
}