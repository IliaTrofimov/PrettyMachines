using PrettyMachines.Algorithms.Turing;


namespace PrettyMachines.Algorithms.Utils;

public static class PrintingExtensions
{
    /// <summary>Converts movement enum into single character representation.</summary>
    public static char ToChar(this TapeMovement movement) => movement switch
    {
        TapeMovement.None  => 'N',
        TapeMovement.Left  => 'L',
        TapeMovement.Right => 'R',
        _                  => '?'
    };
}