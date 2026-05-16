namespace PrettyMachines.Algorithms.Abstract;

public class AlgorithmException : Exception
{
    public AlgorithmException() : base("Algorithm has entered invalid state.")
    {
    }

    public AlgorithmException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}