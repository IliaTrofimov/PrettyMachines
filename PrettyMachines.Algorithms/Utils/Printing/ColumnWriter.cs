namespace PrettyMachines.Algorithms.Utils.Printing;

internal sealed class ColumnWriter
{
    private readonly TextOutput _output;

    public char PaddingChar { get; set; }
    public char QuoteChar { get; set; }
    

    public ColumnWriter(TextOutput output, char paddingChar = ' ', char quoteChar = '\'')
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        PaddingChar = paddingChar;
        QuoteChar = quoteChar;
    }

    public void Print(string? text, int minWidth, Alignment alignment = Alignment.Left)
    {
        var delta = minWidth - text?.Length ?? 0;
        
        if (delta <= 0)
        {
            _output.Print(text);
            return;
        }
        
        var (padLeft, padRight) = alignment switch
        {
            Alignment.Left   => (0, delta),
            Alignment.Right  => (delta, 0),
            Alignment.Center => ((int)Math.Floor(delta / 2.0), (int)Math.Ceiling(delta / 2.0)),
        };
        
        _output.PrintN(padLeft, PaddingChar);
        _output.Print(text);
        _output.PrintN(padRight, PaddingChar);
    }
    
    public void PrintQuoted(string? text, int minWidth, Alignment alignment = Alignment.Left)
    {
        var delta = minWidth - text?.Length ?? 0;
        
        if (delta <= 0)
        {
            _output.PrintQuoted(text, QuoteChar);
            return;
        }
        
        var (padLeft, padRight) = alignment switch
        {
            Alignment.Left   => (0, delta),
            Alignment.Right  => (delta, 0),
            Alignment.Center => ((int)Math.Floor(delta / 2.0), (int)Math.Ceiling(delta / 2.0)),
        };
        
        _output.PrintN(padLeft, PaddingChar);
        _output.PrintQuoted(text, QuoteChar);
        _output.PrintN(padRight, PaddingChar);
    }
}