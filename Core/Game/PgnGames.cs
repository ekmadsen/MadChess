// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace ErikTheCoder.MadChess.Core.Game;


public sealed class PgnGames : List<PgnGame>
{
    public void Load(Board board, string filename, Delegates.WriteMessageLine writeMessageLine)
    {
        using (var pgnReader = File.OpenText(filename))
        {
            var gameNumber = 1;
            PgnGame pgnGame;
            do
            {
                pgnGame = GetNextGame(board, pgnReader, gameNumber);
                if ((pgnGame != null) && (pgnGame.Result != GameResult.Unknown)) Add(pgnGame);
                gameNumber++;
                if ((gameNumber % 1000) == 0) writeMessageLine($"Loaded {gameNumber:n0} games.");
            } while (pgnGame != null);
        }
        GC.Collect();
    }


    private static PgnGame GetNextGame(Board board, StreamReader pgnReader, int gameNumber)
    {
        const string eventTag = "[Event ";
        const string resultTag = "[Result ";
        const string fenTag = "[FEN ";
        while (!pgnReader.EndOfStream)
        {
            var line = pgnReader.ReadLine();
            if (line == null) continue;
            line = line.Trim();
            if (line.StartsWith(eventTag))
            {
                // Found start of game.
                var result = GameResult.Unknown;
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(line);
                while (!pgnReader.EndOfStream)
                {
                    line = pgnReader.ReadLine();
                    if (line == null) continue;
                    line = line.Trim();
                    if (line.StartsWith(fenTag)) break; // Skip games that start from non-standard positions.
                    stringBuilder.AppendLine(line);
                    if (line.StartsWith(resultTag))
                    {
                        // Determine game result.
                        var startPosition = line.IndexOf("\"", resultTag.Length, StringComparison.OrdinalIgnoreCase) + 1;
                        var endPosition = line.IndexOf("\"", startPosition, StringComparison.OrdinalIgnoreCase) - 1;
                        var length = endPosition - startPosition + 1;
                        var gameResultText = line.Substring(startPosition, length);
                        result = gameResultText switch
                        {
                            "1-0" => GameResult.WhiteWon,
                            "1/2-1/2" => GameResult.Draw,
                            "0-1" => GameResult.BlackWon,
                            _ => GameResult.Unknown
                        };
                    }
                    else if (line.EndsWith("1-0") || line.EndsWith("1/2-1/2") || line.EndsWith("0-1") || line.EndsWith("*"))
                    {
                        // Found end of game.
                        return new PgnGame(board, gameNumber, result, stringBuilder.ToString());
                    }
                }
            }
        }
        return null;
    }
}