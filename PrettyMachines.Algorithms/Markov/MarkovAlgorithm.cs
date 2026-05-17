using System.Diagnostics;
using PrettyMachines.Algorithms.Abstract;

namespace PrettyMachines.Algorithms.Markov;

/// <summary>
/// Implementation of the Normal Markov algorithm.
/// Markov algorithm applies predefined transformation and replaces substrings in the input step by step.
/// </summary>
/// <seealso href="https://en.wikipedia.org/wiki/Markov_algorithm"/>
[DebuggerDisplay("MarkovAlgorithm '{(Name ?? \"_unnamed_\")}', rules: {rules.Count}")]
public class MarkovAlgorithm : IAlgorithm<string>
{
    private string? traceFormat, longTraceFormat;  
        
    private readonly List<Substitution> rules;
    private readonly HashSet<char>? alphabet;
    private readonly HashSet<char>? markers;
    private readonly List<string?>? comments;
    
    /// <inheritdoc/> 
    public string? Name { get; }
    
    /// <summary>Gets the collection of substitution rules.</summary>
    public IReadOnlyList<Substitution> Rules => rules;

    /// <summary>Gets the allowed alphabet characters, or <c>null</c> if not restricted.</summary>
    public IReadOnlySet<char>? Alphabet => alphabet;
    
    /// <summary>Gets the marker characters used in rules, or <c>null</c> if none.</summary>
    /// <remarks>Algorithm can produce such symbols, but input text can't have them.</remarks>
    public IReadOnlySet<char>? Markers => markers;
    
    
    /// <summary>Creates a new builder instance for constructing Markov algorithms.</summary>
    /// <param name="name">Optional name for the algorithm.</param>
    /// <returns>A builder for fluent configuration.</returns>
    public static IMarkovAlgorithmBuilder Create(string? name = null) => new MarkovAlgorithmBuilder(name);
    
    
    /// <summary>Private constructor for builder pattern.</summary>
    private MarkovAlgorithm(string? name, List<Substitution> rules, HashSet<char>? alphabet, HashSet<char>? markers, List<string?>? comments = null)
    {
        Name = name;
        this.alphabet = alphabet;
        this.rules = rules;
        this.markers = markers;
        this.comments = comments;
    }
    
    /// <summary>Creates a deep copy of another Markov algorithm.</summary>
    /// <param name="other">The algorithm to copy.</param>
    public MarkovAlgorithm(MarkovAlgorithm other)
    {
        Name = other.Name;
        rules = [..other.rules];
        comments = other.comments != null ? [..other.comments] : null;
        alphabet = other.alphabet != null ? [..other.alphabet] : null;
        markers = other.markers != null ? [..other.markers] : null;
    }
    
    /// <summary>Creates new concatenation of two algorithms.</summary>
    /// <returns>New instance of <see cref="AlgorithmConcatenation{MarkovAlgorithm, T}"/> object with 2 added algorithms.</returns>
    public static AlgorithmConcatenation<MarkovAlgorithm, string> operator+(MarkovAlgorithm a, MarkovAlgorithm b)
    {
        return new AlgorithmConcatenation<MarkovAlgorithm, string>(a, b);
    }
    

    /// <summary>Finds the first rule that matches the given input string.</summary>
    /// <param name="input">The string to test.</param>
    /// <returns>Index of the matching rule, or -1 if none match.</returns>
    public int FindMatchingRule(string input)
    {
        return rules.FindIndex(rule => rule.Matches(input));
    }
    
    /// <summary>Gets the comment associated with a rule.</summary>
    /// <returns>The comment text, or null if it's not set.</returns>
    public string? GetRuleComment(int index) => comments?[index];
    
    /// <summary>Applies a single step of this algorithm.</summary>
    /// <param name="input">Input string to transform.</param>
    /// <param name="matchedRule">Outputs the rule that was applied.</param>
    /// <returns>Updated string.</returns>
    public string NextStep(string input, out Substitution? matchedRule)
    {
        matchedRule = rules.Find(rule => rule.TrySubstitute(ref input));
        return input;
    }

    /// <inheritdoc/> 
    public AlgorithmResult<string> Execute(string input, AlgorithmCancellation cancellation, bool verbose = false)
    {
        if (!ValidateInput(input))
            return new AlgorithmResult<string>(TerminationStatus.InvalidInput, input);
        
        uint steps = 0;
        var result = input;
        var status = TerminationStatus.Aborted;
        List<string>? trace = verbose ? [] : null;
        trace?.Add(CreateFirstTrace(input));
        
        while (cancellation.ShouldContinue(steps++))
        {
            result = NextStep(result, out var matchedRule);
            trace?.Add(CreateTrace(result, matchedRule));
            
            if (matchedRule is null)
            {
                status = TerminationStatus.Stuck;
                break;
            }
            if (matchedRule.IsTerminal)
            {
                status = TerminationStatus.Success;
                break;
            }
        }

        return new AlgorithmResult<string>(status, result, steps, trace);
    }

