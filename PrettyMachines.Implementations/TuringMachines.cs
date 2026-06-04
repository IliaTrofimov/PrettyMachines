using PrettyMachines.Algorithms.Turing;


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
    /// <b>- outputs:</b> calculated binary number.
    /// </summary>
    public static TuringMachine Create_BinaryIncrementMachine()
    {
        return TuringMachine.Create("Binary increment")
            .WithAlphabet(["0", "1"])
            .WithBlankSymbol("_")
            .WithStringComparer(StringComparison.CurrentCultureIgnoreCase)
            .AddInitialState("Find number ending", out var q0)
            .AddState("Increment one bit", out var q1)
            .AddState("Find number start", out var q2)
            .BuildRules(builder => builder
                .AddRule(q0, SymbolMatch.NotEmpty, q0, null, TapeMovement.Right)
                .AddRule(q0, SymbolMatch.Empty,    q1, null, TapeMovement.Left)
                .AddRule(q1, "0",                  q2, "1")
                .AddRule(q1, "1",                  q0, "0",  TapeMovement.Left)
                .AddRule(q2, SymbolMatch.NotEmpty, q0, null, TapeMovement.Right)
                .AddHalt(q1, SymbolMatch.Empty, "1", TapeMovement.Right)
                .AddHalt(q2, SymbolMatch.Empty)
            );
    }
}