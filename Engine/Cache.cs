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
        public const int CapacityPerMegabyte = 1024 * 1024 / (2 * sizeof(ulong)); // CachedPosition struct contains two ulongs.
        public int Positions;
        public byte Searches;
        private const int _buckets = 4;
        private readonly Delegates.ValidateMove _validateMove;
        private CachedPosition[][] _positions;
        private CachedPosition _nullPosition;



        public long Capacity
        {
            get => _positions.Length * _buckets;
            set
            {
                _positions = new CachedPosition[value / _buckets][];
                for (int index = 0; index < _positions.Length; index++) _positions[index] = new CachedPosition[_buckets];
                Reset();
                GC.Collect();
            }
        }


        public Cache(long Capacity, Delegates.ValidateMove ValidateMove)
        {
            this.Capacity = Capacity;
            _validateMove = ValidateMove;
            _nullPosition = new CachedPosition(0, 0);
            CachedPositionData.Clear(ref _nullPosition.Data);
        }


        public ref CachedPosition GetPosition(ulong Key)
        {
            int index = GetIndex(Key);
            for (int bucket = 0; bucket < _buckets; bucket++)
            {
                ref CachedPosition cachedPosition = ref _positions[index][bucket];
                if (cachedPosition.Key == Key)
                {
                    // Position is cached.
                    CachedPositionData.SetLastAccessed(ref cachedPosition.Data, Searches);
                    return ref cachedPosition;
                }
            }
            // Position is not cached.
            return ref _nullPosition;
        }


        public ref CachedPosition GetPositionToOverwrite(ulong Key)
        {
            int index = GetIndex(Key);
            // Find oldest bucket.
            byte earliestAccess = byte.MaxValue;
            int oldestBucket = 0;
            for (int bucket = 0; bucket < _buckets; bucket++)
            {
                ref CachedPosition cachedPosition = ref _positions[index][bucket];
                byte lastAccessed = CachedPositionData.LastAccessed(cachedPosition.Data);
                if (lastAccessed < earliestAccess)
                {
                    earliestAccess = lastAccessed;
                    oldestBucket = bucket;
                }
            }
            ref CachedPosition cachedPositionToOverwrite = ref _positions[index][oldestBucket];
            if (cachedPositionToOverwrite.Key == 0) Positions++; // Oldest bucket has not been used.
            // Set key, clear data, and set search counter.
            cachedPositionToOverwrite.Key = Key;
            CachedPositionData.Clear(ref cachedPositionToOverwrite.Data);
            CachedPositionData.SetLastAccessed(ref cachedPositionToOverwrite.Data, Searches);
            return ref cachedPositionToOverwrite;
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
                for (int bucket = 0; bucket < _buckets; bucket++) { position[bucket] = _nullPosition; }
            }
            Positions = 0;
            Searches = 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndex(ulong Key) => (int)((uint)Key.GetHashCode() % (uint)_positions.Length);
    }
}
