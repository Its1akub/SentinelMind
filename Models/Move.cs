namespace SentinelMind.Models;

public class Move
{
    public int FromCol { get; }
    public int FromRow { get; }
    public int ToCol { get; }
    public int ToRow { get; }
    public Piece? PromotionPiece { get; } = null;

    public Move(int fromCol, int fromRow, int toCol, int toRow, Piece? promotionPiece = null)
    {
        FromCol = fromCol;
        FromRow = fromRow;
        ToCol = toCol;
        ToRow = toRow;

        PromotionPiece = promotionPiece;
    }

    public override string ToString()
        => $"{(char)('a' + FromCol)}{FromRow + 1} -> {(char)('a' + ToCol)}{ToRow + 1}";

}
