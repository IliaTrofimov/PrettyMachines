using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Markov;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.Markov;

public class Concatenation(ITestOutputHelper output) : BaseAlgorithmTest(output)
{
    [Fact]
    public void ConcatTwoAlgorithms()
    {
        var decrement = MarkovAlgorithm.Create("Bin decrement")
            .AddRule("00*", "*11")
            .AddRule("01*", "00", true)
            .AddRule("10*", "01", true)
            .AddRule("11*", "10", true)
            .AddRule("$0", "0$").WithComment("Move right")
            .AddRule("$1", "1$").WithComment("Move right")
            .AddRule("$", "*").WithComment("Last digit")
            .AddRule("", "0$").WithComment("Place marker")
            .Build();
        
        var trimmer = MarkovAlgorithm.Create("Trim leading zeros")
            .AddRule("|0", "|")
            .AddRule("|", "", true)
            .AddRule("", "|").WithComment("Place marker")
            .Build();
        
        var combined = decrement + trimmer;
        Assert.Equal(2, combined.Algorithms.Count);
        Assert.Same(combined.Algorithms[0], decrement);
        Assert.Same(combined.Algorithms[1], trimmer);

        var input = "001101";
        CheckAlgorithm("1100", TerminationStatus.Success, combined, input);
    }
    
    [Fact]
    public void ConcatManyAlgorithms()
    {
        var combined = new AlgorithmConcatenation<MarkovAlgorithm, string>();
        // aa -> A
        // a => ""
        // => 
        // ...
        for (var ch = 'a'; ch <= 'z'; ch++)
        {
            var alg = MarkovAlgorithm.Create()
                .AddRule($"{ch}{ch}", char.ToUpper(ch))
                .AddRule(ch, "", true)
                .AddRule("", "", true)
                .Build();
            combined += alg;
        }
        
        Assert.Equal(26, combined.Algorithms.Count);

        var input = "aabbzzzzvvaaa";
        CheckAlgorithm("ABZZVA", TerminationStatus.Success, combined, input);
    }
    
    [Fact]
    public void StopWhenSomeAlgorithmFails()
    {
        var alg1 = MarkovAlgorithm.Create()
            .WithAlphabet("abcdefABCDEF")
            .WithMarkers('|')
            .AddRule("a", "A")
            .AddRule("b", "B")
            .AddRule("c", "C")
            .AddRule("d", "D")
            .AddRule("e", "E")
            .AddRule("f", "F")
            .AddRule("", "|", true)
            .Build();
        
        var alg2 = MarkovAlgorithm.Create()
            .WithAlphabet("abcdefABCDEF|")
            .AddRule("|", "a||a", true)
            .Build();

        var combined = alg1 + alg2;
        
        var input = "aaaaabbb1233";
        CheckAlgorithm(TerminationStatus.InvalidInput, combined, input);
    }
}