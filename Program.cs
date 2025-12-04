using SentinelMind.AI;
using SentinelMind.Models;

namespace SentinelMind;

class Program
{
    static void Main(string[] args)
    {
        Board board = new Board();
        board.InsertFEN("3B1k2/b1K1p3/8/1b4p1/pr2P1R1/2P3P1/2P5/4N1R1 w - - 0 1");
        Console.WriteLine(board);
        var engine = new Engine();

        try
        {
            var bestMove = engine.FindBestMove(board, PieceColor.White, depth: 5);

            if (bestMove != null)
            {
                board.MakeMove(bestMove);
                Console.WriteLine($"AI hraje: {bestMove}");
            }
            else
            {
                Console.WriteLine("Žádné dostupné tahy");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Chyba při provádění tahu: " + ex.Message);
        }

    }
}