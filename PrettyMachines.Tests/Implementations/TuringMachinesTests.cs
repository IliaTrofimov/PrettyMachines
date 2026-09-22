using PrettyMachines.Abstract;
using PrettyMachines.Implementations;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.Implementations;

public class TuringMachinesTests(ITestOutputHelper output) : BaseAlgorithmTest(output)
{
    private static readonly AlgorithmCancellation Cancellation = new(10_000);
    private static readonly AlgorithmCancellation LongCancellation = new(1_000_000);


    #region Binary increment

    [Theory]
    [InlineData("0",    "1")]
    [InlineData("1",    "10")]
    [InlineData("10",   "11")]
    [InlineData("101",  "110")]
    [InlineData("111",  "1000")]
    [InlineData("1001", "1010")]
    public void BinaryIncrement_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_BinaryIncrementMachine();

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, Cancellation);
    }

    #endregion

    #region Binary decrement

    [Theory]
    [InlineData("0",    "0")]
    [InlineData("1",    "0")]
    [InlineData("10",   "1")]
    [InlineData("11",   "10")]
    [InlineData("100",  "11")]
    [InlineData("101",  "100")]
    [InlineData("1000", "111")]
    public void BinaryDecrement_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_BinaryDecrementMachine();

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, Cancellation);
    }

    #endregion

    #region Binary addition

    [Theory]
    [InlineData("0+0",     "0")]
    [InlineData("1+1",     "10")]
    [InlineData("1+0",     "1")]
    [InlineData("101+11",  "1000")]
    [InlineData("111+1",   "1000")]
    [InlineData("1001+111","10000")]
    [InlineData("0+101",   "101")]
    public void BinaryAddition_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_BinaryAdditionMachine();

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, Cancellation);
    }

    #endregion

    #region Binary subtraction

    [Theory]
    [InlineData("1-1",     "0")]
    [InlineData("10-1",    "1")]
    [InlineData("11-1",    "10")]
    [InlineData("100-1",   "11")]
    [InlineData("1000-1",  "111")]
    [InlineData("101-11",  "10")]
    [InlineData("1010-101","101")]
    [InlineData("0-1",     "0")]
    [InlineData("10-11",   "0")]
    public void BinarySubtraction_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_BinarySubtractionMachine();

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, Cancellation);
    }

    #endregion

    #region String concatenation

    [Theory]
    [InlineData("ab+cd",       "abcd")]
    [InlineData("hello+world", "helloworld")]
    [InlineData("0+101",       "0101")]
    [InlineData("+x",          "x")]
    [InlineData("x+",          "x")]
    public void StringConcatenation_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_StringConcatenationMachine();

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, Cancellation);
    }

    #endregion

    #region Busy beaver

    [Theory]
    [InlineData("",    "")]
    [InlineData("0",   "1")]
    [InlineData("1",   "1")]
    [InlineData("00",  "11")]
    [InlineData("01",  "11")]
    [InlineData("10",  "10")]
    [InlineData("11",  "11")]
    [InlineData("000", "111")]
    [InlineData("101", "101")]
    [InlineData("010", "111")]
    public void BusyBeaver_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_BusyBeaver();

        CheckAlgorithm(expected, TerminationStatus.Stuck, algorithm, input, Cancellation);
    }

    #endregion

    #region Unary to binary

    [Theory]
    [InlineData("",         "0")]
    [InlineData("|",        "1")]
    [InlineData("||",       "10")]
    [InlineData("|||",      "11")]
    [InlineData("||||",     "100")]
    [InlineData("|||||",    "101")]
    [InlineData("|||||||||","1001")]
    public void UnaryToBinary_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_UnaryToBinaryConverterMachine();

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, Cancellation);
    }

    #endregion

    #region Unary to ternary

    [Theory]
    [InlineData("",         "0")]
    [InlineData("|",        "1")]
    [InlineData("||",       "2")]
    [InlineData("|||",      "10")]
    [InlineData("||||",     "11")]
    [InlineData("|||||",    "12")]
    [InlineData("||||||",   "20")]
    [InlineData("||||||||", "22")]
    [InlineData("|||||||||","100")]
    public void UnaryToTernary_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_UnaryToTernaryConverterMachine();

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, Cancellation);
    }

    #endregion

    #region Binary to unary

    [Theory]
    [InlineData("0",    "")]
    [InlineData("1",    "|")]
    [InlineData("10",   "||")]
    [InlineData("11",   "|||")]
    [InlineData("100",  "||||")]
    [InlineData("101",  "|||||")]
    [InlineData("110",  "||||||")]
    [InlineData("111",  "|||||||")]
    [InlineData("1000", "||||||||")]
    public void BinaryToUnary_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_BinaryToUnaryConverterMachine();

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, Cancellation);
    }

    #endregion

    #region Decimal to binary

    [Theory]
    [InlineData("0",    "0")]
    [InlineData("1",    "1")]
    [InlineData("2",    "10")]
    [InlineData("5",    "101")]
    [InlineData("8",    "1000")]
    [InlineData("9",    "1001")]
    [InlineData("10",   "1010")]
    [InlineData("255",  "11111111")]
    [InlineData("1000", "1111101000")]
    public void DecimalToBinary_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_DecimalToBinaryConverterMachine();

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, LongCancellation);
    }

    #endregion

    #region Binary to decimal

    [Theory]
    [InlineData("0",          "0")]
    [InlineData("1",          "1")]
    [InlineData("10",         "2")]
    [InlineData("11",         "3")]
    [InlineData("101",        "5")]
    [InlineData("1000",       "8")]
    [InlineData("1111",       "15")]
    [InlineData("10000",      "16")]
    [InlineData("1111101000", "1000")]
    public void BinaryToDecimal_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_BinaryToDecimalConverterMachine();

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, LongCancellation);
    }

    #endregion

    #region String reversal

    [Theory]
    [InlineData("",      "")]
    [InlineData("a",     "a")]
    [InlineData("ab",    "ba")]
    [InlineData("abc",   "cba")]
    [InlineData("hello", "olleh")]
    [InlineData("kayak", "kayak")]
    public void StringReversal_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_StringReversalMachine();

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, Cancellation);
    }

    [Theory]
    [InlineData("0",    "0")]
    [InlineData("10",   "01")]
    [InlineData("100",  "001")]
    [InlineData("0110", "0110")]
    [InlineData("1011", "1101")]
    public void StringReversal_WithCustomAlphabet_ProducesExpectedResult(string input, string expected)
    {
        var algorithm = TuringMachines.Create_StringReversalMachine("01");

        CheckAlgorithm(expected, TerminationStatus.Success, algorithm, input, Cancellation);
    }

    #endregion
}
