using SentinelMind.Models;

namespace SentinelMind;

class Program
{
    static void Main(string[] args)
    {
        Board board = new Board();
        Console.WriteLine(board);
        board.InsertFEN();
        Console.WriteLine(board);

    }
}