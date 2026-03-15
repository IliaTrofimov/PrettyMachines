namespace PrettyMachines.Implementations.Turing;

/// <summary>Turing machine's tape values comparision type: exact or soft.</summary>
public enum SymbolAcceptance
{
    ExactValue = 0, 
    NotEmptyValue,
    EmptyValue,
    AnyValue
}