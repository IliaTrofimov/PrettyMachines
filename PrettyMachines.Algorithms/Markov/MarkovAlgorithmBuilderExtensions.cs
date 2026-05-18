namespace PrettyMachines.Algorithms.Markov;

public static class MarkovAlgorithmBuilderExtensions
{
    /// <inheritdoc cref="IMarkovAlgorithmBuilder.WithAlphabet(IEnumerable{char})"/> 
    public static IMarkovAlgorithmBuilder WithAlphabet(this IMarkovAlgorithmBuilder b, params char[] alphabetCharacters)
    {
        return b.WithAlphabet(alphabetCharacters.AsEnumerable());
    }
    
    /// <inheritdoc cref="IMarkovAlgorithmBuilder.WithMarkers(IEnumerable{char})"/> 
    public static IMarkovAlgorithmBuilder WithMarkers(this IMarkovAlgorithmBuilder b, params char[] markerCharacters)
    {
        return b.WithMarkers(markerCharacters);
    }

    /// <summary>Adds new substitution rule with given pattern and replacement strings.</summary>
    /// <param name="pattern">Pattern to match.</param>
    /// <param name="replacement">Replacement string.</param>
    /// <param name="isTerminal">If <c>true</c>, algorithm stops after applying this rule.</param>
    /// <returns>A builder for adding an optional comment.</returns>
    public static IMarkovSubstitutionBuilder AddRule(this IMarkovAlgorithmBuilder b, string pattern, string replacement,
                                                     bool isTerminal = false)
    {
        return b.AddRule(new Substitution(pattern, replacement, isTerminal));
    }

    /// <inheritdoc cref="AddRule(IMarkovAlgorithmBuilder,string,string,bool)"/> 
    public static IMarkovSubstitutionBuilder AddRule(this IMarkovAlgorithmBuilder b, char pattern, string replacement,
                                                     bool isTerminal = false)
    {
        return b.AddRule(new Substitution(pattern.ToString(), replacement, isTerminal));
    }

    /// <inheritdoc cref="AddRule(IMarkovAlgorithmBuilder,string,string,bool)"/> 
    public static IMarkovSubstitutionBuilder AddRule(this IMarkovAlgorithmBuilder b, string pattern, char replacement,
                                                     bool isTerminal = false)
    {
        return b.AddRule(new Substitution(pattern, replacement.ToString(), isTerminal));
    }

    /// <inheritdoc cref="AddRule(IMarkovAlgorithmBuilder,string,string,bool)"/> 
    public static IMarkovSubstitutionBuilder AddRule(this IMarkovAlgorithmBuilder b, char pattern, char replacement,
                                                     bool isTerminal = false)
    {
        return b.AddRule(new Substitution(pattern.ToString(), replacement.ToString(), isTerminal));
    }
}