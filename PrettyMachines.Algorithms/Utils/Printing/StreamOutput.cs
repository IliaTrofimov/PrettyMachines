using System.Diagnostics;
using System.Text;


namespace PrettyMachines.Algorithms.Utils.Printing;

[DebuggerDisplay("Position: {_sw.BaseStream.Position,nq}, printed: {_printedLength,nq} symbols")]
internal sealed class StreamOutput : TextOutput, IDisposable
{
    private readonly StreamWriter _sw;
    private int _printedLength; 
    
    public StreamOutput(Stream stream, bool leaveOpen = false, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
            throw new ArgumentException("Stream is not writable", nameof(stream));
        
        _sw = new StreamWriter(stream, leaveOpen: leaveOpen, encoding: encoding);
        _sw.AutoFlush = true;
    }

    public override void Print(string? text)
    {
        _sw.Write(text);
        _printedLength += text?.Length ?? 0;
    }
    
    public override void Print(char character) 
    {
        _sw.Write(character);
        _printedLength++;
    }
    
    public override void Flush()
    {
        _sw.Flush();
    }

    public void Dispose()
    {
        _sw.Flush();
        _sw.Dispose();
    }
}