namespace PrettyMachines.Automata;

/// <summary>Equality comparer for <see cref="AutomatonState"/> that compares states by their <see cref="AutomatonState.Id"/>.</summary>
public sealed class AutomatonStateComparer : EqualityComparer<AutomatonState>
{
    /// <summary>Gets the shared instance of this comparer.</summary>
    public static AutomatonStateComparer Instance { get; } = new();


    public override bool Equals(AutomatonState? x, AutomatonState? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Id == y.Id;
    }

    public override int GetHashCode(AutomatonState obj)
    {
        return obj.Id.GetHashCode();
    }
}
