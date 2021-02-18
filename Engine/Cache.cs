﻿// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
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
        public const int CapacityPerMegabyte = 1024 * 1024 / sizeof(ulong);
        public readonly ulong NullPosition;
        public int Positions;
        public byte Searches;
        private const int _buckets = 4;
        private readonly Delegates.ValidateMove _validateMove;
        private int _indices;
        private ulong[] _positions; // More memory efficient than a jagged array (that has an object header for each sub-array).
        
        
        public int Capacity
        {
            get => _positions.Length;
            set
            {
                _positions = null;
                GC.Collect();
                _positions = new ulong[value];
                _indices = value / _buckets;
                Reset();
            }
        }


        public Cache(int SizeMegabyte, Delegates.ValidateMove ValidateMove)
        {
            Capacity = SizeMegabyte * CapacityPerMegabyte;
            _validateMove = ValidateMove;
            NullPosition = 0;
            CachedPosition.SetPartialKey(ref NullPosition, 0);
            CachedPosition.SetToHorizon(ref NullPosition, 0);
            CachedPosition.SetBestMoveFrom(ref NullPosition, Square.Illegal); // An illegal square indicates no best move stored in cached position.
            CachedPosition.SetBestMoveTo(ref NullPosition, Square.Illegal);
            CachedPosition.SetBestMovePromotedPiece(ref NullPosition, Piece.None);
            CachedPosition.SetScore(ref NullPosition, StaticScore.NotCached);
            CachedPosition.SetScorePrecision(ref NullPosition, ScorePrecision.Unknown);
            CachedPosition.SetLastAccessed(ref NullPosition, 0);
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public ulong GetPosition(ulong Key)
        {
            var index = GetIndex(Key);
            var partialKey = CachedPosition.PartialKey(Key);
            for (var bucket = 0; bucket < _buckets; bucket++)
            {
                var bucketIndex = index + bucket;
                var cachedPosition = _positions[bucketIndex];
                if (CachedPosition.PartialKey(cachedPosition) == partialKey)
                {
                    // Position is cached.
                    CachedPosition.SetLastAccessed(ref cachedPosition, Searches);
                    _positions[bucketIndex] = cachedPosition;
                    return cachedPosition;
                }
            }
            // Position is not cached.
            return NullPosition;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void SetPosition(ulong Key, ulong CachedPosition)
        {
            var index = GetIndex(Key);
            var partialKey = Engine.CachedPosition.PartialKey(Key);
            Engine.CachedPosition.SetLastAccessed(ref CachedPosition, Searches);
            // Find oldest bucket.
            var earliestAccess = byte.MaxValue;
            var oldestBucketIndex = 0;
            for (var bucket = 0; bucket < _buckets; bucket++)
            {
                var bucketIndex = index + bucket;
                var cachedPosition = _positions[bucketIndex];
                if (Engine.CachedPosition.PartialKey(cachedPosition) == partialKey)
                {
                    // Position is cached.  Overwrite position.
                    _positions[bucketIndex] = CachedPosition;
                    return;
                }
                var lastAccessed = Engine.CachedPosition.LastAccessed(cachedPosition);
                if (lastAccessed < earliestAccess)
                {
                    earliestAccess = lastAccessed;
                    oldestBucketIndex = bucket;
                }
            }
            if (Engine.CachedPosition.PartialKey(_positions[oldestBucketIndex]) == 0) Positions++; // Oldest bucket has not been used.
            // Overwrite oldest bucket.
            _positions[oldestBucketIndex] = CachedPosition;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public ulong GetBestMove(ulong CachedPosition)
        {
            var fromSquare = Engine.CachedPosition.BestMoveFrom(CachedPosition);
            if (fromSquare == Square.Illegal) return Move.Null; // Cached position does not specify a best move.
            var bestMove = Move.Null;
            Move.SetFrom(ref bestMove, fromSquare);
            Move.SetTo(ref bestMove, Engine.CachedPosition.BestMoveTo(CachedPosition));
            Move.SetPromotedPiece(ref bestMove, Engine.CachedPosition.BestMovePromotedPiece(CachedPosition));
            Move.SetIsBest(ref bestMove, true);
            var validMove = _validateMove(ref bestMove);
            return validMove ? bestMove : Move.Null;
        }


        public void Reset()
        {
            for (var index = 0; index < _positions.Length; index++) _positions[index] = NullPosition;
            Positions = 0;
            Searches = 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndex(ulong Key)
        {
            // Ensure even distribution of indices by using GetHashCode method rather than raw Zobrist Key for modular division.
            var index = Key.GetHashCode() % _indices; // Index may be negative.
            // Ensure index is positive using technique faster than Math.Abs().  See http://graphics.stanford.edu/~seander/bithacks.html#IntegerAbs.
            var mask = index >> 31;
            return (index ^ mask) - mask;
        }
    }
}
