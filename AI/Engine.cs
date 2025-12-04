using SentinelMind.Models;

namespace SentinelMind.AI;

public class Engine
{
    
    public Move FindBestMove(Board board, PieceColor color, int depth)
    {
        var moves = board.GenerateLegalMoves(color);
        if (!moves.Any())
            return default;

        var bestMove = default(Move);
        int bestScore = color == PieceColor.White ? int.MinValue : int.MaxValue;

        var locker = new object();
        
        Parallel.ForEach(moves, move =>
        {
            var copy = board.Copy();
            try
            {
                copy.MakeMove(move);

                int score = Minimax(copy, depth - 1, int.MinValue, int.MaxValue, color == PieceColor.Black);
                lock (locker)
                {
                    if (color == PieceColor.White)
                    {
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMove = move;
                        }
                    }
                    else
                    {
                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestMove = move;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        });
        return bestMove;
    }

    public int Evaluate(Board board)
    {
        int score = 0;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var piece = board.Squares[row, col].Piece;
                if (piece == null) continue;

                int value = piece.Type switch
                {
                    PieceType.Pawn => 1,
                    PieceType.Knight => 3,
                    PieceType.Bishop => 3,
                    PieceType.Rook => 5,
                    PieceType.Queen => 9,
                    PieceType.King => 100000000,
                    _ => 0
                };

                if (piece.Color == PieceColor.White)
                    score += value;
                else
                    score -= value;
            }
        }

        return score;
    }
    
    public int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (depth == 0)
            return Evaluate(board);

        var moves = board.GenerateLegalMoves(
            maximizingPlayer ? PieceColor.White : PieceColor.Black
        );

        if (!moves.Any())
            return Evaluate(board);

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;

            foreach (var move in moves)
            {
                var copy = board.Copy();
                try
                {
                    copy.MakeMove(move);

                    int eval = Minimax(copy, depth - 1, alpha, beta, false);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);

                    if (alpha >= beta)
                        break;
                }
                catch (Exception)
                {
                    // ignored
                }
                
            }

            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;

            foreach (var move in moves)
            {
                var copy = board.Copy();
                try
                {
                    copy.MakeMove(move);

                    int eval = Minimax(copy, depth - 1, alpha, beta, true);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);

                    if (alpha >= beta)
                        break;
                }
                catch (Exception)
                {
                    // ignored
                }
                
            }

            return minEval;
        }
    }
    
}