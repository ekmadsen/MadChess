// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Collections.Generic;


namespace ErikTheCoder.MadChess.Core.Utilities;


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
                var lastIndex = index + 1;
                token = command[startIndex..lastIndex];
                tokens.Add(token.TrimEnd(tokenizer));
                break;
            }

            if (character == separator)
            {
                if (inToken) continue;

                // Add token.
                token = command[startIndex..index];
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