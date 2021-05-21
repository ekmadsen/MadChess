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
    public static class CachedPositionData
    {
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
        public static int ToHorizon(ulong CachedPositionData) => (int)((CachedPositionData & _toHorizonMask) >> _toHorizonShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetToHorizon(ref ulong CachedPositionData, int ToHorizon)
        {
            // Clear
            CachedPositionData &= _toHorizonUnmask;
            // Set
            CachedPositionData |= ((ulong)ToHorizon << _toHorizonShift) & _toHorizonMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPositionData.ToHorizon(CachedPositionData) == ToHorizon);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMoveFrom(ulong CachedPositionData) => (int)((CachedPositionData & _bestMoveFromMask) >> _bestMoveFromShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMoveFrom(ref ulong CachedPositionData, int BestMoveFrom)
        {
            // Clear
            CachedPositionData &= _bestMoveFromUnmask;
            // Set
            CachedPositionData |= ((ulong)BestMoveFrom << _bestMoveFromShift) & _bestMoveFromMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPositionData.BestMoveFrom(CachedPositionData) == BestMoveFrom);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMoveTo(ulong CachedPositionData) => (int)((CachedPositionData & _bestMoveToMask) >> _bestMoveToShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMoveTo(ref ulong CachedPositionData, int BestMoveTo)
        {
            // Clear
            CachedPositionData &= _bestMoveToUnmask;
            // Set
            CachedPositionData |= ((ulong)BestMoveTo << _bestMoveToShift) & _bestMoveToMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPositionData.BestMoveTo(CachedPositionData) == BestMoveTo);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BestMovePromotedPiece(ulong CachedPositionData) => (int)((CachedPositionData & _bestMovePromotedPieceMask) >> _bestMovePromotedPieceShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBestMovePromotedPiece(ref ulong CachedPositionData, int BestMovePromotedPiece)
        {
            // Clear
            CachedPositionData &= _bestMovePromotedPieceUnmask;
            // Set
            CachedPositionData |= ((ulong)BestMovePromotedPiece << _bestMovePromotedPieceShift) & _bestMovePromotedPieceMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPositionData.BestMovePromotedPiece(CachedPositionData) == BestMovePromotedPiece);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Score(ulong CachedPositionData) => (int)((CachedPositionData & _scoreMask) >> _scoreShift) - StaticScore.Max; // Cached score is a positive number.


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetScore(ref ulong CachedPositionData, int Score)
        {
            // Ensure cached score is a positive number.
            var score = Score + StaticScore.Max;
            // Clear
            CachedPositionData &= _scoreUnmask;
            // Set
            CachedPositionData |= ((ulong)score << _scoreShift) & _scoreMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPositionData.Score(CachedPositionData) == Score);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScorePrecision ScorePrecision(ulong CachedPositionData) => (ScorePrecision)((CachedPositionData & _scorePrecisionMask) >> _scorePrecisionShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetScorePrecision(ref ulong CachedPositionData, ScorePrecision ScorePrecision)
        {
            var scorePrecision = (ulong)ScorePrecision;
            // Clear
            CachedPositionData &= _scorePrecisionUnmask;
            // Set
            CachedPositionData |= (scorePrecision << _scorePrecisionShift) & _scorePrecisionMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPositionData.ScorePrecision(CachedPositionData) == ScorePrecision);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte LastAccessed(ulong CachedPositionData) => (byte)(CachedPositionData & _lastAccessedMask);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLastAccessed(ref ulong CachedPositionData, byte LastAccessed)
        {
            // Clear
            CachedPositionData &= _lastAccessedUnmask;
            // Set
            CachedPositionData |= LastAccessed & _lastAccessedMask;
            // Validate cached position.
            Debug.Assert(Engine.CachedPositionData.LastAccessed(CachedPositionData) == LastAccessed);
        }


        public static bool IsValid(ulong CachedPositionData)
        {
            Debug.Assert(ToHorizon(CachedPositionData) <= Search.MaxHorizon, $"ToHorizon(CachedPosition) = {ToHorizon(CachedPositionData)}, Search.MaxHorizon = {Search.MaxHorizon}{Environment.NewLine}{ToString(CachedPositionData)}");
            Debug.Assert(BestMoveFrom(CachedPositionData) <= Square.Illegal, $"BestMoveFrom(CachedPosition) = {BestMoveFrom(CachedPositionData)}, Square.Illegal = {Square.Illegal}{Environment.NewLine}{ToString(CachedPositionData)}");
            Debug.Assert(BestMoveTo(CachedPositionData) <= Square.Illegal, $"BestMoveTo(CachedPosition) = {BestMoveTo(CachedPositionData)}, Square.Illegal = {Square.Illegal}{Environment.NewLine}{ToString(CachedPositionData)}");
            Debug.Assert(BestMovePromotedPiece(CachedPositionData) >= Piece.None, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(CachedPositionData)}, Piece.None = {Piece.None}{Environment.NewLine}{ToString(CachedPositionData)}");
            Debug.Assert(BestMovePromotedPiece(CachedPositionData) != Piece.WhitePawn, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(CachedPositionData)}, Piece.WhitePawn = {Piece.WhitePawn}{Environment.NewLine}{ToString(CachedPositionData)}");
            Debug.Assert(BestMovePromotedPiece(CachedPositionData) != Piece.WhiteKing, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(CachedPositionData)}, Piece.WhiteKing = {Piece.WhiteKing}{Environment.NewLine}{ToString(CachedPositionData)}");
            Debug.Assert(BestMovePromotedPiece(CachedPositionData) != Piece.BlackPawn, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(CachedPositionData)}, Piece.BlackPawn = {Piece.BlackPawn}{Environment.NewLine}{ToString(CachedPositionData)}");
            Debug.Assert(BestMovePromotedPiece(CachedPositionData) < Piece.BlackKing, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(CachedPositionData)}, Piece.BlackKing = {Piece.BlackKing}{Environment.NewLine}{ToString(CachedPositionData)}");
            Debug.Assert(Score(CachedPositionData) >= -StaticScore.Max, $"Score(CachedPosition) = {Score(CachedPositionData)}, -StaticScore.Max = {-StaticScore.Max}{Environment.NewLine}{ToString(CachedPositionData)}");
            Debug.Assert(Score(CachedPositionData) <= StaticScore.Max, $"Score(CachedPosition) = {Score(CachedPositionData)}, StaticScore.Max = {StaticScore.Max}{Environment.NewLine}{ToString(CachedPositionData)}");
            return true;
        }


        private static string ToString(ulong CachedPositionData)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"To Horizon = {ToHorizon(CachedPositionData)}");
            stringBuilder.AppendLine($"Best Move From = {BestMoveFrom(CachedPositionData)}");
            stringBuilder.AppendLine($"Best Move To = {BestMoveTo(CachedPositionData)}");
            stringBuilder.AppendLine($"Best Move Promoted Piece = {BestMovePromotedPiece(CachedPositionData)}");
            stringBuilder.AppendLine($"Score = {Score(CachedPositionData)}");
            stringBuilder.AppendLine($"Score Precision = {ScorePrecision(CachedPositionData)}");
            stringBuilder.AppendLine($"Last Accessed = {LastAccessed(CachedPositionData)}");
            return stringBuilder.ToString();
        }
    }
}
