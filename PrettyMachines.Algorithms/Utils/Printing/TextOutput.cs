namespace PrettyMachines.Algorithms.Utils.Printing;

internal abstract class TextOutput
{
    public abstract void Print(string? text);
    public abstract void Print(char character);

    public virtual void Flush()
    {
    }

    public void PrintLine(string? text = null)
    {
        Print(text);
        Print(Environment.NewLine);
    }

    public void PrintQuoted(string? text, char quote)
    {
        if (quote == default(char))
        {
            Print(text);
        }
        else
        {
            Print(quote);
            Print(text);
            Print(quote);   
        }
    }
    
    public void PrintQuoted(char character, char quote)
    {
        if (quote == default(char))
        {
            Print(character);
        }
        else
        {
            Print(quote);
            Print(character);
            Print(quote);   
        }
    }
    
    public void PrintN(int times, string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        
        for (var i = 0; i < times; i++)
            Print(text);
    }
    
    public void PrintN(int times, char character)
    {
        if (character == default(char))
            return;
        
        for (var i = 0; i < times; i++)
            Print(character);
    }
}