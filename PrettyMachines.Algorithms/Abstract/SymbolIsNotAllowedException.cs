namespace PrettyMachines.Algorithms.Abstract;

public sealed class SymbolIsNotAllowedException : AlgorithmException
{
    public string? Symbol { get; } = "";
    
    
    public SymbolIsNotAllowedException() 
        : base("Symbol is not allowed by an alphabet.")
    {
    }
    
    public SymbolIsNotAllowedException(string message, Exception? innerException) 
        : base(message, innerException)
    {
    }

    public SymbolIsNotAllowedException(string symbol, string? message = null) 
        : this(symbol, message, null)
    {
    }
    
    public SymbolIsNotAllowedException(char symbol, string? message = null) 
        : this(symbol.ToString(), message, null)
    {
    }
    
    public SymbolIsNotAllowedException(object symbol, string? message = null)
        : this(symbol.ToString(), message, symbol.GetType())
    {
    }

    private SymbolIsNotAllowedException(string? symbolStr, string? userMessage, Type? symbolType)
        : base(GetMessage(symbolStr, userMessage, symbolType))
    {
        Symbol = symbolStr;
    }
    
    private static string GetMessage(string? symbolStr, string? userMessage, Type? symbolType)
    {
        if (symbolType is null || typeof(char) == symbolType || typeof(string) == symbolType)
        {
            return string.IsNullOrWhiteSpace(userMessage) 
                ? $"Symbol '{symbolStr}' is not allowed by an alphabet." 
                : $"Symbol '{symbolStr}' is not allowed by an alphabet: {userMessage}.";
        }
        else
        {
            return string.IsNullOrWhiteSpace(userMessage) 
                ? $"Symbol '{symbolStr}' ({symbolType}) is not allowed by an alphabet." 
                : $"Symbol '{symbolStr}' ({symbolType}) is not allowed by an alphabet: {userMessage}.";   
        }
    }
}