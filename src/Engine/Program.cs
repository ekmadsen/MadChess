// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Runtime;
using ErikTheCoder.MadChess.Core;


namespace ErikTheCoder.MadChess.Engine;

public static class Program
{
    public static void Main()
    {

        // Improve garbage collector performance at the cost of memory usage.
        // Engine should not allocate much memory when searching a position anyhow because it references pre-allocated objects.
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        using (var inputStream = Console.OpenStandardInput())
        using (var outputStream = Console.OpenStandardOutput())
        using (var messenger = new Messenger(inputStream, outputStream))
        using (var uciStream = new UciStream(messenger))
        {
            try
            {
                uciStream.Run();
            }
            catch (Exception exception)
            {
                uciStream.HandleException(exception);
            }
        }
    }
}