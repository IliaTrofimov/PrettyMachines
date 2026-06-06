using System.Text;


namespace PrettyMachines.Algorithms.Utils.Printing;

internal class StringBuilderOutput : TextOutput
{
    private readonly StringBuilder _sb;

    public StringBuilderOutput(StringBuilder? sb = null)
    {
        _sb = sb ?? new StringBuilder();
    }

    public override void Print(string? text) => _sb.Append(text);

    public override void Print(char character) => _sb.Append(character);
}