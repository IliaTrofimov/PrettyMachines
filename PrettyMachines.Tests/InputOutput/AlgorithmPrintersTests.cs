using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions.Execution;
using PrettyMachines.Algorithms.Markov;
using PrettyMachines.Algorithms.Turing;
using PrettyMachines.Algorithms.Utils;
using PrettyMachines.Algorithms.Utils.Printing;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.InputOutput;

public class AlgorithmPrintersTests(ITestOutputHelper output)
{
    public enum PrinterTarget
    {
        String, StringBuilder,Stream
    }
    
    private readonly Regex markovCsvRegex = new Regex(
        @"(?:(?:""[^""]*"")|[^,]*)(?=,|$)",
        RegexOptions.Multiline | RegexOptions.Compiled
    );
    
    private readonly Regex markovRuleRegex = new Regex(
        @"^"".*""\s*[=-]>\s*"".*""\s*(//.+)?$",
        RegexOptions.Multiline | RegexOptions.Compiled
    );
    

    [Theory]
    [InlineData(true, PrinterTarget.String)]
    [InlineData(false, PrinterTarget.String)]
    [InlineData(true, PrinterTarget.StringBuilder)]
    [InlineData(false, PrinterTarget.StringBuilder)]
    [InlineData(true, PrinterTarget.Stream)]
    [InlineData(false, PrinterTarget.Stream)]
    public void Markov_CSV(bool hasHeader, PrinterTarget target)
    {
        var algorithm = GetMarkovAlgorithm(hasHeader);
        string? additionalText = null;
        string printedText = null!;
        
        switch (target)
        {
            case PrinterTarget.String:
                printedText = MarkovAlgorithmPrinter.PrintCsv(algorithm);
                break;
            case PrinterTarget.StringBuilder:
                var stringBuilder = PrepareStringBuilder();
                MarkovAlgorithmPrinter.PrintCsv(stringBuilder, algorithm);
                printedText = stringBuilder.ToString();
                additionalText = "!TEST TEXT";
                break;
            case PrinterTarget.Stream:
                var stream = PrepareStream();
                MarkovAlgorithmPrinter.PrintCsv(stream, algorithm);
                printedText = StreamToString(stream);
                additionalText = "!TEST TEXT";
                break;
        }
        
        TestMarkov(printedText, algorithm, markovCsvRegex, additionalText);
    }
    
    [Theory]
    [InlineData(true, PrinterTarget.String)]
    [InlineData(false, PrinterTarget.String)]
    [InlineData(true, PrinterTarget.StringBuilder)]
    [InlineData(false, PrinterTarget.StringBuilder)]
    [InlineData(true, PrinterTarget.Stream)]
    [InlineData(false, PrinterTarget.Stream)]
    public void Markov_FormatedList(bool hasHeader, PrinterTarget target)
    {
        var algorithm = GetMarkovAlgorithm(hasHeader);
        string? additionalText = null;
        string printedText = null!;
        
        switch (target)
        {
            case PrinterTarget.String:
                printedText = MarkovAlgorithmPrinter.PrintFormatted(algorithm);
                break;
            case PrinterTarget.StringBuilder:
                var stringBuilder = PrepareStringBuilder();
                MarkovAlgorithmPrinter.PrintFormatted(stringBuilder, algorithm);
                printedText = stringBuilder.ToString();
                additionalText = "!TEST TEXT";
                break;
            case PrinterTarget.Stream:
                var stream = PrepareStream();
                MarkovAlgorithmPrinter.PrintFormatted(stream, algorithm);
                printedText = StreamToString(stream);
                additionalText = "!TEST TEXT";
                break;
        }
        
        TestMarkov(printedText, algorithm, markovRuleRegex, additionalText);
    }

    [Fact]
    public void Markov_FormatedList_NoQuoting()
    {
        var algorithm = GetMarkovAlgorithm(false);
        var printedText = MarkovAlgorithmPrinter.PrintFormatted(algorithm, quote: '\0');
        
        WritePrinterOutput(printedText);

        printedText.Should().ContainAll("aa->A", "a ->b", "bb=>  // success", "  =>! // fail");
    }
    