    private bool ValidateInput(string input)
    {
        return alphabet == null || string.IsNullOrEmpty(input) || input.All(alphabet.Contains);
    }
    
    private string CreateFirstTrace(string input)
    {
        InitializeTraceFormatStrings();
        return string.Format(longTraceFormat!, "in", "", input, "<input>");
    }
    
    private string CreateTrace(string stepResult, Substitution? rule)
    {
        InitializeTraceFormatStrings();
        
        if (rule is null)
            return string.Format(traceFormat!, "??", "", stepResult);
        
        var ruleId = rules.IndexOf(rule);
        var comment = GetRuleComment(ruleId);
        
        return string.IsNullOrEmpty(comment)
            ? string.Format(traceFormat!, ruleId, rule, stepResult)
            : string.Format(longTraceFormat!, ruleId, rule, stepResult, comment);
    }

    private void InitializeTraceFormatStrings()
    {
        if (traceFormat == null)
        {
            var length = rules.Max(r => r.ToString().Length);
            traceFormat = $"R_{{0:d2}}, {{1,-{length}}}, '{{2}}'";
            longTraceFormat = traceFormat + " // {3}";
        }
    }

    #region Builder class
    
    private sealed class MarkovAlgorithmBuilder : IMarkovSubstitutionBuilder, IMarkovAlgorithmBuilder
    {
        private readonly string? name;
        private readonly List<Substitution> rules = [];
        private List<string?>? comments;
        private HashSet<char>? alphabet;
        private HashSet<char>? markers;

        internal MarkovAlgorithmBuilder(string? name) => this.name = name;
        
        public IMarkovAlgorithmBuilder WithAlphabet(IEnumerable<char> alphabetCharacters)
        {
            alphabet = [..alphabetCharacters];
            if (alphabet.Count == 0)
                throw new ArgumentException("Alphabet cannot be empty.", nameof(alphabetCharacters));
            return this;
        }
        
        public IMarkovAlgorithmBuilder WithAlphabet(params char[] alphabetCharacters)
        {
            return WithAlphabet((IEnumerable<char>)alphabetCharacters);
        }
        
        public IMarkovAlgorithmBuilder WithMarkers(IEnumerable<char> makerCharacters)
        {
            markers = [..makerCharacters];
            return this;
        }
        
        public IMarkovAlgorithmBuilder WithMarkers(params char[] makerCharacters)
        {
            return WithMarkers((IEnumerable<char>)makerCharacters);
        }
        
        public IMarkovSubstitutionBuilder AddRule(Substitution rule)
        {
            if (!rules.Contains(rule))
            {
                rules.Add(rule);
            }
            return this;
        }

        public IMarkovAlgorithmBuilder WithComment(string? comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return this;
            
            comments ??= new List<string?>(rules.Count);
            InitializeComments();
            comments[^1] = comment;
            return this;
        }

        public IMarkovSubstitutionBuilder AddRule(string pattern, string replacement, bool isTerminal = false)
        {
            return AddRule(new Substitution(pattern, replacement, isTerminal));
        }
        
        public IMarkovSubstitutionBuilder AddRule(char pattern, string replacement, bool isTerminal = false)
        {
            return AddRule(new Substitution(pattern.ToString(), replacement, isTerminal));
        }
        
        public IMarkovSubstitutionBuilder AddRule(string pattern, char replacement, bool isTerminal = false)
        {
            return AddRule(new Substitution(pattern, replacement.ToString(), isTerminal));
        }
        
        public IMarkovSubstitutionBuilder AddRule(char pattern, char replacement, bool isTerminal = false)
        {
            return AddRule(new Substitution(pattern.ToString(), replacement.ToString(), isTerminal));
        }
        
        public MarkovAlgorithm Build(string? name = null)
        {
            ValidateAlphabet();
            InitializeComments();
            return new MarkovAlgorithm(name ?? this.name, rules, alphabet, markers, comments);
        }
        
        private void ValidateAlphabet()
        {
            if (alphabet == null && markers == null)
                return;
            
            for (var i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];

                if (FindInvalidChar(rule.Pattern, out var c1))
                    throw new SymbolIsNotAllowedException(c1, $"Rule_{i} {rule} has invalid symbol in its pattern string.");
                
                if (FindInvalidChar(rule.Replacement, out var c2))
                    throw new SymbolIsNotAllowedException(c2, $"Rule_{i} {rule} has invalid symbol in its replacement string.");
            }

            return;
            
            bool FindInvalidChar(string str, out char ch)
            {
                for (var i = 0; i < str.Length; i++)
                {
                    ch = str[i];
                    if (!(alphabet?.Contains(ch) == true || markers?.Contains(ch) == true))
                        return true;
                }

                ch = '\0';
                return false;
            }
        }
        
        private void InitializeComments()
        {
            if (comments == null)
                return;
            
            for (var i = comments.Count; i <= rules.Count - 1; i++)
                comments.Add(null);
        }
    }
    
    #endregion
}