// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Threading;


namespace ErikTheCoder.MadChess.Core.Utilities;


public static class SafeRandom
{
    private static readonly byte[] _buffer;
    private static readonly Random _random;
    private static readonly Lock _lock;


    static SafeRandom()
    {
        _buffer = new byte[sizeof(ulong)];
        _random = new Random();
        _lock = new Lock();
    }



    public static int NextInt(int inclusiveMin, int exclusiveMax)
    {
        lock (_lock)
        {
            return _random.Next(inclusiveMin, exclusiveMax);
        }
    }


    public static double NextDouble()
    {
        lock (_lock)
        {
            return _random.NextDouble();
        }
    }


    public static ulong NextULong()
    {
        lock (_lock)
        {
            _random.NextBytes(_buffer);
            return BitConverter.ToUInt64(_buffer, 0);
        }
    }
}