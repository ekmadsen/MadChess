// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;


namespace ErikTheCoder.MadChess.Engine
{
    public struct CachedPosition
    {
        public ulong Key;
        public ulong Data;


        public CachedPosition(ulong Key, ulong Data)
        {
            this.Key = Key;
            this.Data = Data;
        }


        public static bool operator ==(CachedPosition Position1, CachedPosition Position2) => Position1.Key == Position2.Key;


        public static bool operator !=(CachedPosition Position1, CachedPosition Position2) => Position1.Key != Position2.Key;


        // ReSharper disable once MemberCanBePrivate.Global
        public bool Equals(CachedPosition OtherPosition) => Key == OtherPosition.Key;


        public override bool Equals(object OtherPosition) => OtherPosition is CachedPosition otherPosition && Equals(otherPosition);


        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => HashCode.Combine(Key, Data);
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }
}