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
    
    /// <summary>Defines marker characters used as special symbols in substitution rules.</summary>
    /// <param name="markerCharacters">Characters treated as markers in rules.</param>
    /// <returns>The builder for chaining.</returns>
    public IMarkovAlgorithmBuilder WithMarkers(IEnumerable<char> markerCharacters);

    /// <summary>Adds a substitution rule to the algorithm.</summary>
    /// <param name="rule">The rule to add.</param>
    /// <returns>A builder for adding an optional comment.</returns>
    public IMarkovSubstitutionBuilder AddRule(Substitution rule);
    
    /// <summary>Builds Markov algorithm instance.</summary>
    /// <returns>A configured <see cref="MarkovAlgorithm"/> ready for execution.</returns>
    public MarkovAlgorithm Build(string? name = null);
}