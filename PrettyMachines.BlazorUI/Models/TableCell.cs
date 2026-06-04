using System.Diagnostics;


namespace PrettyMachines.BlazorUI.Models;

[DebuggerDisplay("({Row,nq}, {Column,nq}) {Text}")]
public sealed class TableCell
{
    public string? Text { get; set; }
    public int Row { get; }
    public int Column { get; }

    public TableCell(int row, int column, string? text)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row, nameof(row));
        ArgumentOutOfRangeException.ThrowIfNegative(column, nameof(column));
        
        Row = row;
        Column = column;
        Text = text;
    }
    
    public static implicit operator string?(TableCell tableCell) => tableCell.Text;
}