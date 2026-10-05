using PrettyMachines.Abstract;
using PrettyMachines.FSM;
using PrettyMachines.Implementations;
using Xunit.Abstractions;


namespace PrettyMachines.Tests.Implementations;

public class FsmTests(ITestOutputHelper output) : BaseAlgorithmTest(output)
{
    private static readonly AlgorithmCancellation Cancellation = new(200);

    [Theory]
	[InlineData("http://abce.bbb", true)]
	[InlineData("https://abc/ef.aa", true)]
	[InlineData("hts://aa.ccc//", false)]
	public void UrlGrammar_Test(string input, bool accept)
	{
		var fsm = FiniteStateMachines.Create_UrlGrammar();
		if (accept)
			CheckAlgorithm(fsm.AcceptedOutput, TerminationStatus.Success, fsm, input, Cancellation);
		else
			CheckAlgorithm(fsm.RejectedOutput, TerminationStatus.Stuck, fsm, input, Cancellation);
	}

	[Theory]
	[InlineData("abc_123@def.aa", true)]
	[InlineData("abc_123@@def.aa", false)]
	[InlineData("abc_123", false)]
	public void EmailsGrammar_Test(string input, bool accept)
	{
		var fsm = FiniteStateMachines.Create_EmailGrammar();
		if (accept)
			CheckAlgorithm(fsm.AcceptedOutput, TerminationStatus.Success, fsm, input, Cancellation);
		else
			CheckAlgorithm(fsm.RejectedOutput, TerminationStatus.Stuck, fsm, input, Cancellation);
	}

	[Theory]
	[InlineData("aba->ABA", true)]
	[InlineData("1231=>45666", true)]
	[InlineData("->ABA", true)]
	[InlineData("XXX->", true)]
	[InlineData("->", true)]
	[InlineData("xx->xx->xxx", false)]
	[InlineData("xx--->yy", false)]
	public void MarkovGrammar_Test(string input, bool accept)
	{
		var fsm = FiniteStateMachines.Create_MarkovAlgorithmGrammar();
		if (accept)
			CheckAlgorithm(fsm.AcceptedOutput, TerminationStatus.Success, fsm, input, Cancellation);
		else
			CheckAlgorithm(fsm.RejectedOutput, TerminationStatus.Stuck, fsm, input, Cancellation);
	}
}