    [Theory]
    [InlineData(PrinterTarget.String)]
    [InlineData(PrinterTarget.StringBuilder)]
    [InlineData(PrinterTarget.Stream)]
    public void Turing_FormatedList(PrinterTarget target)
    {
        var algorithm = GetTuringMachine(true);
        string? additionalText = null;
        string printedText = null!;
        
        switch (target)
        {
            case PrinterTarget.String:
                printedText = InstructionTablePrinter.PrintList(algorithm);
                break;
            case PrinterTarget.StringBuilder:
                var stringBuilder = PrepareStringBuilder();
                InstructionTablePrinter.PrintList(stringBuilder, algorithm);
                printedText = stringBuilder.ToString();
                additionalText = "!TEST TEXT";
                break;
            case PrinterTarget.Stream:
                var stream = PrepareStream();
                InstructionTablePrinter.PrintList(stream, algorithm);
                printedText = StreamToString(stream);
                additionalText = "!TEST TEXT";
                break;
        }
        
        TestTuring(printedText, algorithm, markovRuleRegex, additionalText);
    }
    
    [Theory]
    [InlineData(PrinterTarget.String)]
    [InlineData(PrinterTarget.StringBuilder)]
    [InlineData(PrinterTarget.Stream)]
    public void Turing_FormatedTable(PrinterTarget target)
    {
        var algorithm = GetTuringMachine(true);
        string? additionalText = null;
        string printedText = null!;
        
        switch (target)
        {
            case PrinterTarget.String:
                printedText = InstructionTablePrinter.PrintTable(algorithm);
                break;
            case PrinterTarget.StringBuilder:
                var stringBuilder = PrepareStringBuilder();
                InstructionTablePrinter.PrintTable(stringBuilder, algorithm);
                printedText = stringBuilder.ToString();
                additionalText = "!TEST TEXT";
                break;
            case PrinterTarget.Stream:
                var stream = PrepareStream();
                InstructionTablePrinter.PrintTable(stream, algorithm);
                printedText = StreamToString(stream);
                additionalText = "!TEST TEXT";
                break;
        }
        
        TestTuring(printedText, algorithm, markovRuleRegex, additionalText);
    }
    
    [Theory]
    [InlineData(PrinterTarget.String)]
    [InlineData(PrinterTarget.StringBuilder)]
    [InlineData(PrinterTarget.Stream)]
    public void Turing_CSV(PrinterTarget target)
    {
        var algorithm = GetTuringMachine(true);
        string? additionalText = null;
        string printedText = null!;
        
        switch (target)
        {
            case PrinterTarget.String:
                printedText = InstructionTablePrinter.PrintCsv(algorithm);
                break;
            case PrinterTarget.StringBuilder:
                var stringBuilder = PrepareStringBuilder();
                InstructionTablePrinter.PrintCsv(stringBuilder, algorithm);
                printedText = stringBuilder.ToString();
                additionalText = "!TEST TEXT";
                break;
            case PrinterTarget.Stream:
                var stream = PrepareStream();
                InstructionTablePrinter.PrintCsv(stream, algorithm);
                printedText = StreamToString(stream);
                additionalText = "!TEST TEXT";
                break;
        }
        
        TestTuring(printedText, algorithm, markovRuleRegex, additionalText);
    }
    
    // ------------------------------------
    
    private void TestMarkov(string printText, MarkovAlgorithm algorithm, Regex regex, params string?[] additionalText)
    {
        WritePrinterOutput(printText);
        printText.Should().NotBeNullOrWhiteSpace();
        
        using (new AssertionScope())
        {
            foreach (var rule in algorithm.Rules)
            {
                var expectedPattern = $"\"{rule.Pattern}\"";
                var expectedReplacement = $"\"{rule.Replacement}\"";
                printText.Should()
                    .Contain(expectedPattern, $"output must contain pattern string for rule {{{rule}}}")
                    .And
                    .Contain(expectedReplacement, $"output must contain replacement string for rule {{{rule}}}");
            }

            if (algorithm.Name != null)
                printText.Should().Contain($"//name: \"{algorithm.Name}\"");
            if (algorithm.Alphabet != null)
                printText.Should().Contain($"//alphabet: \"{string.Join("", algorithm.Alphabet)}\"");
            if (algorithm.Markers != null)
                printText.Should().Contain($"//markers: \"{string.Join("", algorithm.Markers)}\"");
        }

        if (additionalText.Length > 0)
        {
            printText.Should().ContainAll(additionalText);
        }
        
        var textLines = printText.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        textLines.Should().AllSatisfy(line =>
        {
            if (line.StartsWith("//") || line.StartsWith("!"))
                return;

            line.Should().MatchRegex(regex);
        });
    }
    
