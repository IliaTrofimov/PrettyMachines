using PrettyMachines.Abstract;
using PrettyMachines.BlazorUI.Services;
using PrettyMachines.Implementations;


namespace PrettyMachines.Tests.BlazorUI;

public class AlgorithmRunSessionTests
{
    [Fact]
    public void Session_exposes_a_tape_whose_head_moves_on_each_step()
    {
        var machine = TuringMachines.Create_BinaryIncrementMachine();

        using var session = new AlgorithmRunSession();
        session.Start(machine, "101", 1000);

        session.Tape.Should().NotBeNull();
        session.Tape!.HeadIndex.Should().Be(0);
        session.Tape.CurrentSymbol.Should().Be("1");

        session.Step();

        session.Tape!.HeadIndex.Should().Be(1);
        session.Tape.CurrentSymbol.Should().Be("0");
    }

    [Fact]
    public void Session_has_no_tape_for_a_markov_algorithm()
    {
        using var session = new AlgorithmRunSession();
        session.Start(MarkovAlgorithms.Create_BinaryIncrement(), "101", 100);

        session.Tape.Should().BeNull();
    }

    [Fact]
    public void Session_steps_through_a_turing_machine_one_snapshot_at_a_time()
    {
        var machine = TuringMachines.Create_BinaryIncrementMachine();

        using var session = new AlgorithmRunSession();
        session.Start(machine, "101", 1000);

        session.IsStarted.Should().BeTrue();
        session.IsValidInput.Should().BeTrue();
        session.Output.Should().Be("101");

        var guard = 0;
        while (session.IsActive && guard++ < 10_000)
            session.Step();

        session.Status.Should().Be(TerminationStatus.Success);
        session.Output.Should().Be("110");
        session.Steps.Should().BeGreaterThan(0);
        session.Trace.Should().NotBeEmpty();
    }

    [Fact]
    public void Session_runs_a_markov_algorithm_to_completion()
    {
        var algorithm = MarkovAlgorithms.Create_BinaryIncrement();

        using var session = new AlgorithmRunSession();
        session.Start(algorithm, "101", 1000);
        session.Run();

        session.Status.Should().Be(TerminationStatus.Success);
        session.Output.Should().Be("110");
    }

    [Fact]
    public void Session_reports_invalid_input()
    {
        var algorithm = MarkovAlgorithms.Create_BinaryIncrement();

        using var session = new AlgorithmRunSession();
        session.Start(algorithm, "abc", 100);

        session.IsValidInput.Should().BeFalse();
        session.IsFinished.Should().BeTrue();
        session.Status.Should().Be(TerminationStatus.InvalidInput);
    }

    [Fact]
    public void Reset_clears_the_session()
    {
        var machine = TuringMachines.Create_BinaryIncrementMachine();

        using var session = new AlgorithmRunSession();
        session.Start(machine, "101", 1000);
        session.Step();

        session.Reset();

        session.IsStarted.Should().BeFalse();
        session.IsFinished.Should().BeTrue();
        session.Steps.Should().Be(0);
        session.Trace.Should().BeEmpty();
        session.Status.Should().Be(TerminationStatus.Unknown);
    }

    [Fact]
    public void Stop_marks_the_session_as_aborted()
    {
        var machine = TuringMachines.Create_UnaryToBinaryConverterMachine();

        using var session = new AlgorithmRunSession();
        session.Start(machine, "|||||", 1_000_000);
        session.Step();

        session.Stop();

        session.IsFinished.Should().BeTrue();
        session.Status.Should().Be(TerminationStatus.Aborted);
    }
}
