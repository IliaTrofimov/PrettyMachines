
namespace PrettyMachines.BlazorUI.Helpers;

public static class Utils
{
    public static bool ContainsOne(this string str, string substring)
    {
        var index = str.IndexOf(substring, StringComparison.CurrentCulture);
        if (index == -1) 
            return false;
        
        var nextIndex = str.IndexOf(substring, int.Min(index + 1, str.Length - 1), StringComparison.CurrentCulture);
        return nextIndex == -1; 
    }

    public static bool TryGetAt<T>(this IReadOnlyList<T?> list, int index, out T? value)
    {
        if (index < 0 || index >= list.Count)
        {
            value = default;
            return false;
        }
        
        value = list[index];
        return true;
    }
    
    public static bool TrySet<T>(this IList<T?> list, int index, T? value)
    {
        if (index < 0 || index >= list.Count)
            return false;
        
        list[index] = value;
        return true;
    }

    public static bool HasIndex<T>(this IReadOnlyList<T> list, int index)
    {
        return index >= 0 && index < list.Count;
    }
}