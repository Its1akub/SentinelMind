using SentinelMind.AI;

namespace SentinelMind.Models;

public class Board
{
    private readonly Slot[,] _board;
    public bool WhiteCanCastleKingSide;
    public bool WhiteCanCastleQueenSide;
    public bool BlackCanCastleKingSide;
    public bool BlackCanCastleQueenSide;
    public (int r, int c)? EnPassantTarget;

    public Board()
    {
        _board = new Slot[8, 8];
        GenerateBoard();
    }
    
    public Slot[,] Squares => _board;
    
    private void GenerateBoard()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col  = 0; col  < 8; col ++)
            {
                _board[row, col] = new Slot(row, col);
            }
        }
    }

    public override string ToString()
    {
        string result = "";
        for (int row = 7; row >= 0; row--)
        {
            for (int col  = 0; col  < 8; col ++)
            {
               var piece = Squares[row, col].Piece;
               result += piece == null ? "-" : piece.ToString();
            }
            result += "\n";
        }
        return result;
    }

    public void InsertFEN(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        GenerateBoard();
        var parts = fen.Split(' ');
        
        string piecePlacement = parts[0];
        var rows = piecePlacement.Split('/');

        for (int row = 0; row < 8; row++)
        {
            int fenRow = 7 - row;
            int col = 0;

            foreach (char c in rows[fenRow])
            {
                if (char.IsDigit(c))
                {
                    int emptySlots  = c - '0';
                    for (int i = 0; i < emptySlots; i++)
                    {
                        _board[row, col++].Piece = null;
                    }
                }
                else
                {
                    _board[row, col++].Piece = CharToPiece(c);
                }
            }
        }
        string castling = parts[2];
        WhiteCanCastleKingSide = castling.Contains('K');
        WhiteCanCastleQueenSide = castling.Contains('Q');
        BlackCanCastleKingSide = castling.Contains('k');
        BlackCanCastleQueenSide = castling.Contains('q');
        
        string ep = parts[3];
        if (ep != "-")
        {
            int file = ep[0] - 'a';
            int rank = 8 - (ep[1] - '0');
            EnPassantTarget = (rank, file);
        }
        else
        {
            EnPassantTarget = null;
        }
    }
    
    private Piece CharToPiece(char c)
    {
        PieceColor color = char.IsUpper(c) ? PieceColor.White : PieceColor.Black;
        char lower = char.ToLower(c);

        PieceType type = lower switch
        {
            'p' => PieceType.Pawn,
            'r' => PieceType.Rook,
            'n' => PieceType.Knight,
            'b' => PieceType.Bishop,
            'q' => PieceType.Queen,
            'k' => PieceType.King,
            _ => throw new Exception("Invalid piece in FEN: " + c),
        };

        return new Piece(type, color);
    }

    
    public Board Copy()
    {
        var newBoard = new Board();
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var piece = Squares[row, col].Piece;
                newBoard.Squares[row, col].Piece = piece != null 
                    ? new Piece(piece.Type, piece.Color) 
                    : null;
            }
        }
        newBoard.WhiteCanCastleKingSide = this.WhiteCanCastleKingSide;
        newBoard.WhiteCanCastleQueenSide = this.WhiteCanCastleQueenSide;
        newBoard.BlackCanCastleKingSide = this.BlackCanCastleKingSide;
        newBoard.BlackCanCastleQueenSide = this.BlackCanCastleQueenSide;
        newBoard.EnPassantTarget = this.EnPassantTarget;
        
        return newBoard;
    }

    public Piece? GetPieceAt(int row, int col)
    {
        if (!IsInsideBoard(row, col)) return null;
        return _board[row, col].Piece;
    }
    
    public void SetPieceAt(int row, int col, Piece? piece)
    {
        if (!IsInsideBoard(row, col)) return;
        _board[row, col].Piece = piece;
    }
    
    public bool IsInsideBoard(int r, int c) =>
        r >= 0 && r < 8 && c >= 0 && c < 8;
    
    public void MakeMove(Move move)
    {
        var piece = GetPieceAt(move.FromRow, move.FromCol);
        if (piece == null)
            throw new Exception($"Chyba: žádná figurka na startovním poli {move.FromRow},{move.FromCol}");

        SetPieceAt(move.FromRow, move.FromCol, null);
        SetPieceAt(move.ToRow, move.ToCol, piece);
    }
    
    public List<Move> GenerateLegalMoves(PieceColor color)
    {
        List<Move> allMoves = new List<Move>();
        object lockObj = new object();

        Parallel.For(0, 8, r =>
        {
            List<Move> localMoves = new List<Move>();

            for (int c = 0; c < 8; c++)
            {
                Piece? piece = GetPieceAt(r, c);
                if (piece == null) continue;
                if (piece.Color != color) continue;

                localMoves.AddRange(GenerateMovesForPiece(r, c, piece));
            }

            lock (lockObj)
            {
                allMoves.AddRange(localMoves);
            }
        });

        return allMoves;
    }
    
    public List<Move> GenerateMovesForPiece(int r, int c, Piece piece)
    {
        return piece.Type switch
        {
            PieceType.Pawn   => GeneratePawnMoves(r, c, piece),
            PieceType.Rook   => GenerateRookMoves(r, c, piece),
            PieceType.Knight => GenerateKnightMoves(r, c, piece),
            PieceType.Bishop => GenerateBishopMoves(r, c, piece),
            PieceType.Queen  => GenerateQueenMoves(r, c, piece),
            PieceType.King   => GenerateKingMoves(r, c, piece),
            _ => new List<Move>()
        };
    }
    private List<Move> GeneratePawnMoves(int r, int c, Piece pawn)
    {
        List<Move> moves = new List<Move>();
        int dir = pawn.Color == PieceColor.White ? 1 : -1; 

        int startRow = pawn.Color == PieceColor.White ? 1 : 6; 

        
        if (IsInsideBoard(r + dir, c) && GetPieceAt(r + dir, c) == null)
            moves.Add(new Move(c, r, c,r + dir));

        
        if (r == startRow && GetPieceAt(r + dir, c) == null && GetPieceAt(r + 2 * dir, c) == null)
            moves.Add(new Move(c, r, c, r + 2 * dir));
        
        foreach (int dc in new int[] { -1, 1 })
        {
            int nr = r + dir;
            int nc = c + dc;
            if (!IsInsideBoard(nr, nc)) continue;
            var target = GetPieceAt(nr, nc);
            if (target != null && target.Color != pawn.Color)
                moves.Add(new Move(c, r, nc, nr));
        }

        return moves;
    }

    
    private List<Move> GenerateKnightMoves(int r, int c, Piece knight)
    {
        List<Move> moves = new List<Move>();
        int[,] offsets = { {1,2}, {2,1}, {2,-1}, {1,-2}, {-1,-2}, {-2,-1}, {-2,1}, {-1,2} };

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int nr = r + offsets[i, 0];
            int nc = c + offsets[i, 1];

            if (!IsInsideBoard(nr, nc)) continue;

            var target = GetPieceAt(nr, nc);
            if (target == null || target.Color != knight.Color)
                moves.Add(new Move(c, r, nc, nr));
        }

        return moves;
    }
    
    private List<Move> GenerateBishopMoves(int r, int c, Piece b)
    {
        List<Move> moves = new List<Move>();
        int[][] dirs = {
            new[] {1, 1}, new[] {1, -1},
            new[] {-1, 1}, new[] {-1, -1}
        };

        foreach (var d in dirs)
            moves.AddRange(GenerateSlidingMoves(r, c, b, d[0], d[1]));

        return moves;
    }

    private List<Move> GenerateRookMoves(int r, int c, Piece rook)
    {
        List<Move> moves = new List<Move>();
        int[][] dirs = {
            new[] {1, 0}, new[] {-1, 0},
            new[] {0, 1}, new[] {0, -1}
        };

        foreach (var d in dirs)
            moves.AddRange(GenerateSlidingMoves(r, c, rook, d[0], d[1]));

        return moves;
    }

    private List<Move> GenerateQueenMoves(int r, int c, Piece q)
    {
        List<Move> moves = new List<Move>();

        moves.AddRange(GenerateRookMoves(r, c, q));
        moves.AddRange(GenerateBishopMoves(r, c, q));

        return moves;
    }

    private List<Move> GenerateKingMoves(int r, int c, Piece king)
    {
        List<Move> moves = new List<Move>();
        int[,] offsets = {
            {1,0}, {-1,0}, {0,1}, {0,-1},
            {1,1}, {1,-1}, {-1,1}, {-1,-1}
        };

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int nr = r + offsets[i, 0];
            int nc = c + offsets[i, 1];

            if (!IsInsideBoard(nr, nc)) continue;

            var target = GetPieceAt(nr, nc);
            if (target == null || target.Color != king.Color)
                moves.Add(new Move(c, r, nc, nr));
        }

        return moves;
    }

    private List<Move> GenerateSlidingMoves(int r, int c, Piece piece, int dr, int dc)
    {
        List<Move> moves = new List<Move>();

        int nr = r + dr;
        int nc = c + dc;

        while (IsInsideBoard(nr, nc))
        {
            var target = GetPieceAt(nr, nc);

            if (target == null)
            {
                moves.Add(new Move(c, r, nc, nr));
            }
            else
            {
                if (target.Color != piece.Color)
                    moves.Add(new Move(c, r, nc, nr));
                break;
            }

            nr += dr;
            nc += dc;
        }

        return moves;
    }


}