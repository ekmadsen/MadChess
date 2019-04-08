// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class Cache
    {
        // TODO: Determine why cache allocates 3 * sizeof(ulong) instead of the expected 2 * sizeof(ulong) considering the .NET runtime does not allocate an ObjectHeader or MethodTable for structs.
        // See https://adamsitnik.com/Value-Types-vs-Reference-Types/.
        public const int CapacityPerMegabyte = 1024 * 1024 / (3 * sizeof(ulong)); // CachedPosition struct contains two ulongs.
        public int Positions;
        public byte Searches;
        public CachedPosition NullPosition;
        private const int _buckets = 4;
        private readonly Delegates.ValidateMove _validateMove;
        private CachedPosition[][] _positions;
        
        
        public long Capacity
        {
            get => _positions.Length * _buckets;
            set
            {
                _positions = null;
                GC.Collect();
                _positions = new CachedPosition[value / _buckets][];
                for (int index = 0; index < _positions.Length; index++) _positions[index] = new CachedPosition[_buckets];
                Reset();
            }
        }


        public Cache(long Capacity, Delegates.ValidateMove ValidateMove)
        {
            this.Capacity = Capacity;
            _validateMove = ValidateMove;
            NullPosition = new CachedPosition(0, 0);
            CachedPositionData.Clear(ref NullPosition.Data);
        }


        public CachedPosition GetPosition(ulong Key)
        {
            int index = GetIndex(Key);
            for (int bucket = 0; bucket < _buckets; bucket++)
            {
                CachedPosition cachedPosition = _positions[index][bucket];
                if (cachedPosition.Key == Key)
                {
                    // Position is cached.
                    CachedPositionData.SetLastAccessed(ref cachedPosition.Data, Searches);
                    _positions[index][bucket] = cachedPosition;
                    return cachedPosition;
                }
            }
            // Position is not cached.
            return NullPosition;
        }


        public void SetPosition(CachedPosition CachedPosition)
        {
            CachedPositionData.SetLastAccessed(ref CachedPosition.Data, Searches);
            int index = GetIndex(CachedPosition.Key);
            // Find oldest bucket.
            byte earliestAccess = byte.MaxValue;
            int oldestBucket = 0;
            for (int bucket = 0; bucket < _buckets; bucket++)
            {
                CachedPosition cachedPosition = _positions[index][bucket];
                if (cachedPosition.Key == CachedPosition.Key)
                {
                    // Position is cached.  Overwrite position.
                    _positions[index][bucket] = CachedPosition;
                    return;
                }
                byte lastAccessed = CachedPositionData.LastAccessed(cachedPosition.Data);
                if (lastAccessed < earliestAccess)
                {
                    earliestAccess = lastAccessed;
                    oldestBucket = bucket;
                }
            }
            if (_positions[index][oldestBucket].Key == 0) Positions++; // Oldest bucket has not been used.
            // Overwrite oldest bucket.
            _positions[index][oldestBucket] = CachedPosition;
        }


        public ulong GetBestMove(CachedPosition Position)
        {
            if (Position.Key == 0) return Move.Null;
            int fromSquare = CachedPositionData.BestMoveFrom(Position.Data);
            if (fromSquare == Square.Illegal) return Move.Null; // Cached position does not specify a best move.
            ulong bestMove = Move.Null;
            Move.SetFrom(ref bestMove, fromSquare);
            Move.SetTo(ref bestMove, CachedPositionData.BestMoveTo(Position.Data));
            Move.SetPromotedPiece(ref bestMove, CachedPositionData.BestMovePromotedPiece(Position.Data));
            Move.SetIsBest(ref bestMove, true);
            bool validMove = _validateMove(ref bestMove);
            return validMove ? bestMove : Move.Null;
        }


        public void Reset()
        {
            for (int index = 0; index < _positions.Length; index++)
            {
                CachedPosition[] position = _positions[index];
                for (int bucket = 0; bucket < _buckets; bucket++) { position[bucket] = NullPosition; }
            }
            Positions = 0;
            Searches = 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndex(ulong Key)
        {
            // Ensure even distribution of indices by using GetHashCode method rather than using raw Zobrist Key for modular division.
            int index = Key.GetHashCode() % _positions.Length; // Index may be negative.
            // Ensure index is positive using technique faster than Math.Abs().  See http://graphics.stanford.edu/~seander/bithacks.html#IntegerAbs.
            int mask = index >> 31;
            return (index ^ mask) - mask;
        }
    }
}
