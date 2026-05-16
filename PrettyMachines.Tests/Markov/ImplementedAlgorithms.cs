using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Implementations;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.Markov;

public class ImplementedAlgorithms(ITestOutputHelper output) : BaseAlgorithmTest(output)
{
    [Theory]
    [InlineData("")]
    [InlineData("()")]
    [InlineData("(()(()())())")]
    public void BracketsGrammar_Success(string input)
    {
        var symbols = new BracketsGrammarSymbols
        {
            Marked = 'x',
            Accepted = 'A',
            Rejected = 'R'
        };
        
        var algorithm = MarkovAlgorithms.Create_BracketsGrammar(symbols);
        CheckAlgorithm(symbols.Accepted.ToString(), TerminationStatus.Success, algorithm, input);
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
        
        var algorithm = MarkovAlgorithms.Create_BracketsGrammar(symbols);
        CheckAlgorithm(symbols.Rejected.ToString(), TerminationStatus.Success, algorithm, input);
    }
    
    [Fact]
    public void BinaryIncrement_Auto()
    {
        var cancellation = new AlgorithmCancellation(100);
        var algorithm = MarkovAlgorithms.Create_BinaryIncrement();

        for (var i = 0; i < 32; i++)
        {
            var result = algorithm.Execute(i.ToString("b"), cancellation, true);
            var expected = i + 1;

            if (result.Output != expected.ToString("b"))
            {
                output.WriteLine($"Execution trace\n{string.Join('\n', result.Trace)}");
                Assert.Fail(
                    $"""
                     Unexpected result for i={i} ('{i:b}' as binary)
                     - expected: '{expected:b}' as binary
                     - actual:   '{result.Output}' as binary
                     """);
            }
        }
    }
    
    [Fact]
    public void BinaryDecrement_Auto()
    {
        var cancellation = new AlgorithmCancellation(100);
        var algorithm = MarkovAlgorithms.Create_BinaryDecrement();

        for (var i = 3; i < 32; i++)
        {
            var result = algorithm.Execute(i.ToString("b"), cancellation, true);
            var expected = i - 1;

            if (result.Output != expected.ToString("b"))
            {
                output.WriteLine($"Execution trace\n{string.Join('\n', result.Trace)}");
                Assert.Fail(
                    $"""
                     Unexpected result for i={i} ('{i:b}' as binary)
                     - expected: '{expected:b}' as binary
                     - actual:   '{result.Output}' as binary
                     """);
            }
        }
    }
    
    [Theory]
    [InlineData(0b001)]
    [InlineData(0b010)]
    [InlineData(0b011)]
    [InlineData(0b100)]
    [InlineData(0b101)]
    [InlineData(0b110)]
    [InlineData(0b111)]
    public void BinaryDecrement(int input)
    {
        var cancellation = new AlgorithmCancellation(100);
        var algorithm = MarkovAlgorithms.Create_BinaryDecrement();

        var result = algorithm.Execute(input.ToString("b"), cancellation, true);
        output.WriteLine($"Execution trace\n{string.Join('\n', result.Trace)}");

        var expected = input - 1;

        if (result.Output != expected.ToString("b"))
        {
            Assert.Fail(
                $"""
                 Unexpected result for i={input} ('{input:b}' as binary)
                 - expected: '{expected:b}' as binary
                 - actual:   '{result.Output}' as binary
                 """);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("00")]
    [InlineData("123")]
    [InlineData("0001")]
    public void LeadingZerosTrim(string input)
    {
        var algorithm = MarkovAlgorithms.Create_LeadingZerosTrim();
        
        var expected = "0";
        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] != '0')
            {
                expected = input[i..];
                break;
            }
        }
        
        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input);
    }
}