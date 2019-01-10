// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine
{
    public readonly struct CachedPosition
    {
        private const int _scorePadding = 131_072;
        public static readonly ulong Null;
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
        public readonly ulong Key;
        public readonly ulong Data;


        // CachedPosition bits

        // 6 6 6 6 5 5 5 5 5 5 5 5 5 5 4 4 4 4 4 4 4 4 4 4 3 3 3 3 3 3 3 3 3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0
        // 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
        // To Horizon |Best From    |Best To      |BMP    |Score                                                      |SP |Last Accessed

        // Best From = Best Move From (one extra bit for illegal square)
        // Best To =   Best Move To   (one extra bit for illegal square)
        // BMP =       Best Move Promoted Piece
        // SP =        Score Precision


        static CachedPosition()
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
            // Set null position.
            Null = 0;
            // Set score first so padding is applied, ensuring a positive number is stored. 
            SetScore(ref Null, StaticScore.NotCached);
            SetToHorizon(ref Null, 0);
            SetBestMoveFrom(ref Null, Square.Illegal); // An illegal square indicates no best move stored in cached position.
            SetBestMoveTo(ref Null, Square.Illegal);
            SetBestMovePromotedPiece(ref Null, Piece.None);
            SetScorePrecision(ref Null, Engine.ScorePrecision.Unknown);
            SetLastAccessed(ref Null, 0);
        }


        public CachedPosition(ulong Key, ulong Data)
        {
            this.Key = Key;
            this.Data = Data;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToHorizon(ulong CachedPosition) => (int)((CachedPosition & _toHorizonMask) >> _toHorizonShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetToHorizon(ref ulong CachedPosition, int ToHorizon)
        {
            // Clear
            CachedPosition &= _toHorizonUnmask;
            // Set.
            CachedPosition |= (ulong)ToHorizon << _toHorizonShift;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.ToHorizon(CachedPosition) == ToHorizon);
            Debug.Assert(IsValid(CachedPosition));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMoveFrom(ulong CachedPosition) => (int)((CachedPosition & _bestMoveFromMask) >> _bestMoveFromShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMoveFrom(ref ulong CachedPosition, int BestMoveFrom)
        {
            // Clear
            CachedPosition &= _bestMoveFromUnmask;
            // Set.
            CachedPosition |= (ulong)BestMoveFrom << _bestMoveFromShift;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.BestMoveFrom(CachedPosition) == BestMoveFrom);
            Debug.Assert(IsValid(CachedPosition));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMoveTo(ulong CachedPosition) => (int)((CachedPosition & _bestMoveToMask) >> _bestMoveToShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMoveTo(ref ulong CachedPosition, int BestMoveTo)
        {
            // Clear
            CachedPosition &= _bestMoveToUnmask;
            // Set.
            CachedPosition |= (ulong)BestMoveTo << _bestMoveToShift;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.BestMoveTo(CachedPosition) == BestMoveTo);
            Debug.Assert(IsValid(CachedPosition));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMovePromotedPiece(ulong CachedPosition) => (int)((CachedPosition & _bestMovePromotedPieceMask) >> _bestMovePromotedPieceShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMovePromotedPiece(ref ulong CachedPosition, int BestMovePromotedPiece)
        {
            // Clear
            CachedPosition &= _bestMovePromotedPieceUnmask;
            // Set.
            CachedPosition |= (ulong)BestMovePromotedPiece << _bestMovePromotedPieceShift;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.BestMovePromotedPiece(CachedPosition) == BestMovePromotedPiece);
            Debug.Assert(IsValid(CachedPosition));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Score(ulong CachedPosition) => (int)((CachedPosition & _scoreMask) >> _scoreShift) - _scorePadding; // Cached score is a positive number.


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetScore(ref ulong CachedPosition, int Score)
        {
            // Ensure cached score is a positive number.
            int score = Score + _scorePadding;
            // Clear
            CachedPosition &= _scoreUnmask;
            // Set.
            CachedPosition |= (ulong)score << _scoreShift;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.Score(CachedPosition) == Score);
            Debug.Assert(IsValid(CachedPosition));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScorePrecision ScorePrecision(ulong CachedPosition) => (ScorePrecision)((CachedPosition & _scorePrecisionMask) >> _scorePrecisionShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetScorePrecision(ref ulong CachedPosition, ScorePrecision ScorePrecision)
        {
            ulong scorePrecision = (ulong)ScorePrecision;
            // Clear
            CachedPosition &= _scorePrecisionUnmask;
            // Set.
            CachedPosition |= scorePrecision << _scorePrecisionShift;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.ScorePrecision(CachedPosition) == ScorePrecision);
            Debug.Assert(IsValid(CachedPosition));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte LastAccessed(ulong CachedPosition) => (byte)(CachedPosition & _lastAccessedMask);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLastAccessed(ref ulong CachedPosition, byte LastAccessed)
        {
            // Clear
            CachedPosition &= _lastAccessedUnmask;
            // Set.
            CachedPosition |= LastAccessed;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.LastAccessed(CachedPosition) == LastAccessed);
            Debug.Assert(IsValid(CachedPosition));
        }


        private static bool IsValid(ulong CachedPosition)
        {
            Debug.Assert(ToHorizon(CachedPosition) <= Search.MaxHorizon);
            Debug.Assert(BestMoveFrom(CachedPosition) <= Square.Illegal);
            Debug.Assert(BestMoveTo(CachedPosition) <= Square.Illegal);
            Debug.Assert(BestMovePromotedPiece(CachedPosition) >= Piece.None);
            Debug.Assert(BestMovePromotedPiece(CachedPosition) != Piece.WhitePawn);
            Debug.Assert(BestMovePromotedPiece(CachedPosition) != Piece.WhiteKing);
            Debug.Assert(BestMovePromotedPiece(CachedPosition) != Piece.BlackPawn);
            Debug.Assert(BestMovePromotedPiece(CachedPosition) < Piece.BlackKing);
            Debug.Assert(Score(CachedPosition) >= -StaticScore.Max);
            Debug.Assert(Score(CachedPosition) <= StaticScore.Max);
            return true;
        }
    }
}