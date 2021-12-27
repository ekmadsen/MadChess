// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;


namespace ErikTheCoder.MadChess.Tests;


public abstract class TestBase
{
    protected static void WriteMessageLine(string message) => Console.WriteLine(message);
    protected static void WriteMessageLine() => Console.WriteLine();
}