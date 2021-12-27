// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Game;


namespace ErikTheCoder.MadChess.Engine.Tuning;


public sealed class QuietPosition
{
    public readonly string Fen;
    public readonly GameResult GameResult;


    public QuietPosition(string fen, GameResult gameResult)
    {
        Fen = fen;
        GameResult = gameResult;
    }
}
