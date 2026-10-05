using PrettyMachines.FSM;
using PrettyMachines.Implementations.Catalog;

namespace PrettyMachines.Implementations;

public static class FiniteStateMachines
{
	private static readonly char[] chars = ['a', 'b', 'c', 'd', 'e', 'f'];
	private static readonly char[] digits = [ '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];


	/// <summary>Creates simple FSM that recognizes URLs.</summary>
	/// <remarks>Accepts characters from 'a' to 'f' for simplicity.</remarks>
	[AlgorithmBuilder("UrlGrammar")]
	public static FiniteStateMachine Create_UrlGrammar()
	{
		var builder = FiniteStateMachine.Create("URL grammar")
			.AddInitialState("qScheme1")
			.AddTerminalState("qPath2")
			.BuildRules(b =>
			{
				// https?://
				b.AddRule("qScheme1", 'h', "qScheme2")
				 .AddRule("qScheme2", 't', "qScheme3")
				 .AddRule("qScheme3", 't', "qScheme4")
				 .AddRule("qScheme4", 'p', "qScheme5")
				 .AddRule("qScheme5", 's', "qScheme6")
				 .AddRule("qScheme5", ':', "qScheme7")
				 .AddRule("qScheme6", ':', "qScheme7")
				 .AddRule("qScheme7", '/', "qScheme8")
				 .AddRule("qScheme8", '/', "qPath1");
				// [a-f0-9]
				b.AddRule("qPath1", chars, "qPath2");
				b.AddRule("qPath1", digits, "qPath2");
				// [a-f0-9./]
				b.AddRule("qPath2", '.', "qPath1")
				 .AddRule("qPath2", '/', "qPath1")
				 .AddRule("qPath2", chars, "qPath2")
				 .AddRule("qPath2", digits, "qPath2");
			});
		return builder;
	}

	/// <summary>Creates simple FSM that recognizes emails addresses.</summary>
	/// <remarks>Accepts characters from 'a' to 'f' for simplicity.</remarks>
	[AlgorithmBuilder("EmailGrammar")]
	public static FiniteStateMachine Create_EmailGrammar()
	{
		var builder = FiniteStateMachine.Create("Email grammar")
			.AddInitialState("qLocal1")
			.AddTerminalState("qDomain4")
			.BuildRules(b =>
			{
				b.AddRule("qLocal1", chars, "qLocal2");

				b.AddRule("qLocal2", chars, "qLocal2")
				 .AddRule("qLocal2", digits, "qLocal2")
				 .AddRule("qLocal2", '_', "qLocal2")
				 .AddRule("qLocal2", '.', "qLocal3")
				 .AddRule("qLocal2", '@', "qDomain1");

				b.AddRule("qLocal3", chars, "qLocal2")
				 .AddRule("qLocal3", digits, "qLocal2");

				b.AddRule("qDomain1", chars, "qDomain2");

				b.AddRule("qDomain2", chars, "qDomain2")
				 .AddRule("qDomain2", digits, "qDomain2")
				 .AddRule("qDomain2", '.', "qDomain3");

				b.AddRule("qDomain3", chars, "qDomain4");

				b.AddRule("qDomain4", chars, "qDomain4")
				 .AddRule("qDomain4", digits, "qDomain4")
				 .AddRule("qDomain4", '.', "qDomain3");
			});
		return builder;
	}

	/// <summary>Creates FSM that recognizes rules for Markov algorithms.</summary>
	/// <remarks>Accepts strings like <c>[empty or any except "->" or "=>"] ["->" or "=>"] [empty or any except "->" or "=>"]</c>.</remarks>
	[AlgorithmBuilder("MarkovAlgorithmGrammar")]
	public static FiniteStateMachine Create_MarkovAlgorithmGrammar()
	{
		var builder = FiniteStateMachine.Create("Markov algorithm rules grammar")
			.AddInitialState("qPattern")
			.AddTerminalState("qReplacement")
			.BuildRules(b =>
			{
				b.AddRule("qPattern", Automata.SymbolMatch.Any, "qPattern");
				b.AddRule("qPattern", '-', "qArrow");
				b.AddRule("qPattern", '=', "qArrow");
				b.AddRule("qArrow", '>', "qReplacement");

				b.AddRule("qReplacement", Automata.SymbolMatch.Any, "qReplacement");
				b.AddRule("qReplacement", '-', "qWrongArrow");
				b.AddRule("qReplacement", '=', "qWrongArrow");

				b.AddRule("qWrongArrow", '>', "qReject");
				b.AddRule("qWrongArrow", Automata.SymbolMatch.Any, "qReplacement");
			});
		return builder;
	}
}