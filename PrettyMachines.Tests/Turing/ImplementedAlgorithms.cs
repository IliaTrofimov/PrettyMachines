using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Implementations;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.Turing;

public class ImplementedAlgorithms(ITestOutputHelper output) : BaseAlgorithmTest(output)
{
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