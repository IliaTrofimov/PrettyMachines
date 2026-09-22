namespace PrettyMachines.Turing;

/// <summary>Equality comparer that used with <see cref="FuzzyKey{T}"/>.</summary>
/// <param name="valueComparer">Underlying value comparer used to test exact values.</param>
/// <typeparam name="T">Type of the key values.</typeparam>
public sealed class FuzzyKeyComparer<T>(IEqualityComparer<T> valueComparer) : EqualityComparer<FuzzyKey<T>>
{
    public override bool Equals(FuzzyKey<T> x, FuzzyKey<T> y)
    {
        if (x.Match != y.Match) return false;
        return x.Match != SymbolMatch.Exact || valueComparer.Equals(x.Value!, y.Value!);
    }

    public override int GetHashCode(FuzzyKey<T> obj)
    {
        return obj.Match == SymbolMatch.Exact && obj.Value is not null
            ? HashCode.Combine(valueComparer.GetHashCode(obj.Value), obj.Match)
            : HashCode.Combine(obj.Match);
    }
}