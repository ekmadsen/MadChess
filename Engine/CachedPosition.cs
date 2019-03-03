// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine
{
    // Use a struct instead of a class to ensure allocating an array of CachedPositions uses a contiguous block of memory.
    // This improves data locality, decreasing chance that accessing a CachedPosition causes a CPU cache miss.
    // See https://stackoverflow.com/questions/16699247/what-is-a-cache-friendly-code.
    public struct CachedPosition
    {
        public ulong Key;
        public ulong Data;


        public CachedPosition(ulong Key, ulong Data)
        {
            this.Key = Key;
            this.Data = Data;
        }
    }
}