namespace PrettyMachines.Algorithms.Turing;

/// <summary>
/// Fluent builder interface for defining Turing machine transition rules.
/// </summary>
public interface ITuringMachineRuleBuilder
{
    /// <summary>Adds a transition rule for given initial and next states.</summary>
    /// <param name="initialState">Current state.</param>
    /// <param name="scan">Symbol to read from the tape.</param>
    /// <param name="nextState">State to transition to.</param>
    /// <param name="print">Symbol to write to the tape or <c>null</c> to write nothing.</param>
    /// <param name="move">Direction to move the tape head.</param>
    /// <returns>The builder instance for chaining.</returns>
    public ITuringMachineRuleBuilder AddRule(TuringMachineState initialState,
                                             FuzzyKey<string> scan,
                                             TuringMachineState nextState,
                                             string? print,
                                             TapeMovement move = TapeMovement.None);
    
    public TuringMachineState this[string name] { get; }
}


public static class TuringMachineRuleBuilderExtensions
{
    /// <inheritdoc cref="ITuringMachineRuleBuilder.AddRule(TuringMachineState,FuzzyKey{string},TuringMachineState,string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddRule(this ITuringMachineRuleBuilder b,
                                                    string initialState,
                                                    FuzzyKey<string> scan,
                                                    TuringMachineState nextState,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(b[initialState], scan, nextState, print, move);
    }
    
    /// <inheritdoc cref="ITuringMachineRuleBuilder.AddRule(TuringMachineState,FuzzyKey{string},TuringMachineState,string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddRule(this ITuringMachineRuleBuilder b,
                                                    TuringMachineState initialState,
                                                    FuzzyKey<string> scan,
                                                    string nextState,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(initialState, scan, b[nextState], print, move);
    }
    
    /// <inheritdoc cref="ITuringMachineRuleBuilder.AddRule(TuringMachineState,FuzzyKey{string},TuringMachineState,string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddRule(this ITuringMachineRuleBuilder b,
                                                    string initialState,
                                                    FuzzyKey<string> scan,
                                                    string nextState,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(b[initialState], scan, b[nextState], print, move);
    }
    
    // ------------------
    
    /// <inheritdoc cref="ITuringMachineRuleBuilder.AddRule(TuringMachineState,FuzzyKey{string},TuringMachineState,string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddRule(this ITuringMachineRuleBuilder b,
                                                    TuringMachineState initialState,
                                                    string scan,
                                                    TuringMachineState nextState,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(initialState, new FuzzyKey<string>(scan), nextState, print, move);
    }
    
    /// <inheritdoc cref="ITuringMachineRuleBuilder.AddRule(TuringMachineState,FuzzyKey{string},TuringMachineState,string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddRule(this ITuringMachineRuleBuilder b,
                                                    string initialState,
                                                    string scan,
                                                    TuringMachineState nextState,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(b[initialState], new FuzzyKey<string>(scan), nextState, print, move);
    }
    
    /// <inheritdoc cref="ITuringMachineRuleBuilder.AddRule(TuringMachineState,FuzzyKey{string},TuringMachineState,string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddRule(this ITuringMachineRuleBuilder b,
                                                    TuringMachineState initialState,
                                                    string scan,
                                                    string nextState,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(initialState, new FuzzyKey<string>(scan), b[nextState], print, move);
    }
    
    /// <inheritdoc cref="ITuringMachineRuleBuilder.AddRule(TuringMachineState,FuzzyKey{string},TuringMachineState,string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddRule(this ITuringMachineRuleBuilder b,
                                                    string initialState,
                                                    string scan,
                                                    string nextState,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(b[initialState], new FuzzyKey<string>(scan), b[nextState], print, move);
    }
    
    // ------------------
    
    /// <inheritdoc cref="ITuringMachineRuleBuilder.AddRule(TuringMachineState,FuzzyKey{string},TuringMachineState,string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddRule(this ITuringMachineRuleBuilder b,
                                                    TuringMachineState initialState,
                                                    SymbolMatch scan,
                                                    TuringMachineState nextState,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(initialState, new FuzzyKey<string>(null, scan), nextState, print, move);
    }
    
    /// <inheritdoc cref="ITuringMachineRuleBuilder.AddRule(TuringMachineState,FuzzyKey{string},TuringMachineState,string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddRule(this ITuringMachineRuleBuilder b,
                                                    string initialState,
                                                    SymbolMatch scan,
                                                    TuringMachineState nextState,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(b[initialState], new FuzzyKey<string>(null, scan), nextState, print, move);
    }
    
    /// <inheritdoc cref="ITuringMachineRuleBuilder.AddRule(TuringMachineState,FuzzyKey{string},TuringMachineState,string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddRule(this ITuringMachineRuleBuilder b,
                                                    TuringMachineState initialState,
                                                    SymbolMatch scan,
                                                    string nextState,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(initialState, new FuzzyKey<string>(null, scan), b[nextState], print, move);
    }
    
    /// <inheritdoc cref="ITuringMachineRuleBuilder.AddRule(TuringMachineState,FuzzyKey{string},TuringMachineState,string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddRule(this ITuringMachineRuleBuilder b,
                                                    string initialState,
                                                    SymbolMatch scan,
                                                    string nextState,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(b[initialState], new FuzzyKey<string>(null, scan), b[nextState], print, move);
    }
    
    // ------------------
    
    /// <summary>Adds a rule that transitions from current state into default terminal state <see cref="TuringMachineState.Halt"/>.</summary>
    /// <param name="initialState">Current state.</param>
    /// <param name="scan">Symbol to read from the tape.</param>
    /// <param name="print">Symbol to write to the tape or <c>null</c> to write nothing.</param>
    /// <param name="move">Direction to move the tape head.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ITuringMachineRuleBuilder AddHalt(this ITuringMachineRuleBuilder b,
                                                    string initialState,
                                                    FuzzyKey<string> scan,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(initialState, scan, TuringMachineState.Halt, print, move);
    }
    
    /// <inheritdoc cref="AddHalt(ITuringMachineRuleBuilder,string,FuzzyKey{string},string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddHalt(this ITuringMachineRuleBuilder b,
                                                    TuringMachineState initialState,
                                                    FuzzyKey<string> scan,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(initialState, scan, TuringMachineState.Halt, print, move);
    }
    
    // ------------------

    /// <inheritdoc cref="AddHalt(ITuringMachineRuleBuilder,string,FuzzyKey{string},string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddHalt(this ITuringMachineRuleBuilder b,
                                                    string initialState,
                                                    string scan,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(initialState, new FuzzyKey<string>(scan), TuringMachineState.Halt, print, move);
    }
    
    /// <inheritdoc cref="AddHalt(ITuringMachineRuleBuilder,string,FuzzyKey{string},string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddHalt(this ITuringMachineRuleBuilder b,
                                                    TuringMachineState initialState,
                                                    string scan,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(initialState, new FuzzyKey<string>(scan), TuringMachineState.Halt, print, move);
    }
    
    // ------------------

    /// <inheritdoc cref="AddHalt(ITuringMachineRuleBuilder,string,FuzzyKey{string},string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddHalt(this ITuringMachineRuleBuilder b,
                                                    string initialState,
                                                    SymbolMatch scan,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(initialState, new FuzzyKey<string>(null, scan), TuringMachineState.Halt, print, move);
    }
    
    /// <inheritdoc cref="AddHalt(ITuringMachineRuleBuilder,string,FuzzyKey{string},string?,TapeMovement)"/>  
    public static ITuringMachineRuleBuilder AddHalt(this ITuringMachineRuleBuilder b,
                                                    TuringMachineState initialState,
                                                    SymbolMatch scan,
                                                    string? print = null,
                                                    TapeMovement move = TapeMovement.None)
    {
        return b.AddRule(initialState, new FuzzyKey<string>(null, scan), TuringMachineState.Halt, print, move);
    }
}