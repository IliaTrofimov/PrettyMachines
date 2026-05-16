namespace PrettyMachines.Algorithms.Markov;

/// <summary>Builder that allows adding a comment to the most recently added rule.</summary>
public interface IMarkovSubstitutionBuilder : IMarkovAlgorithmBuilder
{
    /// <summary>Adds a comment to the last added substitution rule.</summary>
    /// <param name="comment">Documentation or note about the rule.</param>
    /// <returns>The parent algorithm builder for further configuration.</returns>
    public IMarkovAlgorithmBuilder WithComment(string? comment);
}