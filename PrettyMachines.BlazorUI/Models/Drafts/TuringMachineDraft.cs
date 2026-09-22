using PrettyMachines.Turing;


namespace PrettyMachines.BlazorUI.Models.Drafts;

/// <summary>Mutable, editable definition of a Turing machine.</summary>
public sealed class TuringMachineDraft
{
    /// <summary>Gets or sets the algorithm name.</summary>
    public string Name { get; set; } = "New Turing machine";

    /// <summary>Gets the editable alphabet symbols (never contains the blank symbol).</summary>
    public List<string> Alphabet { get; } = [];

    /// <summary>Gets or sets a value indicating whether this machine declares a blank symbol.</summary>
    public bool HasBlankSymbol { get; set; } = true;

    /// <summary>Gets or sets the blank symbol. Only meaningful when <see cref="HasBlankSymbol"/> is <c>true</c>.</summary>
    public string BlankSymbol { get; set; } = "_";

    /// <summary>Gets or sets a value indicating whether unknown symbols must fail the machine.</summary>
    public bool StrictAlphabet { get; set; } = true;

    /// <summary>Gets the states ordered by position.</summary>
    public List<DraftState> States { get; } = [];

    /// <summary>Gets the transition rules.</summary>
    public List<DraftTransition> Transitions { get; } = [];


    /// <summary>Returns the display name of a state index, or <c>Halt</c> for the default terminal state.</summary>
    /// <param name="index">State index or <c>-1</c> for <see cref="TuringMachineState.Halt"/>.</param>
    public string GetStateName(int index)
    {
        if (index == -1)
            return "Halt";
        return index >= 0 && index < States.Count ? $"q{index:D2} {States[index].Name}".Trim() : "?";
    }
}


/// <summary>Editable Turing machine state.</summary>
public sealed class DraftState
{
    /// <summary>Gets or sets the optional state name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Gets or sets a value indicating whether this is the initial state.</summary>
    public bool IsInitial { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a terminal state.</summary>
    public bool IsTerminal { get; set; }
}


/// <summary>Editable Turing machine transition rule.</summary>
public sealed class DraftTransition
{
    /// <summary>Gets or sets the index of the state this rule starts from.</summary>
    public int StateIndex { get; set; }

    /// <summary>Gets or sets how the scanned symbol is matched.</summary>
    public SymbolMatch SymbolMatch { get; set; } = SymbolMatch.Exact;

    /// <summary>Gets or sets the exact scanned symbol (used when <see cref="SymbolMatch"/> is <see cref="SymbolMatch.Exact"/>).</summary>
    public string SymbolValue { get; set; } = "";

    /// <summary>Gets or sets the next state index, or <c>-1</c> to transition to <see cref="TuringMachineState.Halt"/>.</summary>
    public int NextStateIndex { get; set; } = -1;

    /// <summary>Gets or sets a value indicating whether this rule prints a symbol.</summary>
    public bool Prints { get; set; }

    /// <summary>Gets or sets the symbol to print. Only meaningful when <see cref="Prints"/> is <c>true</c>.</summary>
    public string Print { get; set; } = "";

    /// <summary>Gets or sets the tape head movement.</summary>
    public TapeMovement Movement { get; set; } = TapeMovement.None;
}
