using PrettyMachines.Algorithms.Abstract;
using PrettyMachines.Algorithms.Markov;
using PrettyMachines.BlazorUI.Models.Drafts;


namespace PrettyMachines.BlazorUI.Services.DraftAdapters;

/// <summary>Converts between <see cref="MarkovAlgorithm"/> instances and editable <see cref="MarkovAlgorithmDraft"/> definitions.</summary>
public static class MarkovDraftAdapter
{
    /// <summary>Creates an editable draft from an existing algorithm.</summary>
    /// <param name="algorithm">Algorithm to read.</param>
    /// <returns>A new draft that mirrors the algorithm.</returns>
    public static MarkovAlgorithmDraft From(MarkovAlgorithm algorithm)
    {
        ArgumentNullException.ThrowIfNull(algorithm);

        var draft = new MarkovAlgorithmDraft { Name = algorithm.Name ?? "Markov algorithm" };

        if (algorithm.Alphabet is not null)
            draft.Alphabet.AddRange(algorithm.Alphabet);
        if (algorithm.Markers is not null)
            draft.Markers.AddRange(algorithm.Markers);

        for (var i = 0; i < algorithm.Rules.Count; i++)
        {
            var rule = algorithm.Rules[i];
            draft.Rules.Add(new DraftRule
            {
                Pattern = rule.Pattern,
                Replacement = rule.Replacement,
                IsTerminal = rule.IsTerminal,
                Comment = algorithm.GetRuleComment(i),
            });
        }

        return draft;
    }

    /// <summary>Builds an algorithm from an editable draft.</summary>
    /// <param name="draft">Draft to build.</param>
    /// <returns>A new immutable algorithm instance.</returns>
    /// <exception cref="InvalidOperationException">The draft is not a valid Markov algorithm.</exception>
    public static MarkovAlgorithm ToAlgorithm(MarkovAlgorithmDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        Validate(draft);

        var builder = MarkovAlgorithm.Create(string.IsNullOrWhiteSpace(draft.Name) ? null : draft.Name.Trim());
        if (draft.Alphabet.Count > 0)
            builder = builder.WithAlphabet(draft.Alphabet);
        if (draft.Markers.Count > 0)
            builder = builder.WithMarkers(draft.Markers);

        foreach (var rule in draft.Rules)
        {
            var substitution = builder.AddRule(new Substitution(rule.Pattern, rule.Replacement, rule.IsTerminal));
            builder = string.IsNullOrWhiteSpace(rule.Comment)
                ? substitution
                : substitution.WithComment(rule.Comment);
        }

        try
        {
            return builder.Build();
        }
        catch (Exception exception) when (exception is AlgorithmException or ArgumentException or InvalidOperationException)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    /// <summary>Verifies that a draft can be built into an algorithm.</summary>
    /// <param name="draft">Draft to validate.</param>
    /// <exception cref="InvalidOperationException">The draft is not a valid Markov algorithm.</exception>
    public static void Validate(MarkovAlgorithmDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (draft.Rules.Count == 0)
            throw new InvalidOperationException("A Markov algorithm must declare at least one rule.");

        var duplicate = draft.Rules
            .GroupBy(rule => rule.Pattern, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new InvalidOperationException(
                $"Every rule must have a unique pattern; '{duplicate.Key}' is used more than once.");
    }
}
