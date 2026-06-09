using PrettyMachines.Algorithms.Utils;


namespace PrettyMachines.Algorithms.Turing;

/// <summary>
/// Represents single action that Turing machine takes executing some instruction.   
/// </summary>
public readonly struct TuringMachineAction
{
    /// <summary>Gets the state to transition to after this action.</summary>
    public TuringMachineState NextState { get; } = TuringMachineState.Halt;

    /// <summary>Gets the symbol to print into the current tape cell or <c>null</c> to print nothing.</summary>
    public string? PrintedSymbol { get; } = default;
    
    /// <summary>Gets the direction to move the tape head.</summary>
    public TapeMovement Movement { get; } = TapeMovement.None;
    
    
    /// <summary>Default HALT action that just terminates execution of an algorithm.</summary>
    public static TuringMachineAction Halt { get; } = new(TuringMachineState.Halt);
    
    
    /// <summary>
    /// Creates new action that prints symbol and moves the tape.
    /// </summary>
    /// <param name="nextState">State to transition to after this action</param>
    /// <param name="printedSymbol">Symbol to print into the tape or <c>null</c> to print nothing.</param>
    /// <param name="movement">Direction to move the tape head</param>
    public TuringMachineAction(TuringMachineState nextState, string? printedSymbol, TapeMovement movement = TapeMovement.None)
    {
        NextState = nextState;
        PrintedSymbol = printedSymbol;
        Movement = movement;
    }
    
    /// <summary>
    /// Creates new action that moves the tape and doesn't print anything.
    /// </summary>
    /// <param name="nextState">State to transition to after this action</param>
    /// <param name="movement">Direction to move the tape head</param>
    public TuringMachineAction(TuringMachineState nextState, TapeMovement movement = TapeMovement.None) 
        : this(nextState, null, movement)
    {
    }
    
    /// <summary>
    /// Creates new terminating <see cref="TuringMachineAction"/> that can print symbol and move tape before stopping an algorithm. 
    /// </summary>
    /// <param name="printedSymbol">Symbol to print.</param>
    /// <param name="movement">Direction to move the tape.</param>
    public static TuringMachineAction CreateHalt(string? printedSymbol, TapeMovement movement = TapeMovement.None)
    {
        return new(TuringMachineState.Halt, printedSymbol, movement);
    }
    
    /// <inheritdoc cref="object.ToString()"/>
    ///<inheritdoc cref="ToFormattedString()"/>
    public override string ToString() => ToFormattedString();
    
    /// <summary>
    /// Outputs string representation of this object with specified quoting character for <see cref="PrintedSymbol"/>.
    /// </summary>
    /// <remarks>
    /// <c>R q01</c>, <c>L q01</c> or <c>N q01</c> when action doesn't print symbol;<br/>
    /// <c>'x' R q01</c> when action prints symbol;<br/>
    /// </remarks>
    public string ToFormattedString(char quote = '\'')
    {
        return PrintedSymbol != null 
            ? $"{quote}{PrintedSymbol}{quote} {Movement.ToChar()} {NextState}" 
            : $"{Movement.ToChar()} {NextState}";
    }
}