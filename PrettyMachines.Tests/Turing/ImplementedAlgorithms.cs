using System.Text;
using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Turing;
using PrettyMachines.Algorithms.Utils;
using PrettyMachines.Implementations;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.Turing;

public class ImplementedAlgorithms(ITestOutputHelper output) : BaseAlgorithmTest(output)
{
    [Fact]
    public void TestPrint()
    {
        var algorithm = TuringMachine.Create()
            .AddState(out var q1)
            .AddState(out var q2)
            .WithBlankSymbol("_")
            .AddTerminalState(out var q3)
            .BuildRules(builder => builder
                .AddRule(q1, "x", q2, "X", TapeMovement.Right)
                .AddRule(q1, "y", q2, "Y", TapeMovement.Right)
                .AddRule(q2, SymbolMatch.NotEmpty, q3, "_", TapeMovement.None)
                .AddRule(q2, SymbolMatch.Empty, q3, "_", TapeMovement.None)
            );
    }
    
    [Theory]
    [InlineData("(")]
    [InlineData("())")]
    [InlineData("(()(()()())")]
    public void BracketsGrammar_Rejected(string input)
    {
        var symbols = new BracketsGrammarSymbols
        {
            Marked = 'x',
            Accepted = 'A',
            Rejected = 'R'
        };
        
        var algorithm = TuringMachines.Create_BracketsGrammar(symbols);
        
        CheckAlgorithm(TerminationStatus.Success, algorithm, input);
        
    }
}