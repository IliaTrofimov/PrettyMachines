using System.Text;
using PrettyMachines.Implementations.Turing;


namespace PrettyMachines.Implementations.Serialization;

public static class TuringMachineSerializer
{
    public static void ToListView<T>(this InstructionsTable<T> instructionsTable, TextWriter writer)
    {
        var tuples = new List<(string, string)>(instructionsTable.InstructionsCount);
        var maxConditionLength = 0;
        
        foreach (var (condition, action) in instructionsTable.Instructions)
        {
            var condString = condition.ToString();
            if (maxConditionLength < condString.Length)
                maxConditionLength = condString.Length;
            
            var actString = action.ToString();
            tuples.Add((condString, actString));
        }

        var format = $"{{0,{maxConditionLength}}} -> {{1}}";
        foreach (var (condition, action) in  tuples)
            writer.WriteLine(format, condition, action);
    } 
    
    public static void ToListView<T>(this InstructionsTable<T> instructionsTable, StringBuilder builder)
    {
        var tuples = new List<(string, string)>(instructionsTable.InstructionsCount);
        var maxConditionLength = 0;
        
        foreach (var (condition, action) in instructionsTable.Instructions)
        {
            var condString = condition.ToString();
            if (maxConditionLength < condString.Length)
                maxConditionLength = condString.Length;
            
            var actString = action.ToString();
            tuples.Add((condString, actString));
        }

        var format = $"{{0,-{maxConditionLength}}} -> {{1}}\n";
        foreach (var (condition, action) in  tuples)
            builder.AppendFormat(format, condition, action);
    }

    public static string ToListView<T>(this InstructionsTable<T> instructionsTable)
    {
        var sb = new StringBuilder();
        ToListView(instructionsTable, sb);
        return sb.ToString();
    }
}