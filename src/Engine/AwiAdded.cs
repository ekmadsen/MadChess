using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErikTheCoder.MadChess.Core;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Score;

namespace ErikTheCoder.MadChess.Engine;

[Flags]
public enum ToStringFlags { None, PassedPawns = 1, Mobility = 2, KingSafety = 4, Location = 8,
    PawnStructure = 16, Threats = 32, Minors = 64, Majors = 128, Material = 256, SimpleEndgame = 512, 
    FiftyMove = 1024 };

public sealed partial class UciStream
{
    private void TestStaticScore(List<string> tokens)
    {
        _commandStopwatch.Restart();

        var file = tokens.Count > 1 ? tokens[1].Trim() : "testpos.txt";
        _messenger.WriteLine("Number                                                                     Position  Depth     Expected        Moves  Correct    Pct");
        _messenger.WriteLine("======  ===========================================================================  =====  ===========  ===========  =======  =====");

        _board.Nodes = 0;
        _search.NodesInfoUpdate = NodesInfoInterval;
        var positions = 0;
        //var correctPositions = 0;

        // Verify move counts of test positions.
        using (var reader = File.OpenText(file))
        {
            while (!reader.EndOfStream)
            {
                // Load position, horizon, and correct move count.
                var line = reader.ReadLine();
                if (line == null) continue;

                positions++;

                var parsedTokens = Tokens.Parse(line, '|', '"');
                var fen = parsedTokens[0];
                if (fen.StartsWith("#")) continue;
                //var horizon = int.Parse(parsedTokens[1]);
                //var expectedMoves = long.Parse(parsedTokens[2]);

                _messenger.WriteLine("xxxxx xxxxx xxxxx xxxxxxxxxxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxxxxxxxxxxxxxx xxxxxxxxxxxxxxxxxxxxxxxx xxxxxxxxxxx xxxxx ");
                _messenger.WriteLine($"================ Position: {parsedTokens[1]}");
                // Setup position.  Preserve move count.
                _board.SetPosition(fen, true);
                _messenger.WriteLine(_evaluation.ToString(_board.CurrentPosition, 
                    ToStringFlags.Mobility | ToStringFlags.Location | ToStringFlags.KingSafety ));

                // Count nodes.  Do not display node count.
                //_search.NodesInfoUpdate = long.MaxValue;
                ////var moves = CountMoves(0, horizon);

                //var correct = moves == expectedMoves;
                //if (correct) correctPositions++;
                //var correctFraction = (100d * correctPositions) / positions;

                // _messenger.WriteLine($"{positions,6}  {fen,75}  {horizon,5:0}  {expectedMoves,11:n0}  {moves,11:n0}  {correct,7}  {correctFraction,5:0.0}");

                //GoSync("go movetime 2000".Split().ToList());

            }
        }

        _commandStopwatch.Stop();

        _messenger.WriteLine();
        // _messenger.WriteLine($"Test completed in {_commandStopwatch.Elapsed.TotalSeconds:0} seconds.");

        // Display node count.
        //var nodesPerSecond = _board.Nodes / _commandStopwatch.Elapsed.TotalSeconds;
        //_messenger.WriteLine($"Counted {_board.Nodes:n0} nodes ({nodesPerSecond:n0} nodes per second).");
    }

}

static class Ext
{
    public static bool HasFlag(object flags, object flag)
    {
        int iflags = (int)flags;
        int iflag = (int)flag;
        return (iflags & iflag) == iflag;
    }

    public static void LogScoredMoves(Board board, Messenger messenger, ScoredMove[] bestMoves, int scoreError)
    {
        for (int i = 0; i < FastMath.Min(board.CurrentPosition.MoveIndex, 5); ++i)
            LogScoredMove(messenger, bestMoves, scoreError, i);
    }

    private static void LogScoredMove(Messenger messenger, ScoredMove[] bestMoves, int scoreError, int num)
    {
        ScoredMove m = bestMoves[num];
        var delta = bestMoves[0].Score - m.Score;
        var inOut = num == 0 ? "best" : delta < scoreError ? "in" : "out";
        messenger.WriteLine($"info num={num} {inOut} {Move.ToLongAlgebraic(m.Move)} " +
            $"scoreCp={m.Score} delta={delta} scError={scoreError}");
    }


}


