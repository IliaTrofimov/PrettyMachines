namespace PrettyMachines.Automata;

/// <summary>Base state of any automaton.</summary>
public class AutomatonState
{
    private string? stringView;


    /// <summary>Unique identifier of the state.</summary>
    public int Id { get; }

    /// <summary>Optional state's name.</summary>
    public string? Name { get; }

    /// <summary>
    /// Returns <c>true</c> if this state is accepting/final.
    /// For Turing machines it means the machine must stop.
    /// </summary>
    public bool IsTerminal { get; }


    /// <summary>Initializes new state with id, optional name and terminal flag.</summary>
    /// <param name="id">Unique identifier of the state.</param>
    /// <param name="name">Optional state's name.</param>
    /// <param name="isTerminal">Indicates that state is accepting/final.</param>
    public AutomatonState(int id, string? name = null, bool isTerminal = false)
    {
        Id = id;
        Name = name;
        IsTerminal = isTerminal;
    }


    /// <inheritdoc cref="object.ToString()"/>
    /// <remarks>
    /// <c>qO1</c> for non-terminal states;<br/>
    /// <c>!qO1</c> for terminal states.
    /// </remarks>
    public override string ToString()
    {
        return stringView ??= IsTerminal ? $"!q{Id:D2}" : $"q{Id:D2}";
    }
}
