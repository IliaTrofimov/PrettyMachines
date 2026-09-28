using Microsoft.Extensions.Localization;
using PrettyMachines.BlazorUI.Localization;

namespace PrettyMachines.BlazorUI.Services;

internal sealed class AlgorithmLocalizer(IStringLocalizer<Names> localeNames)
{	
	public string GetAlgorithmName(string? algorithmId)
	{
        if (string.IsNullOrEmpty(algorithmId))
			return localeNames["Algorithm"].Value;
            		
		var localized = localeNames[algorithmId];
        return localized.ResourceNotFound
            ? CleanAlgorithmName(algorithmId)
            : localized.Value;
	}

	public string GetAlgorithmFamilyName(string familyId)
	{
        if (string.IsNullOrEmpty(familyId))
			return localeNames["Algorithm"].Value;
            		
		var localized = localeNames[familyId];
        return localized.ResourceNotFound
            ? CleanFamilyName(familyId)
            : localized.Value;
	}

	private static string CleanAlgorithmName(string name)
    {
        var words = SplitPascalCase(name);
        if (words.Count == 0)
            return name;

        words[0] = NormalizeWord(words[0], upperFirst: true);
        for (var i = 1; i < words.Count; i++)
            words[i] = NormalizeWord(words[i], upperFirst: false);

        return string.Join(' ', words);
    }

    private static string CleanFamilyName(string name)
    {
        var words = SplitPascalCase(name);
        if (words.Count == 0)
            return name;

        for (var i = 1; i < words.Count; i++)
            words[i] = NormalizeWord(words[i], upperFirst: false);

        return string.Join(' ', words);
    }

	private static string NormalizeWord(string word, bool upperFirst)
    {
        if (word.Length == 0)
            return word;

        var first = upperFirst ? char.ToUpperInvariant(word[0]) : char.ToLowerInvariant(word[0]);
        return first + word[1..].ToLowerInvariant();
    }

    private static List<string> SplitPascalCase(string value)
    {
        var words = new List<string>();
        if (string.IsNullOrEmpty(value))
            return words;

        var start = 0;
        for (var i = 1; i < value.Length; i++)
        {
            var current = value[i];
            var previous = value[i - 1];
            var isBoundary = char.IsUpper(current) && (
                !char.IsUpper(previous) || 
                (i + 1 < value.Length && char.IsLower(value[i + 1])));

            if (!isBoundary)
                continue;

            words.Add(value[start..i]);
            start = i;
        }

        words.Add(value[start..]);
        return words.Where(w => w.Length > 0).ToList();
    }
}