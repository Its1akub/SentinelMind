namespace SentinelMind.Models;

public class Slot
{
    public int Row { get; }
    public int Col { get; }
    
    public Piece? Piece { get; set; }

    public Slot(int row, int col)
    {
        Row = row;
        Col = col;
        Piece = null;
    }
    
}
