namespace SentinelMind.Models;

public class Piece
{
    public PieceType Type { get; }
    public PieceColor Color { get; }

    public Piece(PieceType type, PieceColor color)
    {
        Type = type;
        Color = color;
    }

    public override string ToString()
    {
        string c;
        c = Type switch
        {
            PieceType.Rook => "r",
            PieceType.Queen => "q",
            PieceType.King => "k",
            PieceType.Pawn => "p",
            PieceType.Knight => "n",
            PieceType.Bishop => "b",
            _ => " "
        };

        return Color == PieceColor.White ? c.ToUpper() : c;
    }
}

public enum PieceType
{
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}

public enum PieceColor
{
    White,
    Black
}