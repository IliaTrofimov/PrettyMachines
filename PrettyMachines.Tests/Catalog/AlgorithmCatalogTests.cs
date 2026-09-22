using PrettyMachines.Markov;
using PrettyMachines.Turing;
using PrettyMachines.Implementations;
using PrettyMachines.Implementations.Catalog;


namespace PrettyMachines.Tests.Catalog;

public class AlgorithmCatalogTests
{
    [Fact]
    public void Discover_returns_both_known_families()
    {
        var families = AlgorithmCatalog.Discover();

        families.Select(family => family.Id).Should().Contain([nameof(TuringMachines), nameof(MarkovAlgorithms)]);
        families.SelectMany(family => family.Algorithms).Should().NotBeEmpty();
    }

    [Fact]
    public void Discover_finds_every_factory_of_both_families()
    {
        var families = AlgorithmCatalog.Discover();

        var turing = families.Single(family => family.Id == nameof(TuringMachines));
        var markov = families.Single(family => family.Id == nameof(MarkovAlgorithms));

        turing.Algorithms.Should().HaveCount(13);
        markov.Algorithms.Should().HaveCount(14);
    }

    [Theory]
    [InlineData("Create_BinaryIncrementMachine", "Binary increment")]
    [InlineData("Create_BracketsGrammar", "Brackets grammar")]
    [InlineData("Create_StringReversalMachine", "String reversal")]
    [InlineData("Create_UnaryToBinaryConverterMachine", "Unary to binary converter")]
    [InlineData("Create_BinaryToDecimalConverterMachine", "Binary to decimal converter")]
    [InlineData("Create_LeadingZerosTrim", "Leading zeros trim")]
    [InlineData("Create_BusyBeaver", "Busy beaver")]
    public void CleanAlgorithmName_strips_prefix_and_suffix_and_splits_pascal_case(string methodName, string expected)
    {
        AlgorithmCatalog.CleanAlgorithmName(methodName).Should().Be(expected);
    }

    [Theory]
    [InlineData("TuringMachines", "Turing machines")]
    [InlineData("MarkovAlgorithms", "Markov algorithms")]
    public void CleanFamilyName_produces_readable_name(string typeName, string expected)
    {
        AlgorithmCatalog.CleanFamilyName(typeName).Should().Be(expected);
    }

    public static IEnumerable<object[]> AllAlgorithms() =>
        AlgorithmCatalog.Discover()
            .SelectMany(family => family.Algorithms)
            .Select(descriptor => new object[] { descriptor.FamilyId, descriptor.Id });

    [Theory]
    [MemberData(nameof(AllAlgorithms))]
    public void Every_descriptor_creates_a_named_algorithm(string familyId, string algorithmId)
    {
        var descriptor = AlgorithmCatalog.Find(familyId, algorithmId);

        descriptor.Should().NotBeNull();
        var algorithm = descriptor!.Create();
        algorithm.Should().NotBeNull();
        algorithm.Name.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Factories_with_optional_parameters_use_their_defaults()
    {
        AlgorithmCatalog.Find(nameof(TuringMachines), "Create_BracketsGrammar")!
            .Create().Should().BeOfType<TuringMachine>();
        AlgorithmCatalog.Find(nameof(TuringMachines), "Create_StringReversalMachine")!
            .Create().Should().BeOfType<TuringMachine>();
        AlgorithmCatalog.Find(nameof(MarkovAlgorithms), "Create_BracketsGrammar")!
            .Create().Should().BeOfType<MarkovAlgorithm>();
        AlgorithmCatalog.Find(nameof(MarkovAlgorithms), "Create_StringReversal")!
            .Create().Should().BeOfType<MarkovAlgorithm>();
    }

    [Fact]
    public void Find_is_case_sensitive_and_returns_null_for_unknown_ids()
    {
        AlgorithmCatalog.Find(nameof(TuringMachines), "Create_BinaryIncrementMachine").Should().NotBeNull();
        AlgorithmCatalog.Find(nameof(TuringMachines), "create_binaryincrementmachine").Should().BeNull();
        AlgorithmCatalog.Find("Missing", "Create_BusyBeaver").Should().BeNull();
        AlgorithmCatalog.Find(null, null).Should().BeNull();
    }

    [Fact]
    public void Descriptors_expose_family_and_kind()
    {
        var descriptor = AlgorithmCatalog.Find(nameof(TuringMachines), "Create_BusyBeaver");

        descriptor.Should().NotBeNull();
        descriptor!.FamilyId.Should().Be(nameof(TuringMachines));
        descriptor.IsTuring.Should().BeTrue();
        descriptor.IsMarkov.Should().BeFalse();
    }
}
