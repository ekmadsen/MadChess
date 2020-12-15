// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine
{
    public static class CachedPositionData
    {
        private const int _scorePadding = 131_072;
        private static readonly int _toHorizonShift;
        private static readonly ulong _toHorizonMask;
        private static readonly ulong _toHorizonUnmask;
        private static readonly int _bestMoveFromShift;
        private static readonly ulong _bestMoveFromMask;
        private static readonly ulong _bestMoveFromUnmask;
        private static readonly int _bestMoveToShift;
        private static readonly ulong _bestMoveToMask;
        private static readonly ulong _bestMoveToUnmask;
        private static readonly int _bestMovePromotedPieceShift;
        private static readonly ulong _bestMovePromotedPieceMask;
        private static readonly ulong _bestMovePromotedPieceUnmask;
        private static readonly int _scoreShift;
        private static readonly ulong _scoreMask;
        private static readonly ulong _scoreUnmask;
        private static readonly int _scorePrecisionShift;
        private static readonly ulong _scorePrecisionMask;
        private static readonly ulong _scorePrecisionUnmask;
        private static readonly ulong _lastAccessedMask;
        private static readonly ulong _lastAccessedUnmask;


        // CachedPosition.Data Bits

        // 6 6 6 6 5 5 5 5 5 5 5 5 5 5 4 4 4 4 4 4 4 4 4 4 3 3 3 3 3 3 3 3 3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0
        // 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
        // To Horizon |Best From    |Best To      |BMP    |Score                                                      |SP |Last Accessed

        // Best From = Best Move From (one extra bit for illegal square)
        // Best To =   Best Move To   (one extra bit for illegal square)
        // BMP =       Best Move Promoted Piece
        // SP =        Score Precision


        static CachedPositionData()
        {
            // Create bit shifts and masks.
            _toHorizonShift = 58;
            _toHorizonMask = Bitwise.CreateULongMask(58, 63);
            _toHorizonUnmask = Bitwise.CreateULongUnmask(58, 63);
            _bestMoveFromShift = 51;
            _bestMoveFromMask = Bitwise.CreateULongMask(51, 57);
            _bestMoveFromUnmask = Bitwise.CreateULongUnmask(51, 57);
            _bestMoveToShift = 44;
            _bestMoveToMask = Bitwise.CreateULongMask(44, 50);
            _bestMoveToUnmask = Bitwise.CreateULongUnmask(44, 50);
            _bestMovePromotedPieceShift = 40;
            _bestMovePromotedPieceMask = Bitwise.CreateULongMask(40, 43);
            _bestMovePromotedPieceUnmask = Bitwise.CreateULongUnmask(40, 43);
            _scoreShift = 10;
            _scoreMask = Bitwise.CreateULongMask(10, 39);
            _scoreUnmask = Bitwise.CreateULongUnmask(10, 39);
            _scorePrecisionShift = 8;
            _scorePrecisionMask = Bitwise.CreateULongMask(8, 9);
            _scorePrecisionUnmask = Bitwise.CreateULongUnmask(8, 9);
            _lastAccessedMask = Bitwise.CreateULongMask(0, 7);
            _lastAccessedUnmask = Bitwise.CreateULongUnmask(0, 7);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToHorizon(ulong Data) => (int)((Data & _toHorizonMask) >> _toHorizonShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetToHorizon(ref ulong Data, int ToHorizon)
        {
            // Clear
            Data &= _toHorizonUnmask;
            // Set
            Data |= ((ulong)ToHorizon << _toHorizonShift) & _toHorizonMask;
            // Validate cached position.
            Debug.Assert(CachedPositionData.ToHorizon(Data) == ToHorizon);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMoveFrom(ulong Data) => (int)((Data & _bestMoveFromMask) >> _bestMoveFromShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMoveFrom(ref ulong Data, int BestMoveFrom)
        {
            // Clear
            Data &= _bestMoveFromUnmask;
            // Set
            Data |= ((ulong)BestMoveFrom << _bestMoveFromShift) & _bestMoveFromMask;
            // Validate cached position.
            Debug.Assert(CachedPositionData.BestMoveFrom(Data) == BestMoveFrom);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMoveTo(ulong Data) => (int)((Data & _bestMoveToMask) >> _bestMoveToShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMoveTo(ref ulong Data, int BestMoveTo)
        {
            // Clear
            Data &= _bestMoveToUnmask;
            // Set
            Data |= ((ulong)BestMoveTo << _bestMoveToShift) & _bestMoveToMask;
            // Validate cached position.
            Debug.Assert(CachedPositionData.BestMoveTo(Data) == BestMoveTo);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMovePromotedPiece(ulong Data) => (int)((Data & _bestMovePromotedPieceMask) >> _bestMovePromotedPieceShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMovePromotedPiece(ref ulong Data, int BestMovePromotedPiece)
        {
            // Clear
            Data &= _bestMovePromotedPieceUnmask;
            // Set
            Data |= ((ulong)BestMovePromotedPiece << _bestMovePromotedPieceShift) & _bestMovePromotedPieceMask;
            // Validate cached position.
            Debug.Assert(CachedPositionData.BestMovePromotedPiece(Data) == BestMovePromotedPiece);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Score(ulong Data) => (int)((Data & _scoreMask) >> _scoreShift) - _scorePadding; // Cached score is a positive number.


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetScore(ref ulong Data, int Score)
        {
            // Ensure cached score is a positive number.
            var score = Score + _scorePadding;
            // Clear
            Data &= _scoreUnmask;
            // Set
            Data |= ((ulong)score << _scoreShift) & _scoreMask;
            // Validate cached position.
            Debug.Assert(CachedPositionData.Score(Data) == Score);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScorePrecision ScorePrecision(ulong Data) => (ScorePrecision)((Data & _scorePrecisionMask) >> _scorePrecisionShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetScorePrecision(ref ulong Data, ScorePrecision ScorePrecision)
        {
            var scorePrecision = (ulong)ScorePrecision;
            // Clear
            Data &= _scorePrecisionUnmask;
            // Set
            Data |= (scorePrecision << _scorePrecisionShift) & _scorePrecisionMask;
            // Validate cached position.
            Debug.Assert(CachedPositionData.ScorePrecision(Data) == ScorePrecision);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte LastAccessed(ulong Data) => (byte)(Data & _lastAccessedMask);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLastAccessed(ref ulong Data, byte LastAccessed)
        {
            // Clear
            Data &= _lastAccessedUnmask;
            // Set
            Data |= LastAccessed & _lastAccessedMask;
            // Validate cached position.
            Debug.Assert(CachedPositionData.LastAccessed(Data) == LastAccessed);
        }


        public static void Clear(ref ulong Data)
        {
            SetToHorizon(ref Data, 0);
            SetBestMoveFrom(ref Data, Square.Illegal); // An illegal square indicates no best move stored in cached position.
            SetBestMoveTo(ref Data, Square.Illegal);
            SetBestMovePromotedPiece(ref Data, Piece.None);
            SetScore(ref Data, StaticScore.NotCached);
            SetScorePrecision(ref Data, Engine.ScorePrecision.Unknown);
            SetLastAccessed(ref Data, 0);
        }


        public static bool IsValid(ulong Data)
        {
            Debug.Assert(ToHorizon(Data) <= Search.MaxHorizon);
            Debug.Assert(BestMoveFrom(Data) <= Square.Illegal);
            Debug.Assert(BestMoveTo(Data) <= Square.Illegal);
            Debug.Assert(BestMovePromotedPiece(Data) >= Piece.None);
            Debug.Assert(BestMovePromotedPiece(Data) != Piece.WhitePawn);
            Debug.Assert(BestMovePromotedPiece(Data) != Piece.WhiteKing);
            Debug.Assert(BestMovePromotedPiece(Data) != Piece.BlackPawn);
            Debug.Assert(BestMovePromotedPiece(Data) < Piece.BlackKing);
            Debug.Assert(Score(Data) >= -StaticScore.Max);
            Debug.Assert(Score(Data) <= StaticScore.Max);
            return true;
        }
    }
}
