// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace MadChess.Engine
{
    public static class Move
    {
        public const int LongAlgebraicMaxLength = 5;
        private const int _historyPadding = 67_108_864; // History has 48 - 22 + 1 = 27 bits.  2 Pow 27 = 134_217_728.
        public static readonly ulong Null;
        private static readonly int _bestShift;
        private static readonly ulong _bestMask;
        private static readonly ulong _bestUnmask;
        private static readonly int _captureVictimShift;
        private static readonly ulong _captureVictimMask;
        private static readonly ulong _captureVictimUnmask;
        private static readonly int _captureAttackerShift;
        private static readonly ulong _captureAttackerMask;
        private static readonly ulong _captureAttackerUnmask;
        private static readonly int _promotedPieceShift;
        private static readonly ulong _promotedPieceMask;
        private static readonly ulong _promotedPieceUnmask;
        private static readonly int _killerShift;
        private static readonly ulong _killerMask;
        private static readonly ulong _killerUnmask;
        private static readonly int _historyShift;
        private static readonly ulong _historyMask;
        private static readonly ulong _historyUnmask;
        private static readonly int _playedShift;
        private static readonly ulong _playedMask;
        private static readonly ulong _playedUnmask;
        private static readonly int _castlingShift;
        private static readonly ulong _castlingMask;
        private static readonly ulong _castlingUnmask;
        private static readonly int _kingMoveShift;
        private static readonly ulong _kingMoveMask;
        private static readonly ulong _kingMoveUnmask;
        private static readonly int _enPassantShift;
        private static readonly ulong _enPassantMask;
        private static readonly ulong _enPassantUnmask;
        private static readonly int _pawnMoveShift;
        private static readonly ulong _pawnMoveMask;
        private static readonly ulong _pawnMoveUnmask;
        private static readonly int _checkShift;
        private static readonly ulong _checkMask;
        private static readonly ulong _checkUnmask;
        private static readonly int _doublePawnMoveShift;
        private static readonly ulong _doublePawnMoveMask;
        private static readonly ulong _doublePawnMoveUnmask;
        private static readonly int _quietShift;
        private static readonly ulong _quietMask;
        private static readonly ulong _quietUnmask;
        private static readonly int _fromShift;
        private static readonly ulong _fromMask;
        private static readonly ulong _fromUnmask;
        private static readonly ulong _toMask;
        private static readonly ulong _toUnmask;


        // Move bits
        // Higher priority moves have higher ulong value.

        // 6 6 6 6 5 5 5 5 5 5 5 5 5 5 4 4 4 4 4 4 4 4 4 4 3 3 3 3 3 3 3 3 3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0
        // 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
        // B|CapV   |CapA   |Promo  |Kil|History                                              |!|O|K|E|D|P|C|Q|From         |To           

        // B =     Best Move
        // CapV =  Capture Victim
        // CapA =  Capture Attacker (inverted)
        // Promo = Promoted Piece
        // Kil  =  Killer Move
        // ! =     Played
        // O =     Castling
        // K =     King Move
        // E =     En Passant Capture
        // D =     Double Pawn Move
        // P =     Pawn Move
        // C =     Check
        // Q =     Quiet (not capture, pawn promotion, castling, or check)
        // From =  From (one extra bit for illegal square)
        // To =    To (one extra bit for illegal square)


        static Move()
        {
            // Create bit masks and shifts.
            _bestShift = 63;
            _bestMask = Bitwise.CreateULongMask(63);
            _bestUnmask = Bitwise.CreateULongUnmask(63);
            _captureVictimShift = 59;
            _captureVictimMask = Bitwise.CreateULongMask(59, 62);
            _captureVictimUnmask = Bitwise.CreateULongUnmask(59, 62);
            _captureAttackerShift = 55;
            _captureAttackerMask = Bitwise.CreateULongMask(55, 58);
            _captureAttackerUnmask = Bitwise.CreateULongUnmask(55, 58);
            _promotedPieceShift = 51;
            _promotedPieceMask = Bitwise.CreateULongMask(51, 54);
            _promotedPieceUnmask = Bitwise.CreateULongUnmask(51, 54);
            _killerShift = 49;
            _killerMask = Bitwise.CreateULongMask(49, 50);
            _killerUnmask = Bitwise.CreateULongUnmask(49, 50);
            _historyShift = 22;
            _historyMask = Bitwise.CreateULongMask(22, 48);
            _historyUnmask = Bitwise.CreateULongUnmask(22, 48);
            _playedShift = 21;
            _playedMask = Bitwise.CreateULongMask(21);
            _playedUnmask = Bitwise.CreateULongUnmask(21);
            _castlingShift = 20;
            _castlingMask = Bitwise.CreateULongMask(20);
            _castlingUnmask = Bitwise.CreateULongUnmask(20);
            _kingMoveShift = 19;
            _kingMoveMask = Bitwise.CreateULongMask(19);
            _kingMoveUnmask = Bitwise.CreateULongUnmask(19);
            _enPassantShift = 18;
            _enPassantMask = Bitwise.CreateULongMask(18);
            _enPassantUnmask = Bitwise.CreateULongUnmask(18);
            _doublePawnMoveShift = 17;
            _doublePawnMoveMask = Bitwise.CreateULongMask(17);
            _doublePawnMoveUnmask = Bitwise.CreateULongUnmask(17);
            _pawnMoveShift = 16;
            _pawnMoveMask = Bitwise.CreateULongMask(16);
            _pawnMoveUnmask = Bitwise.CreateULongUnmask(16);
            _checkShift = 15;
            _checkMask = Bitwise.CreateULongMask(15);
            _checkUnmask = Bitwise.CreateULongUnmask(15);
            _quietShift = 14;
            _quietMask = Bitwise.CreateULongMask(14);
            _quietUnmask = Bitwise.CreateULongUnmask(14);
            _fromShift = 7;
            _fromMask = Bitwise.CreateULongMask(7, 13);
            _fromUnmask = Bitwise.CreateULongUnmask(7, 13);
            _toMask = Bitwise.CreateULongMask(0, 6);
            _toUnmask = Bitwise.CreateULongUnmask(0, 6);
            // Set null move.
            Null = 0;
            SetIsBest(ref Null, false);
            SetCaptureVictim(ref Null, Piece.None);
            SetCaptureAttacker(ref Null, Piece.None);
            SetPromotedPiece(ref Null, Piece.None);
            SetKiller(ref Null, 0);
            SetHistory(ref Null, 0);
            SetPlayed(ref Null, false);
            SetIsCastling(ref Null, false);
            SetIsKingMove(ref Null, false);
            SetIsEnPassantCapture(ref Null, false);
            SetIsDoublePawnMove(ref Null, false);
            SetIsPawnMove(ref Null, false);
            SetIsCheck(ref Null, false);
            SetIsQuiet(ref Null, false);
            SetFrom(ref Null, Square.Illegal);
            SetTo(ref Null, Square.Illegal);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBest(ulong Move) => (Move & _bestMask) >> _bestShift > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIsBest(ref ulong Move, bool IsBest)
        {
            ulong isBest = IsBest ? 1ul : 0;
            // Clear
            Move &= _bestUnmask;
            // Set
            Move |= isBest << _bestShift;
            // Validate move.
            Debug.Assert(Engine.Move.IsBest(Move) == IsBest);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CaptureVictim(ulong Move) => (int)((Move & _captureVictimMask) >> _captureVictimShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCaptureVictim(ref ulong Move, int CaptureVictim)
        {
            // Clear
            Move &= _captureVictimUnmask;
            // Set
            Move |= (ulong) CaptureVictim << _captureVictimShift;
            // Validate move.
            Debug.Assert(Engine.Move.CaptureVictim(Move) == CaptureVictim);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CaptureAttacker(ulong Move)
        {
            // Value is inverted.
            int storedPiece = (int) ((Move & _captureAttackerMask) >> _captureAttackerShift);
            return 12 - storedPiece;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCaptureAttacker(ref ulong Move, int CaptureAttacker)
        {
            // Invert piece value so P x Q captures are given a higher priority than Q x Q.
            ulong storedPiece = (ulong) (12 - CaptureAttacker);
            // Clear
            Move &= _captureAttackerUnmask;
            // Set
            Move |= storedPiece << _captureAttackerShift;
            // Validate move.
            Debug.Assert(Engine.Move.CaptureAttacker(Move) == CaptureAttacker);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PromotedPiece(ulong Move) => (int) ((Move & _promotedPieceMask) >> _promotedPieceShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPromotedPiece(ref ulong Move, int PromotedPiece)
        {
            // Clear
            Move &= _promotedPieceUnmask;
            // Set.
            Move |= (ulong) PromotedPiece << _promotedPieceShift;
            // Validate move.
            Debug.Assert(Engine.Move.PromotedPiece(Move) == PromotedPiece);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Killer(ulong Move) => (int) ((Move & _killerMask) >> _killerShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetKiller(ref ulong Move, int Killer)
        {
            // Clear
            Move &= _killerUnmask;
            // Set
            Move |= (ulong) Killer << _killerShift;
            // Validate move.
            Debug.Assert(Engine.Move.Killer(Move) == Killer);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int History(ulong Move) => (int) ((Move & _historyMask) >> _historyShift) - _historyPadding; // History score is a positive number.


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetHistory(ref ulong Move, int History)
        {
            // Ensure history score is a positive number.
            int history = History + _historyPadding;
            // Clear
            Move &= _historyUnmask;
            // Set
            Move |= (ulong) history << _historyShift;
            // Validate move.
            Debug.Assert(Engine.Move.History(Move) == History);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Played(ulong Move) => (Move & _playedMask) >> _playedShift > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPlayed(ref ulong Move, bool Played)
        {
            ulong played = Played ? 1ul : 0;
            // Clear
            Move &= _playedUnmask;
            // Set
            Move |= played << _playedShift;
            // Validate move.
            Debug.Assert(Engine.Move.Played(Move) == Played);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCastling(ulong Move) => (Move & _castlingMask) >> _castlingShift > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIsCastling(ref ulong Move, bool IsCastling)
        {
            ulong isCastling = IsCastling ? 1ul : 0;
            // Clear
            Move &= _castlingUnmask;
            // Set
            Move |= isCastling << _castlingShift;
            // Validate move.
            Debug.Assert(Engine.Move.IsCastling(Move) == IsCastling);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKingMove(ulong Move) => (Move & _kingMoveMask) >> _kingMoveShift > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIsKingMove(ref ulong Move, bool IsKingMove)
        {
            ulong isKingMove = IsKingMove ? 1ul : 0;
            // Clear
            Move &= _kingMoveUnmask;
            // Set
            Move |= isKingMove << _kingMoveShift;
            // Validate move.
            Debug.Assert(Engine.Move.IsKingMove(Move) == IsKingMove);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnPassantCapture(ulong Move) => (Move & _enPassantMask) >> _enPassantShift > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIsEnPassantCapture(ref ulong Move, bool IsEnPassantCapture)
        {
            ulong isEnPassantCapture = IsEnPassantCapture ? 1ul : 0;
            // Clear
            Move &= _enPassantUnmask;
            // Set
            Move |= isEnPassantCapture << _enPassantShift;
            // Validate move.
            Debug.Assert(Engine.Move.IsEnPassantCapture(Move) == IsEnPassantCapture);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPawnMove(ulong Move) => (Move & _pawnMoveMask) >> _pawnMoveShift > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIsPawnMove(ref ulong Move, bool IsPawnMove)
        {
            ulong isPawnMove = IsPawnMove ? 1ul : 0;
            // Clear
            Move &= _pawnMoveUnmask;
            // Set
            Move |= isPawnMove << _pawnMoveShift;
            // Validate move.
            Debug.Assert(Engine.Move.IsPawnMove(Move) == IsPawnMove);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCheck(ulong Move) => (Move & _checkMask) >> _checkShift > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIsCheck(ref ulong Move, bool IsCheck)
        {
            ulong isCheck = IsCheck ? 1ul : 0;
            // Clear
            Move &= _checkUnmask;
            // Set
            Move |= isCheck << _checkShift;
            // Validate move.
            Debug.Assert(Engine.Move.IsCheck(Move) == IsCheck);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDoublePawnMove(ulong Move) => (Move & _doublePawnMoveMask) >> _doublePawnMoveShift > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIsDoublePawnMove(ref ulong Move, bool IsDoublePawnMove)
        {
            ulong isDoublePawnMove = IsDoublePawnMove ? 1ul : 0;
            // Clear
            Move &= _doublePawnMoveUnmask;
            // Set
            Move |= isDoublePawnMove << _doublePawnMoveShift;
            // Validate move.
            Debug.Assert(Engine.Move.IsDoublePawnMove(Move) == IsDoublePawnMove);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsQuiet(ulong Move) => (Move & _quietMask) >> _quietShift > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIsQuiet(ref ulong Move, bool IsQuiet)
        {
            ulong isQuiet = IsQuiet ? 1ul : 0;
            // Clear
            Move &= _quietUnmask;
            // Set
            Move |= isQuiet << _quietShift;
            // Validate move.
            Debug.Assert(Engine.Move.IsQuiet(Move) == IsQuiet);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int From(ulong Move) => (int) ((Move & _fromMask) >> _fromShift);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFrom(ref ulong Move, int From)
        {
            // Clear
            Move &= _fromUnmask;
            // Set
            Move |= (ulong) From << _fromShift;
            // Validate move.
            Debug.Assert(Engine.Move.From(Move) == From);
            Debug.Assert(IsValid(Move));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int To(ulong Move) => (int) ((Move & _toMask) >> 0);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetTo(ref ulong Move, int To)
        {
            // Clear
            Move &= _toUnmask;
            // Set
            Move |= (ulong) To << 0;
            // Validate move.
            Debug.Assert(Engine.Move.To(Move) == To);
            Debug.Assert(IsValid(Move));
        }


        public static bool Equals(ulong Move1, ulong Move2)
        {
            if (From(Move1) == From(Move2))
            {
                if (To(Move1) == To(Move2)) return PromotedPiece(Move1) == PromotedPiece(Move2);
                return false;
            }
            return false;
        }


        public static ulong ParseLongAlgebraic(string LongAlgebraic, bool WhiteMove)
        {
            int fromSquare = Board.GetSquare(LongAlgebraic.Substring(0, 2));
            int toSquare = Board.GetSquare(LongAlgebraic.Substring(2, 2));
            // Set case of promoted piece character based on side to move.
            int promotedPiece = LongAlgebraic.Length == 5
                ? Piece.ParseChar(WhiteMove ? char.ToUpper(LongAlgebraic[4]) : char.ToLower(LongAlgebraic[4]))
                : Piece.None;
            ulong move = Null;
            SetFrom(ref move, fromSquare);
            SetTo(ref move, toSquare);
            SetPromotedPiece(ref move, promotedPiece);
            return move;
        }


        public static ulong ParseStandardAlgebraic(Board Board, string StandardAlgebraic)
        {
            ulong move = Null;
            // Remove check and checkmate symbols.
            string standardAlgebraicNoCheck = StandardAlgebraic.TrimEnd("+#".ToCharArray());
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (standardAlgebraicNoCheck)
            {
                case "O-O-O":
                case "0-0-0":
                    if (Board.CurrentPosition.WhiteMove)
                    {
                        // White castle queenside
                        SetFrom(ref move, Square.e1);
                        SetTo(ref move, Square.c1);
                        if (!Board.ValidateMove(ref move)) throw new Exception($"Move {StandardAlgebraic} is illegal in position {Board.CurrentPosition.ToFen()}.");
                        return move;
                    }
                    // Black castle queenside
                    SetFrom(ref move, Square.e8);
                    SetTo(ref move, Square.c8);
                    if (!Board.ValidateMove(ref move)) throw new Exception($"Move {StandardAlgebraic} is illegal in position {Board.CurrentPosition.ToFen()}.");
                    return move;
                case "O-O":
                case "0-0":
                    if (Board.CurrentPosition.WhiteMove)
                    {
                        // White castle kingside
                        SetFrom(ref move, Square.e1);
                        SetTo(ref move, Square.g1);
                        if (!Board.ValidateMove(ref move)) throw new Exception($"Move {StandardAlgebraic} is illegal in position {Board.CurrentPosition.ToFen()}.");
                        return move;
                    }
                    // Black castle kingside
                    SetFrom(ref move, Square.e8);
                    SetTo(ref move, Square.g8);
                    if (!Board.ValidateMove(ref move)) throw new Exception($"Move {StandardAlgebraic} is illegal in position {Board.CurrentPosition.ToFen()}.");
                    return move;
            }
            int length = standardAlgebraicNoCheck.Length;
            int piece;
            int fromFile = -1;
            int fromRank = -1;
            int toSquare;
            int promotedPiece = Piece.None;
            if (char.IsLower(standardAlgebraicNoCheck, 0))
            {
                // Pawn move
                piece = Board.CurrentPosition.WhiteMove ? Piece.WhitePawn : Piece.BlackPawn;
                fromFile = Board.Files[Board.GetSquare($"{standardAlgebraicNoCheck[0]}1")];
                switch (length)
                {
                    case 2:
                        // Pawn move
                        toSquare = Board.GetSquare(standardAlgebraicNoCheck);
                        break;
                    case 4 when standardAlgebraicNoCheck[1] == 'x':
                        // Pawn capture
                        toSquare = Board.GetSquare(standardAlgebraicNoCheck.Substring(2, 2));
                        break;
                    case 4 when standardAlgebraicNoCheck[2] == '=':
                        // Pawn promotion.  Set case of promoted piece character based on side to move.
                        toSquare = Board.GetSquare(standardAlgebraicNoCheck.Substring(0, 2));
                        promotedPiece = Piece.ParseChar(Board.CurrentPosition.WhiteMove
                            ? char.ToUpper(standardAlgebraicNoCheck[length - 1])
                            : char.ToLower(standardAlgebraicNoCheck[length - 1]));
                        break;
                    case 6:
                        // Pawn promotion with capture.  Set case of promoted piece character based on side to move.
                        toSquare = Board.GetSquare(standardAlgebraicNoCheck.Substring(2, 2));
                        promotedPiece = Piece.ParseChar(Board.CurrentPosition.WhiteMove
                            ? char.ToUpper(standardAlgebraicNoCheck[length - 1])
                            : char.ToLower(standardAlgebraicNoCheck[length - 1]));
                        break;
                    default:
                        throw new Exception($"Move {StandardAlgebraic} is illegal in position {Board.CurrentPosition.ToFen()}.");
                }
            }
            else
            {
                // Piece move
                piece = Piece.ParseChar(Board.CurrentPosition.WhiteMove
                    ? char.ToUpper(standardAlgebraicNoCheck[0])
                    : char.ToLower(standardAlgebraicNoCheck[0]));
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (standardAlgebraicNoCheck[1] == 'x')
                {
                    // Piece capture
                    string square = standardAlgebraicNoCheck.Substring(2, 2);
                    toSquare = Board.GetSquare(square);
                }
                else if (standardAlgebraicNoCheck[2] == 'x')
                {
                    // Piece capture with disambiguation
                    string square = standardAlgebraicNoCheck.Substring(3, 2);
                    toSquare = Board.GetSquare(square);
                    if (char.IsLetter(standardAlgebraicNoCheck[1])) fromFile = Board.Files[Board.GetSquare($"{standardAlgebraicNoCheck[1]}1")]; // Piece disambiguated by file.
                    else fromRank = Board.WhiteRanks[Board.GetSquare($"a{standardAlgebraicNoCheck[1]}")]; // Piece disambiguated by rank.
                }
                else if (length == 3)
                {
                    // Piece move
                    string square = standardAlgebraicNoCheck.Substring(1, 2);
                    toSquare = Board.GetSquare(square);
                }
                else if (length == 4)
                {
                    Debugger.Break();


                    // Piece move with disambiguation
                    string square = standardAlgebraicNoCheck.Substring(2, 2);
                    toSquare = Board.GetSquare(square);
                    if (char.IsLetter(standardAlgebraicNoCheck[1])) fromFile = Board.Files[Board.GetSquare($"{standardAlgebraicNoCheck[1]}1")]; // Piece disambiguated by file.
                    else fromRank = Board.WhiteRanks[Board.GetSquare($"a{standardAlgebraicNoCheck[1]}")]; // Piece disambiguated by rank.
                }
                else throw new Exception($"{StandardAlgebraic} move not supported.");
            }
            Board.CurrentPosition.GenerateMoves();
            for (int moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++)
            {
                move = Board.CurrentPosition.Moves[moveIndex];
                if (!Board.IsMoveLegal(ref move)) continue; // Skip illegal move.
                int movePiece = Board.CurrentPosition.GetPiece(From(move));
                if (movePiece != piece) continue; // Wrong piece
                int moveToSquare = To(move);
                if (moveToSquare != toSquare) continue; // Wrong square
                int movePromotedPiece = PromotedPiece(move);
                if (movePromotedPiece != promotedPiece) continue; // Wrong promoted piece
                if (fromFile >= 0)
                {
                    // Piece disambiguated by file.
                    int moveFromFile = Board.Files[From(move)];
                    if (moveFromFile != fromFile) continue; // Wrong file
                }
                if (fromRank >= 0)
                {
                    // Piece disambiguated by rank.
                    // Use white ranks regardless of side to move.
                    int moveFromRank = Board.WhiteRanks[From(move)];
                    if (moveFromRank != fromRank) continue; // Wrong rank
                }
                if (!Board.ValidateMove(ref move)) throw new Exception($"Move {StandardAlgebraic} is illegal in position {Board.CurrentPosition.ToFen()}.");
                return move;
            }
            throw new Exception($"Failed to parse {StandardAlgebraic} standard algebraic notation move.");
        }


        private static bool IsValid(ulong Move) => true; // TODO: Validate move.


        public static string ToLongAlgebraic(ulong Move)
        {
            if (Move == Null) return "Null";
            int fromSquare = From(Move);
            int toSquare = To(Move);
            int promotedPiece = PromotedPiece(Move);
            return $"{Board.SquareLocations[fromSquare]}{Board.SquareLocations[toSquare]}{(promotedPiece == Piece.None ? string.Empty : Piece.GetChar(promotedPiece).ToString().ToLower())}";
        }


        public static string ToString(ulong Move)
        {
            return $"{ToLongAlgebraic(Move)} (B = {IsBest(Move)}, CapV = {Piece.GetChar(CaptureVictim(Move))}, CapA = {Piece.GetChar(CaptureAttacker(Move))}, Promo = {Piece.GetChar(PromotedPiece(Move))}, O = {IsCastling(Move)}, " +
                   $"K = {IsKingMove(Move)}, E = {IsEnPassantCapture(Move)}, D = {IsDoublePawnMove(Move)}, P = {IsPawnMove(Move)}, C = {IsCheck(Move)}, Q = {IsQuiet(Move)}";
        }
    }
}