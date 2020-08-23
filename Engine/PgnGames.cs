// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class PgnGames : List<PgnGame>
    {
        public void Load(Board Board, string Filename)
        {
            using (StreamReader pgnReader = File.OpenText(Filename))
            {
                int gameNumber = 1;
                PgnGame pgnGame;
                do
                {
                    pgnGame = GetNextGame(Board, pgnReader, gameNumber);
                    if ((pgnGame != null) && (pgnGame.Result != GameResult.Unknown)) Add(pgnGame);
                    gameNumber++;
                } while (pgnGame != null);
            }
            GC.Collect();
        }


        private static PgnGame GetNextGame(Board Board, StreamReader PgnReader, int GameNumber)
        {
            const string eventTag = "[Event ";
            const string resultTag = "[Result ";
            const string fenTag = "[FEN ";
            while (!PgnReader.EndOfStream)
            {
                string line = PgnReader.ReadLine();
                if (line == null) continue;
                line = line.Trim();
                if (line.StartsWith(eventTag))
                {
                    // Found start of game.
                    GameResult result = GameResult.Unknown;
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(line);
                    while (!PgnReader.EndOfStream)
                    {
                        line = PgnReader.ReadLine();
                        if (line == null) continue;
                        line = line.Trim();
                        if (line.StartsWith(fenTag)) break; // Skip games that start from non-standard positions.
                        stringBuilder.AppendLine(line);
                        if (line.StartsWith(resultTag))
                        {
                            // Determine game result.
                            int startPosition = line.IndexOf("\"", resultTag.Length, StringComparison.OrdinalIgnoreCase) + 1;
                            int endPosition = line.IndexOf("\"", startPosition, StringComparison.OrdinalIgnoreCase) - 1;
                            int length = endPosition - startPosition + 1;
                            string gameResultText = line.Substring(startPosition, length);
                            result = gameResultText switch
                            {
                                "1-0" => GameResult.WhiteWon,
                                "1/2-1/2" => GameResult.Draw,
                                "0-1" => GameResult.BlackWon,
                                _ => GameResult.Unknown
                            };
                        }
                        else if (line.EndsWith("1-0") || line.EndsWith("1/2-1/2") || line.EndsWith("0-1") || line.EndsWith("*")) return new PgnGame(Board, GameNumber, result, stringBuilder.ToString()); // Found end of game.
                    }
                }
            }
            return null;
        }
    }
}
