// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Heuristics;
using ErikTheCoder.MadChess.Engine.Score;


namespace ErikTheCoder.MadChess.Engine.Hashtable;


public sealed class Cache
{
    public static readonly int CapacityPerMegabyte = 1024 * 1024 / Marshal.SizeOf(typeof(CachedPosition));
    public readonly CachedPosition NullPosition;
    public int Positions;
    public byte Searches;
    private const int _buckets = 4;
    private readonly Stats _stats;
    private readonly Core.Delegates.ValidateMove _validateMove;
    private int _indices;
    private CachedPosition[] _positions; // More memory efficient than a jagged array that has a .NET object header for each sub-array (for garbage collection tracking of reachable-from-root).
        
        
    public int Capacity
    {
        get => _positions.Length;
        set
        {
            _positions = null;
            GC.Collect();
            _positions = new CachedPosition[value];
            _indices = value / _buckets;
            Reset();
        }
    }


    public Cache(int sizeMegabyte, Stats stats, Core.Delegates.ValidateMove validateMove)
    {
        _stats = stats;
        _validateMove = validateMove;
        // Set null position.
        NullPosition = new CachedPosition(0, 0);
        CachedPositionData.SetToHorizon(ref NullPosition.Data, 0);
        CachedPositionData.SetBestMoveFrom(ref NullPosition.Data, Square.Illegal); // An illegal square indicates no best move stored in cached position.
        CachedPositionData.SetBestMoveTo(ref NullPosition.Data, Square.Illegal);
        CachedPositionData.SetBestMovePromotedPiece(ref NullPosition.Data, Piece.None);
        CachedPositionData.SetScore(ref NullPosition.Data, StaticScore.NotCached);
        CachedPositionData.SetScorePrecision(ref NullPosition.Data, ScorePrecision.Unknown);
        CachedPositionData.SetLastAccessed(ref NullPosition.Data, 0);
        // Set capacity (which resets position array).
        Capacity = sizeMegabyte * CapacityPerMegabyte;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public CachedPosition GetPosition(ulong key)
    {
        _stats.CacheProbes++;
        var index = GetIndex(key);
        for (var bucket = 0; bucket < _buckets; bucket++)
        {
            var bucketIndex = index + bucket;
            var position = _positions[bucketIndex];
            if (position.Key == key)
            {
                // Position is cached.
                _stats.CacheHits++;
                CachedPositionData.SetLastAccessed(ref position.Data, Searches);
                _positions[bucketIndex] = position;
                Debug.Assert(CachedPositionData.IsValid(position.Data));
                return position;
            }
        }
        // Position is not cached.
        Debug.Assert(CachedPositionData.IsValid(NullPosition.Data));
        return NullPosition;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void SetPosition(CachedPosition cachedPosition)
    {
        Debug.Assert(CachedPositionData.IsValid(cachedPosition.Data));
        var index = GetIndex(cachedPosition.Key);
        CachedPositionData.SetLastAccessed(ref cachedPosition.Data, Searches);
        // Find oldest bucket.
        var earliestAccess = byte.MaxValue;
        var oldestBucketIndex = 0;
        for (var bucket = 0; bucket < _buckets; bucket++)
        {
            var bucketIndex = index + bucket;
            var position = _positions[bucketIndex];
            if (position.Key == cachedPosition.Key)
            {
                // Position is cached.  Overwrite position.
                Debug.Assert(CachedPositionData.IsValid(cachedPosition.Data));
                _positions[bucketIndex] = cachedPosition;
                return;
            }
            var lastAccessed = CachedPositionData.LastAccessed(position.Data);
            if (lastAccessed < earliestAccess)
            {
                earliestAccess = lastAccessed;
                oldestBucketIndex = bucketIndex;
            }
        }
        if (_positions[oldestBucketIndex].Key == NullPosition.Key) Positions++; // Oldest bucket has not been used.
        // Overwrite oldest bucket.
        Debug.Assert(CachedPositionData.IsValid(cachedPosition.Data));
        _positions[oldestBucketIndex] = cachedPosition;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ulong GetBestMove(ulong cachedPosition)
    {
        _stats.CacheBestMoveProbes++;
        Debug.Assert(CachedPositionData.IsValid(cachedPosition));
        var fromSquare = CachedPositionData.BestMoveFrom(cachedPosition);
        if (fromSquare == Square.Illegal) return Move.Null; // Cached position does not specify a best move.
        var bestMove = Move.Null;
        Move.SetFrom(ref bestMove, fromSquare);
        Move.SetTo(ref bestMove, CachedPositionData.BestMoveTo(cachedPosition));
        Move.SetPromotedPiece(ref bestMove, CachedPositionData.BestMovePromotedPiece(cachedPosition));
        Move.SetIsBest(ref bestMove, true);
        var validMove = _validateMove(ref bestMove);
        if (validMove) _stats.CacheValidBestMove++;
        else _stats.CacheInvalidBestMove++;
        Debug.Assert(Move.IsValid(bestMove));
        return validMove ? bestMove : Move.Null;
    }


    public void Reset()
    {
        for (var index = 0; index < _positions.Length; index++) _positions[index] = NullPosition;
        Positions = 0;
        Searches = 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(ulong key)
    {
        // Ensure even distribution of indices by using GetHashCode method rather than raw Zobrist key for modular division.
        var index = (key.GetHashCode() % _indices) * _buckets;
        // Ensure index is positive.
        return FastMath.Abs(index);
    }
}