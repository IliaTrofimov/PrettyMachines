namespace PrettyMachines.Algorithms.Markov;

/// <summary>Fluent builder for constructing Markov algorithms.</summary>
public interface IMarkovAlgorithmBuilder
{
    /// <summary>
    /// Sets the alphabet. Restricts the algorithm to only process strings containing these characters.
    /// </summary>
    /// <param name="alphabetCharacters">Characters allowed in input strings.</param>
    /// <returns>The builder for chaining.</returns>
    public IMarkovAlgorithmBuilder WithAlphabet(IEnumerable<char> alphabetCharacters);

    /// <inheritdoc cref="WithAlphabet(IEnumerable{char})"/> 
    public IMarkovAlgorithmBuilder WithAlphabet(params char[] alphabetCharacters);
    
    /// <summary>Defines marker characters used as special symbols in substitution rules.</summary>
    /// <param name="markerCharacters">Characters treated as markers in rules.</param>
    /// <returns>The builder for chaining.</returns>
    public IMarkovAlgorithmBuilder WithMarkers(IEnumerable<char> markerCharacters);

    /// <inheritdoc cref="WithMarkers(IEnumerable{char})"/> 
    public IMarkovAlgorithmBuilder WithMarkers(params char[] markerCharacters);

    /// <summary>Adds a substitution rule to the algorithm.</summary>
    /// <param name="rule">The rule to add.</param>
    /// <returns>A builder for adding an optional comment.</returns>
    public IMarkovSubstitutionBuilder AddRule(Substitution rule);
        
    /// <summary>Adds new substitution rule with given pattern and replacement strings.</summary>
    /// <param name="pattern">Pattern to match.</param>
    /// <param name="replacement">Replacement string.</param>
    /// <param name="isTerminal">If <c>true</c>, algorithm stops after applying this rule.</param>
    /// <returns>A builder for adding an optional comment.</returns>
    public IMarkovSubstitutionBuilder AddRule(string pattern, string replacement, bool isTerminal = false);

    /// <inheritdoc cref="AddRule(string,string,bool)"/> 
    public IMarkovSubstitutionBuilder AddRule(char pattern, string replacement, bool isTerminal = false);

    /// <inheritdoc cref="AddRule(string,string,bool)"/> 
    public IMarkovSubstitutionBuilder AddRule(string pattern, char replacement, bool isTerminal = false);

    /// <inheritdoc cref="AddRule(string,string,bool)"/> 
    public IMarkovSubstitutionBuilder AddRule(char pattern, char replacement, bool isTerminal = false);

    /// <summary>Builds Markov algorithm instance.</summary>
    /// <returns>A configured <see cref="MarkovAlgorithm"/> ready for execution.</returns>
    public MarkovAlgorithm Build(string? name = null);
}