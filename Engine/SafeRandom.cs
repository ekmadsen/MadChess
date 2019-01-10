// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;


namespace ErikTheCoder.MadChess.Engine
{
    public static class SafeRandom
    {
        private static readonly byte[] _buffer;
        private static readonly Random _random;
        private static readonly object _lock;


        static SafeRandom()
        {
            _buffer = new byte[sizeof(ulong)];
            _random = new Random();
            _lock = new object();
        }



        public static int NextInt(int InclusiveMin, int ExclusiveMax)
        {
            lock (_lock)
            {
                return _random.Next(InclusiveMin, ExclusiveMax);
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
}
