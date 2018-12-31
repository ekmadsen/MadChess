// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace MadChess.Engine
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
                            int startPosition = line.IndexOf("\"", resultTag.Length, StringComparison.CurrentCultureIgnoreCase) + 1;
                            int endPosition = line.IndexOf("\"", startPosition, StringComparison.CurrentCultureIgnoreCase) - 1;
                            int length = endPosition - startPosition + 1;
                            string gameResultText = line.Substring(startPosition, length);
                            switch (gameResultText)
                            {
                                case "1-0":
                                    result = GameResult.WhiteWon;
                                    break;
                                case "1/2-1/2":
                                    result = GameResult.Draw;
                                    break;
                                case "0-1":
                                    result = GameResult.BlackWon;
                                    break;
                                default:
                                    result = GameResult.Unknown;
                                    break;
                            }
                        }
                        else if (line.EndsWith("1-0") || line.EndsWith("1/2-1/2") || line.EndsWith("0-1") || line.EndsWith("*")) return new PgnGame(Board, GameNumber, result, stringBuilder.ToString()); // Found end of game.
                    }
                }
            }
            return null;
        }
    }
}
