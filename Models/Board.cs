namespace SentinelMind.Models;

public class Board
{
    private readonly string[,] _board;

    public Board()
    {
        _board = new string[8, 8];
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col  = 0; col  < 8; col ++)
            {
                _board[row, col] = "-";
            }
        }
    }

    public override string ToString()
    {
        string result = "";
        for (int row = 0; row < 8; row++)
        {
            for (int col  = 0; col  < 8; col ++)
            {
               result += _board[row, col] +" ";
            }
            result += "\n";
        }
        return result;
    }

    public string[,] InsertFEN(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        string piecePlacement = fen.Split(' ')[0];
        
        var rows = piecePlacement.Split('/');

        for (int row = 0; row < 8; row++)
        {
            int col = 0;

            foreach (char c in rows[row])
            {
                if (char.IsDigit(c))
                {
                    int empty = c - '0';
                    for (int i = 0; i < empty; i++)
                    {
                        _board[row, col++] = "-";
                    }
                }
                else
                {
                    _board[row, col++] = c.ToString();
                }
            }
        }

        return _board;
    }
}