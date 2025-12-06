using System.Diagnostics;
using SentinelMind.Models;

namespace SentinelMind.AI;

public class Engine
{
    public Stopwatch Timer = new Stopwatch();
    private long _nodes = 0;
    
    public float RunningTime { get; set; } = 0f;
    public bool Verbose { get; set; } = false;
    public int LastEval { get; private set; } = 0;
    public double TimeLimit { get; set; } = 0;
    public long Nodes
    {
        get => _nodes;
        private set => _nodes = value;
    }
    
    public Move FindBestMove(Board board, PieceColor color, int depth)
    {
        Nodes = 0;
        LastEval = 0;
        Timer = Stopwatch.StartNew();
        
        var moves = board.GenerateLegalMoves(color);
        if (!moves.Any())
            return default;

        var bestMove = default(Move);
        int bestScore = color == PieceColor.White ? int.MinValue : int.MaxValue;

        var locker = new object();
        
        
        
        Parallel.ForEach(moves, move =>
        {
            if (TimeLimit > 0 && Timer.ElapsedMilliseconds >= TimeLimit)
            {
                Timer.Stop();
                return;
            }
            
            var copy = board.Copy();
            try
            {
                copy.MakeMove(move);

                int score = Minimax(copy, depth - 1, int.MinValue, int.MaxValue, color == PieceColor.Black);
                if (TimeLimit > 0 && Timer.ElapsedMilliseconds >= TimeLimit)
                {
                    Timer.Stop();
                    return;
                }
                
                
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
        if (Timer.IsRunning)
        {
            RunningTime = Timer.ElapsedMilliseconds/1000f;
            Timer.Stop();
        }
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
        Interlocked.Increment(ref _nodes);

        if (depth == 0)
        {
            int eval = Evaluate(board);
            if (Verbose)
                Console.WriteLine($"{new string('_', depth*2)}Eval at depth 0: {eval}");
            LastEval = eval;      
            return eval;
        }

        var moves = board.GenerateLegalMoves(
            maximizingPlayer ? PieceColor.White : PieceColor.Black
        );

        if (!moves.Any())
        {
            int eval = Evaluate(board);
            if (Verbose)
                Console.WriteLine($"{new string('_', depth*2)}No moves, eval: {eval}");
            LastEval = eval;
            return eval;
        }

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;

            foreach (var move in moves)
            {
                var copy = board.Copy();
                try
                {
                    copy.MakeMove(move);

                    if (Verbose)
                        Console.WriteLine($"{new string('_', depth*2)}Max: Trying move {move}");
                    
                    int eval = Minimax(copy, depth - 1, alpha, beta, false);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);

                    if (alpha >= beta)
                        if (Verbose)
                            Console.WriteLine($"{new string('_', depth*2)}Pruning remaining moves at depth {depth}");
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

                    if (Verbose)
                        Console.WriteLine($"{new string('_', depth*2)}Min: Trying move {move}");
                    
                    int eval = Minimax(copy, depth - 1, alpha, beta, true);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);

                    if (alpha >= beta)
                        if (Verbose)
                            Console.WriteLine($"{new string('_', depth*2)}Pruning remaining moves at depth {depth}");
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