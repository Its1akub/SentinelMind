using SentinelMind.AI;
using SentinelMind.Models;

namespace SentinelMind;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("====================Help====================\nUsage: SentinelMind.exe \"<FEN>\" [--depth 5] \nOr: SentinelMind.exe --help" );
            return;
        }

        if (args[0] == "--help" || args[0] == "-h" || args[0] == "-?")
        {
            Console.WriteLine("Usage: myengine.exe \"<FEN>\" [options]\nOptions:"+
            "\n  -i, --intput <p>                               'p' = printing the chessboard"+
            "\n  -d, --depth <N>                                depth search (default 5)"+
            "\n  -t, --time <N>                                 time limit in seconds"+
            "\n  -v, --verbose                                  Detail content"+
            "\n  -b, -s, --benvh, --benchmarks, --stats         Detail content"+
            "\n  --nodes                                        number of search nodes"+
            "\n  --eval                                         the evaluated score for the move"+
            "\n  --moves                                        legal moves"+
            "\n  -o, --output <p|f>                             'p' = printing the chessboard, 'f' = printing the FEN"+
            "\n \nSentinelMind.exe  (-h, --help, -?)             print this help"+
            "\n \nOr look on this repository: https://github.com/Its1akub/SentinelMind");
            return;
        }
        
        string fen = args[0];
        int depth = 5; 
        bool verbose = false;
        double timeLimitMs = 0;
        bool benchmark = false;
        bool showNodes = false;
        bool showEval = false;
        bool showMoves = false;
        string outputMode = "n"; 
        char inputMode = 'n';

        try
        {
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--depth":
                    case "-d":
                        string data = args[++i];
                        try
                        {
                            depth = int.Parse(data);
                        }
                        catch (Exception e)
                        {
                            throw new InvalidCastException("depth must be an integer");
                        }
                        break;
                    case "-i":
                        char input = args[++i].ToCharArray()[0];
                        if (input == 'p') inputMode = input; 
                        else throw new Exception("Invalid input");
                        break;

                    case "--verbose":
                    case "-v":
                        verbose = true;
                        break;
                    case "-t":
                    case "--time":
                        timeLimitMs = double.Parse(args[++i]) * 1000.0;
                        break;
                    case "--bench":
                    case "--benchmark":    
                    case "--stats":
                    case "-b":
                    case "-s":    
                        benchmark = true;
                        break;
                    case "--nodes":
                        showNodes = true;
                        break;
                    case "--eval":
                        showEval = true;
                        break;
                    case "--moves":
                        showMoves = true;
                        break;
                    case "-o":
                    case "--output":
                        string output = args[++i];
                        if (output == "p" || output == "f" || output == "pf" || output == "fp") outputMode = output; 
                        else throw new Exception("Invalid input");
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }
       
        
        

        try
        {
            Board board = new Board();
            board.InsertFEN(fen);
            if (inputMode == 'p') Console.WriteLine("\n==========Before==========\n"+board+"\n===========================\n");
            var engine = new Engine();
            if (verbose) engine.Verbose = true;
            if (timeLimitMs > 0) engine.TimeLimit = timeLimitMs;
            
                
            if (showMoves)
            {
                var moves = board.GenerateLegalMoves(PieceColor.White);
                Console.WriteLine("\n==========Legal moves:==========\n");
                foreach (var m in moves)
                    Console.WriteLine(m);
            }
            
            if (verbose) Console.WriteLine($"\n==========Data==========\nEvalution for {depth} depth…");
            
            var bestMove = engine.FindBestMove(board, PieceColor.White, depth: depth);

            board.MakeMove(bestMove);
            Console.WriteLine($"\n==========Best move==========\n{bestMove} \n==============================\n");
            
            if (benchmark)
            {
                
                Console.WriteLine($"\n==========Benchmarks==========\nNodes searched: {engine.Nodes}");
                Console.WriteLine($"Eval: {engine.LastEval}");
                Console.WriteLine($"Time: {engine.Timer.ElapsedMilliseconds} ms");
                Console.WriteLine($"NPS (Nodes per Second): {engine.Nodes * 1000 / Math.Max(engine.Timer.ElapsedMilliseconds, 1)}\n===============================\n");
            }
            if (showEval)  Console.WriteLine($"\n==========Stats==========\nEval: {engine.LastEval}\n==========================\n");
            if (showNodes) Console.WriteLine($"\n==========Stats==========\nNodes searched: {engine.Nodes}\n==========================\n");
            if (outputMode.Contains('p')) Console.WriteLine("\n==========After==========\n"+board+"\n==========================\n");
            if (outputMode.Contains('f')) Console.WriteLine("\n====================\nFen: "+board.GetFEN()+"\n=====================\n");
                
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

    }
}