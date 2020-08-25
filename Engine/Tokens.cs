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
        public static List<string> Parse(string Command, char Separator, char Tokenizer)
        {
            List<string> tokens = new List<string>();
            int startIndex = 0;
            bool inToken = false;
            for (int index = 0; index < Command.Length; index++)
            {
                string token;
                char character = Command[index];
                if (index == Command.Length - 1)
                {
                    // Add last token.
                    token = Command.Substring(startIndex, index - startIndex + 1);
                    tokens.Add(token.TrimEnd(Tokenizer));
                    break;
                }
                if (character == Separator)
                {
                    if (inToken) continue;
                    // Add token.
                    token = Command.Substring(startIndex, index - startIndex);
                    tokens.Add(token.TrimEnd(Tokenizer));
                    startIndex = index + 1;
                }
                else if (character == Tokenizer)
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