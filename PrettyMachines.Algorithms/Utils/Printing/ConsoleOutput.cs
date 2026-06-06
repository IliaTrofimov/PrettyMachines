namespace PrettyMachines.Algorithms.Utils.Printing;

internal sealed class ConsoleOutput : TextOutput
{
    public override void Print(string? text) => Console.Write(text);
    
    public override void Print(char character) => Console.Write(character);
}