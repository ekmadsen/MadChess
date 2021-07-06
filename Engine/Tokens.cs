// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Collections.Generic;


namespace ErikTheCoder.MadChess.Engine
{
    public static class Tokens
    {
        public static List<string> Parse(string command, char separator, char tokenizer)
        {
            var tokens = new List<string>();
            var startIndex = 0;
            var inToken = false;
            for (var index = 0; index < command.Length; index++)
            {
                string token;
                var character = command[index];
                if (index == command.Length - 1)
                {
                    // Add last token.
                    token = command.Substring(startIndex, index - startIndex + 1);
                    tokens.Add(token.TrimEnd(tokenizer));
                    break;
                }
                if (character == separator)
                {
                    if (inToken) continue;
                    // Add token.
                    token = command.Substring(startIndex, index - startIndex);
                    tokens.Add(token.TrimEnd(tokenizer));
                    startIndex = index + 1;
                }
                else if (character == tokenizer)
                {
                    if (inToken) inToken = false;
                    else
                    {
                        startIndex = index + 1;
                        inToken = true;
                    }
                }
            }
            return tokens;
        }
    }
}