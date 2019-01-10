// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class Cache
    {
        public const int CapacityPerMegabyte = 1024 * 1024 / (3 * sizeof(ulong)); // CachedPosition struct contains two ulongs.
        private const int _buckets = 4;
        private readonly Delegates.ValidateMove _validateMove;
        public int Positions;
        public byte Searches;
        private CachedPosition[][] _positions;


        public long Capacity
        {
            get => _positions.Length * _buckets;
            set
            {
                _positions = new CachedPosition[value / _buckets][];
                GC.Collect();
                for (int index = 0; index < _positions.Length; index++) _positions[index] = new CachedPosition[_buckets];
                Reset();
            }
        }


        public Cache(long Capacity, Delegates.ValidateMove ValidateMove)
        {
            this.Capacity = Capacity;
            _validateMove = ValidateMove;
        }


        public ulong GetPosition(ulong Key)
        {
            int index = GetIndex(Key);
            for (int bucket = 0; bucket < _buckets; bucket++)
            {
                CachedPosition cachedPosition = _positions[index][bucket];
                if (cachedPosition.Key == Key)
                {
                    // Position is cached.
                    ulong data = cachedPosition.Data;
                    CachedPosition.SetLastAccessed(ref data, Searches);
                    cachedPosition = new CachedPosition(Key, data);
                    _positions[index][bucket] = cachedPosition;
                    return data;
                }
            }
            // Position is not cached.
            return CachedPosition.Null;
        }


        public void SetPosition(ulong Key, ulong Position)
        {
            int index = GetIndex(Key);
            byte earliestAccess = byte.MaxValue;
            int oldestBucket = 0;
            ulong oldestPosition = 0;
            for (int bucket = 0; bucket < _buckets; bucket++)
            {
                CachedPosition cachedPosition = _positions[index][bucket];
                byte lastAccessed = CachedPosition.LastAccessed(cachedPosition.Data);
                if (lastAccessed < earliestAccess)
                {
                    earliestAccess = lastAccessed;
                    oldestBucket = bucket;
                    oldestPosition = cachedPosition.Data;
                }
                if (cachedPosition.Key == Key)
                {
                    // Position is cached.  Overwrite position.
                    CachedPosition.SetLastAccessed(ref Position, Searches);
                    _positions[index][bucket] = new CachedPosition(Key, Position);
                    return;
                }
            }
            // Position is not cached.
            if (oldestPosition == CachedPosition.Null) Positions++; // Oldest bucket has not been used.
            // Overwrite oldest bucket.
            CachedPosition.SetLastAccessed(ref Position, Searches);
            _positions[index][oldestBucket] = new CachedPosition(Key, Position);
        }


        public ulong GetBestMove(ulong CachedPosition)
        {
            if (CachedPosition == Engine.CachedPosition.Null) return Move.Null;
            int fromSquare = Engine.CachedPosition.BestMoveFrom(CachedPosition);
            if (fromSquare == Square.Illegal) return Move.Null; // Cached position does not specify a best move.
            ulong bestMove = Move.Null;
            Move.SetFrom(ref bestMove, fromSquare);
            Move.SetTo(ref bestMove, Engine.CachedPosition.BestMoveTo(CachedPosition));
            Move.SetPromotedPiece(ref bestMove, Engine.CachedPosition.BestMovePromotedPiece(CachedPosition));
            Move.SetIsBest(ref bestMove, true);
            bool validMove = _validateMove(ref bestMove);
            return validMove ? bestMove : Move.Null;
        }


        public void Reset()
        {
            for (int index = 0; index < _positions.Length; index++)
            {
                CachedPosition[] position = _positions[index];
                for (int bucket = 0; bucket < _buckets; bucket++) position[bucket] = new CachedPosition(0, CachedPosition.Null);
            }
            Positions = 0;
            Searches = 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndex(ulong Key) => (int)(Key % (ulong)_positions.Length);
    }
}