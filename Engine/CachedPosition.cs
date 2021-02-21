// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;


namespace ErikTheCoder.MadChess.Engine
{
    public static class CachedPosition
    {
        private const int _scorePadding = 16_384;
        private static readonly ulong _partialKeyMask;
        private static readonly ulong _partialKeyUnmask;
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
        // Partial Key                  |To Horizon |Best From    |Best To      |BMP    |Score                        |SP |Last Accessed

        // Best From = Best Move From (one extra bit for illegal square)
        // Best To =   Best Move To   (one extra bit for illegal square)
        // BMP =       Best Move Promoted Piece
        // SP =        Score Precision


        static CachedPosition()
        {
            // Create bit shifts and masks.
            _partialKeyMask = Bitwise.CreateULongMask(49, 63);
            _partialKeyUnmask = Bitwise.CreateULongUnmask(49, 63);
            _toHorizonShift = 43;
            _toHorizonMask = Bitwise.CreateULongMask(43, 48);
            _toHorizonUnmask = Bitwise.CreateULongUnmask(43, 48);
            _bestMoveFromShift = 36;
            _bestMoveFromMask = Bitwise.CreateULongMask(36, 42);
            _bestMoveFromUnmask = Bitwise.CreateULongUnmask(36, 42);
            _bestMoveToShift = 29;
            _bestMoveToMask = Bitwise.CreateULongMask(29, 35);
            _bestMoveToUnmask = Bitwise.CreateULongUnmask(29, 35);
            _bestMovePromotedPieceShift = 25;
            _bestMovePromotedPieceMask = Bitwise.CreateULongMask(25, 28);
            _bestMovePromotedPieceUnmask = Bitwise.CreateULongUnmask(25, 28);
            _scoreShift = 10;
            _scoreMask = Bitwise.CreateULongMask(10, 24);
            _scoreUnmask = Bitwise.CreateULongUnmask(10, 24);
            _scorePrecisionShift = 8;
            _scorePrecisionMask = Bitwise.CreateULongMask(8, 9);
            _scorePrecisionUnmask = Bitwise.CreateULongUnmask(8, 9);
            _lastAccessedMask = Bitwise.CreateULongMask(0, 7);
            _lastAccessedUnmask = Bitwise.CreateULongUnmask(0, 7);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong PartialKey(ulong CachedPosition) => CachedPosition & _partialKeyMask;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPartialKey(ref ulong CachedPosition, ulong PartialKey)
        {
            // Clear
            CachedPosition &= _partialKeyUnmask;
            // Set
            CachedPosition |= PartialKey & _partialKeyMask;
            // Validate partial key.
            Debug.Assert(Engine.CachedPosition.PartialKey(CachedPosition) == PartialKey);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToHorizon(ulong CachedPosition) => (int)((CachedPosition & _toHorizonMask) >> _toHorizonShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetToHorizon(ref ulong CachedPosition, int ToHorizon)
        {
            // Clear
            CachedPosition &= _toHorizonUnmask;
            // Set
            CachedPosition |= ((ulong)ToHorizon << _toHorizonShift) & _toHorizonMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.ToHorizon(CachedPosition) == ToHorizon);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMoveFrom(ulong CachedPosition) => (int)((CachedPosition & _bestMoveFromMask) >> _bestMoveFromShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMoveFrom(ref ulong CachedPosition, int BestMoveFrom)
        {
            // Clear
            CachedPosition &= _bestMoveFromUnmask;
            // Set
            CachedPosition |= ((ulong)BestMoveFrom << _bestMoveFromShift) & _bestMoveFromMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.BestMoveFrom(CachedPosition) == BestMoveFrom);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMoveTo(ulong CachedPosition) => (int)((CachedPosition & _bestMoveToMask) >> _bestMoveToShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMoveTo(ref ulong CachedPosition, int BestMoveTo)
        {
            // Clear
            CachedPosition &= _bestMoveToUnmask;
            // Set
            CachedPosition |= ((ulong)BestMoveTo << _bestMoveToShift) & _bestMoveToMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.BestMoveTo(CachedPosition) == BestMoveTo);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMovePromotedPiece(ulong CachedPosition) => (int)((CachedPosition & _bestMovePromotedPieceMask) >> _bestMovePromotedPieceShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMovePromotedPiece(ref ulong CachedPosition, int BestMovePromotedPiece)
        {
            // Clear
            CachedPosition &= _bestMovePromotedPieceUnmask;
            // Set
            CachedPosition |= ((ulong)BestMovePromotedPiece << _bestMovePromotedPieceShift) & _bestMovePromotedPieceMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.BestMovePromotedPiece(CachedPosition) == BestMovePromotedPiece);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Score(ulong CachedPosition) => (int)((CachedPosition & _scoreMask) >> _scoreShift) - _scorePadding; // Cached score is a positive number.


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetScore(ref ulong CachedPosition, int Score)
        {
            // Ensure cached score is a positive number.
            var score = Score + _scorePadding;
            // Clear
            CachedPosition &= _scoreUnmask;
            // Set
            CachedPosition |= ((ulong)score << _scoreShift) & _scoreMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.Score(CachedPosition) == Score);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScorePrecision ScorePrecision(ulong CachedPosition) => (ScorePrecision)((CachedPosition & _scorePrecisionMask) >> _scorePrecisionShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetScorePrecision(ref ulong CachedPosition, ScorePrecision ScorePrecision)
        {
            var scorePrecision = (ulong)ScorePrecision;
            // Clear
            CachedPosition &= _scorePrecisionUnmask;
            // Set
            CachedPosition |= (scorePrecision << _scorePrecisionShift) & _scorePrecisionMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.ScorePrecision(CachedPosition) == ScorePrecision);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte LastAccessed(ulong CachedPosition) => (byte)(CachedPosition & _lastAccessedMask);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLastAccessed(ref ulong CachedPosition, byte LastAccessed)
        {
            // Clear
            CachedPosition &= _lastAccessedUnmask;
            // Set
            CachedPosition |= LastAccessed & _lastAccessedMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPosition.LastAccessed(CachedPosition) == LastAccessed);
        }


        public static bool IsValid(ulong CachedPosition)
        {
            Debug.Assert(ToHorizon(CachedPosition) <= Search.MaxHorizon, $"ToHorizon(CachedPosition) = {ToHorizon(CachedPosition)}, Search.MaxHorizon = {Search.MaxHorizon}{Environment.NewLine}{ToString(CachedPosition)}");
            Debug.Assert(BestMoveFrom(CachedPosition) <= Square.Illegal, $"BestMoveFrom(CachedPosition) = {BestMoveFrom(CachedPosition)}, Square.Illegal = {Square.Illegal}{Environment.NewLine}{ToString(CachedPosition)}");
            Debug.Assert(BestMoveTo(CachedPosition) <= Square.Illegal, $"BestMoveTo(CachedPosition) = {BestMoveTo(CachedPosition)}, Square.Illegal = {Square.Illegal}{Environment.NewLine}{ToString(CachedPosition)}");
            Debug.Assert(BestMovePromotedPiece(CachedPosition) >= Piece.None, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(CachedPosition)}, Piece.None = {Piece.None}{Environment.NewLine}{ToString(CachedPosition)}");
            Debug.Assert(BestMovePromotedPiece(CachedPosition) != Piece.WhitePawn, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(CachedPosition)}, Piece.WhitePawn = {Piece.WhitePawn}{Environment.NewLine}{ToString(CachedPosition)}");
            Debug.Assert(BestMovePromotedPiece(CachedPosition) != Piece.WhiteKing, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(CachedPosition)}, Piece.WhiteKing = {Piece.WhiteKing}{Environment.NewLine}{ToString(CachedPosition)}");
            Debug.Assert(BestMovePromotedPiece(CachedPosition) != Piece.BlackPawn, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(CachedPosition)}, Piece.BlackPawn = {Piece.BlackPawn}{Environment.NewLine}{ToString(CachedPosition)}");
            Debug.Assert(BestMovePromotedPiece(CachedPosition) < Piece.BlackKing, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(CachedPosition)}, Piece.BlackKing = {Piece.BlackKing}{Environment.NewLine}{ToString(CachedPosition)}");
            Debug.Assert(Score(CachedPosition) >= -StaticScore.Max, $"Score(CachedPosition) = {Score(CachedPosition)}, -StaticScore.Max = {-StaticScore.Max}{Environment.NewLine}{ToString(CachedPosition)}");
            Debug.Assert(Score(CachedPosition) <= StaticScore.Max, $"Score(CachedPosition) = {Score(CachedPosition)}, StaticScore.Max = {StaticScore.Max}{Environment.NewLine}{ToString(CachedPosition)}");
            return true;
        }


        private static string ToString(ulong CachedPosition)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Partial Key = {Bitwise.ToString(PartialKey(CachedPosition))}");
            stringBuilder.AppendLine($"To Horizon = {ToHorizon(CachedPosition)}");
            stringBuilder.AppendLine($"Best Move From = {BestMoveFrom(CachedPosition)}");
            stringBuilder.AppendLine($"Best Move To = {BestMoveTo(CachedPosition)}");
            stringBuilder.AppendLine($"Best Move Promoted Piece = {BestMovePromotedPiece(CachedPosition)}");
            stringBuilder.AppendLine($"Score = {Score(CachedPosition)}");
            stringBuilder.AppendLine($"Score Precision = {ScorePrecision(CachedPosition)}");
            stringBuilder.AppendLine($"Last Accessed = {LastAccessed(CachedPosition)}");
            return stringBuilder.ToString();
        }
    }
}
