using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Markov;
using PrettyMachines.Algorithms.Turing;
using PrettyMachines.BlazorUI.Services;
using PrettyMachines.BlazorUI.Services.DraftAdapters;
using PrettyMachines.Implementations;
using PrettyMachines.Implementations.Catalog;


namespace PrettyMachines.Tests.BlazorUI;

public class DraftAdapterTests
{
    private static readonly AlgorithmCancellation Cancellation = new(100_000);


    public static IEnumerable<object[]> TuringAlgorithms() => Families(nameof(TuringMachines));

    public static IEnumerable<object[]> MarkovAlgorithms() => Families(nameof(MarkovAlgorithms));


    [Theory]
    [MemberData(nameof(TuringAlgorithms))]
    public void Turing_draft_round_trip_preserves_structure(string algorithmId)
    {
        var descriptor = AlgorithmCatalog.Find(nameof(TuringMachines), algorithmId)!;
        var original = (TuringMachine)descriptor.Create();

        var draft = TuringDraftAdapter.From(original);
        var rebuilt = TuringDraftAdapter.ToAlgorithm(draft);

        rebuilt.Name.Should().Be(original.Name);
        rebuilt.HasStrictAlphabet.Should().Be(original.HasStrictAlphabet);
        (rebuilt.Instructions.BlankSymbol ?? "").Should().Be(original.Instructions.BlankSymbol ?? "");
        TuringSignatures(rebuilt).Should().BeEquivalentTo(TuringSignatures(original));

        if (original.HasStrictAlphabet)
            rebuilt.Instructions.Alphabet.Should().BeEquivalentTo(original.Instructions.Alphabet);
    }

    [Theory]
    [MemberData(nameof(MarkovAlgorithms))]
    public void Markov_draft_round_trip_preserves_structure(string algorithmId)
    {
        var descriptor = AlgorithmCatalog.Find(nameof(MarkovAlgorithms), algorithmId)!;
        var original = (MarkovAlgorithm)descriptor.Create();

        var draft = MarkovDraftAdapter.From(original);
        var rebuilt = MarkovDraftAdapter.ToAlgorithm(draft);

        rebuilt.Name.Should().Be(original.Name);
        rebuilt.Rules.Should().HaveCount(original.Rules.Count);

        for (var i = 0; i < original.Rules.Count; i++)
        {
            rebuilt.Rules[i].Pattern.Should().Be(original.Rules[i].Pattern);
            rebuilt.Rules[i].Replacement.Should().Be(original.Rules[i].Replacement);
            rebuilt.Rules[i].IsTerminal.Should().Be(original.Rules[i].IsTerminal);
            rebuilt.GetRuleComment(i).Should().Be(original.GetRuleComment(i));
        }

        if (original.Alphabet is not null)
            rebuilt.Alphabet.Should().BeEquivalentTo(original.Alphabet);
        if (original.Markers is not null)
            rebuilt.Markers.Should().BeEquivalentTo(original.Markers);
    }

    [Theory]
    [InlineData("Create_BinaryIncrementMachine", "101", "110")]
    [InlineData("Create_BinaryDecrementMachine", "100", "11")]
    [InlineData("Create_StringConcatenationMachine", "ab+cd", "abcd")]
    [InlineData("Create_StringReversalMachine", "abc", "cba")]
    public void Turing_draft_round_trip_preserves_execution(string algorithmId, string input, string expected)
    {
        var descriptor = AlgorithmCatalog.Find(nameof(TuringMachines), algorithmId)!;
        var original = (TuringMachine)descriptor.Create();
        var rebuilt = TuringDraftAdapter.ToAlgorithm(TuringDraftAdapter.From(original));

        var originalResult = original.Execute(input, Cancellation);
        var rebuiltResult = rebuilt.Execute(input, Cancellation);

        originalResult.Output.Should().Be(expected);
        rebuiltResult.Output.Should().Be(expected);
        rebuiltResult.Termination.Should().Be(originalResult.Termination);
    }

    [Theory]
    [InlineData("Create_BinaryIncrement", "101", "110")]
    [InlineData("Create_StringConcatenation", "ab+cd", "abcd")]
    [InlineData("Create_LeadingZerosTrim", "000123", "123")]
    [InlineData("Create_StringReversal", "abc", "cba")]
    public void Markov_draft_round_trip_preserves_execution(string algorithmId, string input, string expected)
    {
        var descriptor = AlgorithmCatalog.Find(nameof(MarkovAlgorithms), algorithmId)!;
        var original = (MarkovAlgorithm)descriptor.Create();
        var rebuilt = MarkovDraftAdapter.ToAlgorithm(MarkovDraftAdapter.From(original));

        var originalResult = original.Execute(input, Cancellation);
        var rebuiltResult = rebuilt.Execute(input, Cancellation);

        originalResult.Output.Should().Be(expected);
        rebuiltResult.Output.Should().Be(expected);
        rebuiltResult.Termination.Should().Be(originalResult.Termination);
    }

    [Fact]
    public void CreateBlank_builds_for_both_families()
    {
        var turing = DraftAlgorithmFactory.CreateBlank(nameof(TuringMachines));
        turing.Turing.Should().NotBeNull();
        DraftAlgorithmFactory.Build(turing).Should().BeOfType<TuringMachine>();

        var markov = DraftAlgorithmFactory.CreateBlank(nameof(MarkovAlgorithms));
        markov.Markov.Should().NotBeNull();
        DraftAlgorithmFactory.Build(markov).Should().BeOfType<MarkovAlgorithm>();
    }

    [Fact]
    public void Fork_creates_a_runnable_copy_of_a_built_in()
    {
        var descriptor = AlgorithmCatalog.Find(nameof(TuringMachines), "Create_BinaryIncrementMachine")!;

        var draft = DraftAlgorithmFactory.Fork(descriptor);
        draft.SourceDescriptorId.Should().Be(descriptor.Id);

        var algorithm = DraftAlgorithmFactory.Build(draft);
        algorithm.Execute("101", Cancellation).Output.Should().Be("110");
    }


    private static IEnumerable<object[]> Families(string familyId) =>
        AlgorithmCatalog.Discover()
            .Single(family => family.Id == familyId)
            .Algorithms
            .Select(descriptor => new object[] { descriptor.Id });

    private static List<string> TuringSignatures(TuringMachine machine)
    {
        var states = machine.Instructions.States
            .Where(state => state.Id != TuringMachineState.Halt.Id)
            .OrderBy(state => state.Id)
            .ToList();

        var index = new Dictionary<int, int>();
        for (var i = 0; i < states.Count; i++)
            index[states[i].Id] = i;

        return machine.Instructions
            .Select(instruction => string.Join("|",
                index[instruction.InitialState.Id],
                instruction.ScannedSymbol.Match,
                instruction.ScannedSymbol.Value ?? "<null>",
                instruction.NextState.Id == TuringMachineState.Halt.Id ? "halt" : index[instruction.NextState.Id].ToString(),
                instruction.PrintedSymbol is null ? "<none>" : $"'{instruction.PrintedSymbol}'",
                instruction.Movement))
            .OrderBy(signature => signature, StringComparer.Ordinal)
            .ToList();
    }
}
