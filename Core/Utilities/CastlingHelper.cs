// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Text;
using ErikTheCoder.MadChess.Core.Game;


namespace ErikTheCoder.MadChess.Core.Utilities
{
    public static class CastlingHelper
    {
        public static string ToString(bool[][] castling)
        {
            var stringBuilder = new StringBuilder();
            if (castling[(int)Color.White][(int)BoardSide.King]) stringBuilder.Append('K');
            if (castling[(int)Color.White][(int)BoardSide.Queen]) stringBuilder.Append('Q');
            if (castling[(int)Color.Black][(int)BoardSide.King]) stringBuilder.Append('k');
            if (castling[(int)Color.Black][(int)BoardSide.Queen]) stringBuilder.Append('q');
            return stringBuilder.Length == 0
                ? "-"
                : stringBuilder.ToString();
        }
    }
}
