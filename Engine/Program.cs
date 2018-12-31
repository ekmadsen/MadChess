﻿// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Runtime;


namespace MadChess.Engine
{
    public static class Program
    {
        public static void Main()
        {
            // Improve garbage collector performance at the cost of memory usage.
            // Engine should not allocate much memory when searching a position anyhow, since it references pre-allocated objects.
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            using (UciStream uciStream = new UciStream())
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
}