    private void TestTuring(string printText, TuringMachine algorithm, Regex regex, params string?[] additionalText)
    {
        WritePrinterOutput(printText);
        printText.Should().NotBeNullOrWhiteSpace();
        
        var instructions = algorithm.Instructions;
        
        using (new AssertionScope())
        {
            foreach (var rule in instructions)
            {
                printText.Should()
                    .Contain(rule.InitialState.ToString(), "output must contain initial state")
                    .And
                    .Contain(rule.NextState.ToString(), "output must contain next state")
                    .And
                    .Contain(rule.ScannedSymbol.ToString(), "output must scanned symbol")
                    .And
                    .Contain(rule.Movement.ToChar().ToString(), "output must contain movement");
                
                if (rule.PrintedSymbol is not null)
                    printText.Should().Contain(rule.PrintedSymbol, "output must printed symbol");
            }

            if (algorithm.Name != null)
                printText.Should().Contain($"//name: \"{algorithm.Name}\"");
            if (instructions.Alphabet.Count > 0)
                printText.Should().Contain($"//alphabet-characters: \"{string.Join("", algorithm.Instructions.Alphabet)}\"");
            
            printText.Should().Contain($"//alphabet-strict: \"{algorithm.HasStrictAlphabet}\"");

        }

        if (additionalText.Length > 0)
        {
            printText.Should().ContainAll(additionalText);
        }
    }


    private MarkovAlgorithm GetMarkovAlgorithm(bool withHeader)
    {
        var builder = MarkovAlgorithm.Create()
            .AddRule("aa", "A")
            .AddRule("a", "b")
            .AddRule("bb", "", true).WithComment("success")
            .AddRule("", "!", true).WithComment("fail");

        if (withHeader)
        {
            return builder
                .WithAlphabet("abA")
                .WithMarkers("!")
                .Build("test-algorithm");
        }
    
        return builder.Build();
    }
    
    private TuringMachine GetTuringMachine(bool withHeader)
    {
        var builder = TuringMachine.Create(withHeader ? "test-algorithm" : null)
            .AddInitialState("q0", out var q0)
            .AddState("q1", out var q1)
            .AddTerminalState("q2", out var q2);
            
        if (withHeader)
        {
            builder = builder.WithAlphabet("a", "b", "c");
        }

        return builder.BuildRules(x => x
            .AddRule(q0, "a", q0, "a", TapeMovement.Right)
            .AddRule(q0, "b", q1, "b", TapeMovement.None)
            .AddRule(q1, SymbolMatch.NotEmpty, q1, null, TapeMovement.Left)
            .AddRule(q1, SymbolMatch.Empty, q2)
            .AddHalt(q1, "c")
        );
    }

    private void WritePrinterOutput(string text)
    {
        output.WriteLine("vvvvvvvvvvvv PRINTED TEXT vvvvvvvvvvvv");
        output.WriteLine(text);
        output.WriteLine("^^^^^^^^^^^^ PRINTED TEXT ^^^^^^^^^^^^");
    }
    
    private Stream PrepareStream()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("!TEST TEXT\n");
        writer.Flush();
        return stream;
    }

    private string StreamToString(Stream stream)
    {
        stream.Position = 0;
        using var sr = new StreamReader(stream);
        return sr.ReadToEnd();
    }
    
    private StringBuilder PrepareStringBuilder()
    {
        return new StringBuilder("!TEST TEXT\n");
    }
}