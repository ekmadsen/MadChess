// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Core.Moves;


public static class Move
{
    // History has 48 - 21 + 1 = 28 bits.
    // Eliminate one bit to prevent overflow caused by zero (adds one distinct value to range).
    // 2 Pow 27 = 134_217_728.
    // Value may be positive or negative, so max value is 134_217_728 / 2.
    public const int HistoryMaxValue = 67_108_864;
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
    private static readonly int _fromShift;
    private static readonly ulong _fromMask;
    private static readonly ulong _fromUnmask;
    private static readonly ulong _toMask;
    private static readonly ulong _toUnmask;


    // Move Bits
    // Higher priority moves have higher ulong value.

    // 6 6 6 6 5 5 5 5 5 5 5 5 5 5 4 4 4 4 4 4 4 4 4 4 3 3 3 3 3 3 3 3 3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0
    // 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
    // B|CapV   |CapA   |Promo  |Kil|History                                                |!|O|K|E|2|P|C|From         |To

    // B =     Best Move
    // CapV =  Capture Victim
    // CapA =  Capture Attacker (inverted)
    // Promo = Promoted Piece
    // Kil =   Killer Move
    // ! =     Played
    // O =     Castling
    // K =     King Move
    // E =     En Passant Capture
    // 2 =     Double Pawn Move
    // P =     Pawn Move
    // C =     Check
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
        _historyShift = 21;
        _historyMask = Bitwise.CreateULongMask(21, 48);
        _historyUnmask = Bitwise.CreateULongUnmask(21, 48);
        _playedShift = 20;
        _playedMask = Bitwise.CreateULongMask(20);
        _playedUnmask = Bitwise.CreateULongUnmask(20);
        _castlingShift = 19;
        _castlingMask = Bitwise.CreateULongMask(19);
        _castlingUnmask = Bitwise.CreateULongUnmask(19);
        _kingMoveShift = 18;
        _kingMoveMask = Bitwise.CreateULongMask(18);
        _kingMoveUnmask = Bitwise.CreateULongUnmask(18);
        _enPassantShift = 17;
        _enPassantMask = Bitwise.CreateULongMask(17);
        _enPassantUnmask = Bitwise.CreateULongUnmask(17);
        _doublePawnMoveShift = 16;
        _doublePawnMoveMask = Bitwise.CreateULongMask(16);
        _doublePawnMoveUnmask = Bitwise.CreateULongUnmask(16);
        _pawnMoveShift = 15;
        _pawnMoveMask = Bitwise.CreateULongMask(15);
        _pawnMoveUnmask = Bitwise.CreateULongUnmask(15);
        _checkShift = 14;
        _checkMask = Bitwise.CreateULongMask(14);
        _checkUnmask = Bitwise.CreateULongUnmask(14);
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
        SetFrom(ref Null, Square.Illegal);
        SetTo(ref Null, Square.Illegal);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBest(ulong move) => (move & _bestMask) >> _bestShift > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetIsBest(ref ulong move, bool isBest)
    {
        var value = isBest ? 1ul : 0;
        // Clear
        move &= _bestUnmask;
        // Set
        move |= (value << _bestShift) & _bestMask;
        // Validate move.
        Debug.Assert(IsBest(move) == isBest);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece CaptureVictim(ulong move) => (Piece)((move & _captureVictimMask) >> _captureVictimShift);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetCaptureVictim(ref ulong move, Piece captureVictim)
    {
        // Clear
        move &= _captureVictimUnmask;
        // Set
        move |= ((ulong)captureVictim << _captureVictimShift) & _captureVictimMask;
        // Validate move.
        Debug.Assert(CaptureVictim(move) == captureVictim);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece CaptureAttacker(ulong move)
    {
        // Value is inverted.
        var storedPiece = (int)((move & _captureAttackerMask) >> _captureAttackerShift);
        return Piece.BlackKing - storedPiece;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetCaptureAttacker(ref ulong move, Piece captureAttacker)
    {
        // Invert piece value so P x Q captures are given a higher priority than Q x Q.
        var storedPiece = (ulong)(Piece.BlackKing - captureAttacker);
        // Clear
        move &= _captureAttackerUnmask;
        // Set
        move |= (storedPiece << _captureAttackerShift) & _captureAttackerMask;
        // Validate move.
        Debug.Assert(CaptureAttacker(move) == captureAttacker);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece PromotedPiece(ulong move) => (Piece)((move & _promotedPieceMask) >> _promotedPieceShift);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetPromotedPiece(ref ulong move, Piece promotedPiece)
    {
        // Clear
        move &= _promotedPieceUnmask;
        // Set
        move |= ((ulong)promotedPiece << _promotedPieceShift) & _promotedPieceMask;
        // Validate move.
        Debug.Assert(PromotedPiece(move) == promotedPiece);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Killer(ulong move) => (int)((move & _killerMask) >> _killerShift);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetKiller(ref ulong move, int killer)
    {
        // Clear
        move &= _killerUnmask;
        // Set
        move |= ((ulong)killer << _killerShift) & _killerMask;
        // Validate move.
        Debug.Assert(Killer(move) == killer);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int History(ulong move) => (int) ((move & _historyMask) >> _historyShift) - HistoryMaxValue;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetHistory(ref ulong move, int history)
    {
        // Ensure history is >= 0 before shifting into ulong.
        var value = history + HistoryMaxValue;
        // Clear
        move &= _historyUnmask;
        // Set
        move |= ((ulong)value << _historyShift) & _historyMask;
        // Validate move.
        Debug.Assert(History(move) == history);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Played(ulong move) => (move & _playedMask) >> _playedShift > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetPlayed(ref ulong move, bool played)
    {
        var value = played ? 1ul : 0;
        // Clear
        move &= _playedUnmask;
        // Set
        move |= (value << _playedShift) & _playedMask;
        // Validate move.
        Debug.Assert(Played(move) == played);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCastling(ulong move) => (move & _castlingMask) >> _castlingShift > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetIsCastling(ref ulong move, bool isCastling)
    {
        var value = isCastling ? 1ul : 0;
        // Clear
        move &= _castlingUnmask;
        // Set
        move |= (value << _castlingShift) & _castlingMask;
        // Validate move.
        Debug.Assert(IsCastling(move) == isCastling);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKingMove(ulong move) => (move & _kingMoveMask) >> _kingMoveShift > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetIsKingMove(ref ulong move, bool isKingMove)
    {
        var value = isKingMove ? 1ul : 0;
        // Clear
        move &= _kingMoveUnmask;
        // Set
        move |= (value << _kingMoveShift) & _kingMoveMask;
        // Validate move.
        Debug.Assert(IsKingMove(move) == isKingMove);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEnPassantCapture(ulong move) => (move & _enPassantMask) >> _enPassantShift > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetIsEnPassantCapture(ref ulong move, bool isEnPassantCapture)
    {
        var value = isEnPassantCapture ? 1ul : 0;
        // Clear
        move &= _enPassantUnmask;
        // Set
        move |= (value << _enPassantShift) & _enPassantMask;
        // Validate move.
        Debug.Assert(IsEnPassantCapture(move) == isEnPassantCapture);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPawnMove(ulong move) => (move & _pawnMoveMask) >> _pawnMoveShift > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetIsPawnMove(ref ulong move, bool isPawnMove)
    {
        var value = isPawnMove ? 1ul : 0;
        // Clear
        move &= _pawnMoveUnmask;
        // Set
        move |= (value << _pawnMoveShift) & _pawnMoveMask;
        // Validate move.
        Debug.Assert(IsPawnMove(move) == isPawnMove);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCheck(ulong move) => (move & _checkMask) >> _checkShift > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetIsCheck(ref ulong move, bool isCheck)
    {
        var value = isCheck ? 1ul : 0;
        // Clear
        move &= _checkUnmask;
        // Set
        move |= (value << _checkShift) & _checkMask;
        // Validate move.
        Debug.Assert(IsCheck(move) == isCheck);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDoublePawnMove(ulong move) => (move & _doublePawnMoveMask) >> _doublePawnMoveShift > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetIsDoublePawnMove(ref ulong move, bool isDoublePawnMove)
    {
        var value = isDoublePawnMove ? 1ul : 0;
        // Clear
        move &= _doublePawnMoveUnmask;
        // Set
        move |= (value << _doublePawnMoveShift) & _doublePawnMoveMask;
        // Validate move.
        Debug.Assert(IsDoublePawnMove(move) == isDoublePawnMove);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsQuiet(ulong move) => (move & (_captureVictimMask | _promotedPieceMask)) == 0; // Not a capture or pawn promotion.


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square From(ulong move) => (Square)((move & _fromMask) >> _fromShift);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFrom(ref ulong move, Square from)
    {
        // Clear
        move &= _fromUnmask;
        // Set
        move |= ((ulong)from << _fromShift) & _fromMask;
        // Validate move.
        Debug.Assert(From(move) == from);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square To(ulong move) => (Square)(move & _toMask);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTo(ref ulong move, Square to)
    {
        // Clear
        move &= _toUnmask;
        // Set
        move |= (ulong)to & _toMask;
        // Validate move.
        Debug.Assert(To(move) == to);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(ulong move1, ulong move2) => (From(move1) == From(move2)) && (To(move1) == To(move2)) && (PromotedPiece(move1) == PromotedPiece(move2));


    public static ulong ParseLongAlgebraic(string longAlgebraic, Color colorToMove)
    {
        var fromSquare = Board.GetSquare(longAlgebraic.Substring(0, 2));
        var toSquare = Board.GetSquare(longAlgebraic.Substring(2, 2));
        // Set case of promoted piece character based on side to move.
        var promotedPiece = longAlgebraic.Length == 5
            ? PieceHelper.ParseChar(colorToMove == Color.White ? char.ToUpper(longAlgebraic[4]) : char.ToLower(longAlgebraic[4]))
            : Piece.None;
        var move = Null;
        SetFrom(ref move, fromSquare);
        SetTo(ref move, toSquare);
        SetPromotedPiece(ref move, promotedPiece);
        return move;
    }


    public static ulong ParseStandardAlgebraic(Board board, string standardAlgebraic)
    {
        var move = Null;
        // Remove check and checkmate symbols.
        var standardAlgebraicNoCheck = standardAlgebraic.TrimEnd("+#".ToCharArray());
        // ReSharper disable once SwitchStatementMissingSomeCases
        switch (standardAlgebraicNoCheck)
        {
            case "O-O-O":
            case "0-0-0":
                // Castle Queenside
                SetFrom(ref move, Board.CastleFromSquares[(int)board.CurrentPosition.ColorToMove]);
                SetTo(ref move, Board.CastleToSquares[(int)board.CurrentPosition.ColorToMove][(int)BoardSide.Queen]);
                if (!board.ValidateMove(ref move)) throw new Exception($"Move {standardAlgebraic} is illegal in position {board.CurrentPosition.ToFen()}.");
                return move;
            case "O-O":
            case "0-0":
                // Castle Kingside
                SetFrom(ref move, Board.CastleFromSquares[(int)board.CurrentPosition.ColorToMove]);
                SetTo(ref move, Board.CastleToSquares[(int)board.CurrentPosition.ColorToMove][(int)BoardSide.King]);
                if (!board.ValidateMove(ref move)) throw new Exception($"Move {standardAlgebraic} is illegal in position {board.CurrentPosition.ToFen()}.");
                return move;
        }
        var length = standardAlgebraicNoCheck.Length;
        var fromFile = -1;
        var fromRank = -1;
        var promotedPiece = Piece.None;
        Piece piece;
        Square toSquare;
        if (char.IsLower(standardAlgebraicNoCheck, 0))
        {
            // Pawn Move
            piece = PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, board.CurrentPosition.ColorToMove);
            fromFile = Board.Files[(int)Board.GetSquare($"{standardAlgebraicNoCheck[0]}1")];
            switch (length)
            {
                case 2:
                    // Pawn Move
                    toSquare = Board.GetSquare(standardAlgebraicNoCheck);
                    break;
                case 4 when standardAlgebraicNoCheck[1] == 'x':
                    // Pawn Capture
                    toSquare = Board.GetSquare(standardAlgebraicNoCheck.Substring(2, 2));
                    break;
                case 4 when standardAlgebraicNoCheck[2] == '=':
                    // Pawn promotion.  Set case of promoted piece character based on side to move.
                    toSquare = Board.GetSquare(standardAlgebraicNoCheck.Substring(0, 2));
                    promotedPiece = PieceHelper.ParseChar(board.CurrentPosition.ColorToMove == Color.White
                        ? char.ToUpper(standardAlgebraicNoCheck[length - 1])
                        : char.ToLower(standardAlgebraicNoCheck[length - 1]));
                    break;
                case 6:
                    // Pawn promotion with capture.  Set case of promoted piece character based on side to move.
                    toSquare = Board.GetSquare(standardAlgebraicNoCheck.Substring(2, 2));
                    promotedPiece = PieceHelper.ParseChar(board.CurrentPosition.ColorToMove == Color.White
                        ? char.ToUpper(standardAlgebraicNoCheck[length - 1])
                        : char.ToLower(standardAlgebraicNoCheck[length - 1]));
                    break;
                default:
                    throw new Exception($"Move {standardAlgebraic} is illegal in position {board.CurrentPosition.ToFen()}.");
            }
        }
        else
        {
            // Piece Move
            piece = PieceHelper.ParseChar(board.CurrentPosition.ColorToMove == Color.White
                ? char.ToUpper(standardAlgebraicNoCheck[0])
                : char.ToLower(standardAlgebraicNoCheck[0]));
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (standardAlgebraicNoCheck[1] == 'x')
            {
                // Piece Capture
                var square = standardAlgebraicNoCheck.Substring(2, 2);
                toSquare = Board.GetSquare(square);
            }
            else if (standardAlgebraicNoCheck[2] == 'x')
            {
                // Piece Capture with Disambiguation
                var square = standardAlgebraicNoCheck.Substring(3, 2);
                toSquare = Board.GetSquare(square);
                if (char.IsLetter(standardAlgebraicNoCheck[1])) fromFile = Board.Files[(int)Board.GetSquare($"{standardAlgebraicNoCheck[1]}1")]; // Piece disambiguated by file.
                else fromRank = Board.Ranks[(int)Color.White][(int)Board.GetSquare($"a{standardAlgebraicNoCheck[1]}")]; // Piece disambiguated by rank.
            }
            else if ((length > 3) && (standardAlgebraicNoCheck[3] == 'x'))
            {
                // Piece Capture with From Square Specified
                var square = standardAlgebraicNoCheck.Substring(4, 2);
                toSquare = Board.GetSquare(square);
                fromFile = Board.Files[(int)Board.GetSquare($"{standardAlgebraicNoCheck[1]}1")];
                fromRank = Board.Ranks[(int)Color.White][(int)Board.GetSquare($"a{standardAlgebraicNoCheck[2]}")];
            }
            else if (length == 3)
            {
                // Piece Move
                var square = standardAlgebraicNoCheck.Substring(1, 2);
                toSquare = Board.GetSquare(square);
            }
            else if (length == 4)
            {
                // Piece Move with Disambiguation
                var square = standardAlgebraicNoCheck.Substring(2, 2);
                toSquare = Board.GetSquare(square);
                if (char.IsLetter(standardAlgebraicNoCheck[1])) fromFile = Board.Files[(int)Board.GetSquare($"{standardAlgebraicNoCheck[1]}1")]; // Piece disambiguated by file.
                else fromRank = Board.Ranks[(int)Color.White][(int)Board.GetSquare($"a{standardAlgebraicNoCheck[1]}")]; // Piece disambiguated by rank.
            }
            else if (length == 5)
            {
                // Piece Move with From Square Specified
                var square = standardAlgebraicNoCheck.Substring(3, 2);
                toSquare = Board.GetSquare(square);
                fromFile = Board.Files[(int)Board.GetSquare($"{standardAlgebraicNoCheck[1]}1")];
                fromRank = Board.Ranks[(int)Color.White][(int)Board.GetSquare($"a{standardAlgebraicNoCheck[2]}")];
            }
            else throw new Exception($"{standardAlgebraic} move not supported.");
        }
        board.CurrentPosition.GenerateMoves();
        for (var moveIndex = 0; moveIndex < board.CurrentPosition.MoveIndex; moveIndex++)
        {
            move = board.CurrentPosition.Moves[moveIndex];
            if (!board.IsMoveLegal(ref move)) continue; // Skip illegal move.
            var movePiece = board.CurrentPosition.GetPiece(From(move));
            if (movePiece != piece) continue; // Wrong Piece
            var moveToSquare = To(move);
            if (moveToSquare != toSquare) continue; // Wrong Square
            var movePromotedPiece = PromotedPiece(move);
            if (movePromotedPiece != promotedPiece) continue; // Wrong Promoted Piece
            if (fromFile >= 0)
            {
                // Piece disambiguated by file.
                var moveFromFile = Board.Files[(int)From(move)];
                if (moveFromFile != fromFile) continue; // Wrong File
            }
            if (fromRank >= 0)
            {
                // Piece disambiguated by rank.
                var moveFromRank = Board.Ranks[(int)Color.White][(int)From(move)];
                if (moveFromRank != fromRank) continue; // Wrong Rank
            }
            if (!board.ValidateMove(ref move)) throw new Exception($"Move {standardAlgebraic} is illegal in position {board.CurrentPosition.ToFen()}.");
            return move;
        }
        throw new Exception($"Failed to parse {standardAlgebraic} standard algebraic notation move.");
    }


    public static bool IsValid(ulong move)
    {
        Debug.Assert(CaptureVictim(move) >= Piece.None, $"CaptureVictim(Move) = {CaptureVictim(move)}, Piece.None = {Piece.None}");
        Debug.Assert(CaptureVictim(move) < Piece.BlackKing, $"CaptureVictim(Move) = {CaptureVictim(move)}, Piece.BlackKing = {Piece.BlackKing}");
        Debug.Assert(CaptureVictim(move) != Piece.WhiteKing, $"CaptureVictim(Move) = {CaptureVictim(move)}, Piece.WhiteKing = {Piece.WhiteKing}");
        Debug.Assert(CaptureVictim(move) != Piece.BlackKing, $"CaptureVictim(Move) = {CaptureVictim(move)}, Piece.BlackKing = {Piece.BlackKing}");
        Debug.Assert(CaptureAttacker(move) >= Piece.None, $"CaptureAttacker(Move) = {CaptureAttacker(move)}, Piece.None = {Piece.None}");
        Debug.Assert(CaptureAttacker(move) <= Piece.BlackKing, $"CaptureAttacker(Move) = {CaptureAttacker(move)}, Piece.BlackKing = {Piece.BlackKing}");
        Debug.Assert(PromotedPiece(move) >= Piece.None, $"PromotedPiece(Move) = {PromotedPiece(move)}, Piece.None = {Piece.None}");
        Debug.Assert(PromotedPiece(move) < Piece.BlackKing, $"PromotedPiece(Move) = {PromotedPiece(move)}, Piece.BlackKing = {Piece.BlackKing}");
        Debug.Assert(PromotedPiece(move) != Piece.WhitePawn, $"PromotedPiece(Move) = {PromotedPiece(move)}, Piece.WhitePawn = {Piece.WhitePawn}");
        Debug.Assert(PromotedPiece(move) != Piece.BlackPawn, $"PromotedPiece(Move) = {PromotedPiece(move)}, Piece.BlackPawn = {Piece.BlackPawn}");
        Debug.Assert(PromotedPiece(move) != Piece.WhiteKing, $"PromotedPiece(Move) = {PromotedPiece(move)}, Piece.WhiteKing = {Piece.WhiteKing}");
        Debug.Assert(PromotedPiece(move) != Piece.BlackKing, $"PromotedPiece(Move) = {PromotedPiece(move)}, Piece.BlackKing = {Piece.BlackKing}");
        Debug.Assert(Killer(move) >= 0, $"Killer(Move) = {Killer(move)}");
        Debug.Assert(Killer(move) <= 2, $"Killer(Move) = {Killer(move)}");
        Debug.Assert(From(move) >= Square.A8, $"From(Move) = {From(move)}");
        Debug.Assert(From(move) <= Square.Illegal, $"From(Move) = {From(move)}");
        Debug.Assert(To(move) >= Square.A8, $"To(Move) = {To(move)}");
        Debug.Assert(To(move) <= Square.Illegal, $"To(Move) = {To(move)}");
        return true;
    }


    public static string ToLongAlgebraic(ulong move)
    {
        if (move == Null) return "Null";
        var fromSquare = From(move);
        var toSquare = To(move);
        var promotedPiece = PromotedPiece(move);
        return $"{Board.SquareLocations[(int)fromSquare]}{Board.SquareLocations[(int)toSquare]}{(promotedPiece == Piece.None ? string.Empty : PieceHelper.GetChar(promotedPiece).ToString().ToLower())}";
    }


    public static string ToString(ulong move)
    {
        return $"{ToLongAlgebraic(move)} (B = {IsBest(move)}, CapV = {PieceHelper.GetChar(CaptureVictim(move))}, CapA = {PieceHelper.GetChar(CaptureAttacker(move))}, Promo = {PieceHelper.GetChar(PromotedPiece(move))}, Kil = {Killer(move)}, " +
               $"! = {Played(move)},  O = {IsCastling(move)}, K = {IsKingMove(move)}, E = {IsEnPassantCapture(move)}, 2 = {IsDoublePawnMove(move)}, P = {IsPawnMove(move)}, C = {IsCheck(move)}, Q = {IsQuiet(move)} " +
               $"From = {From(move)}, To = {To(move)})";
    }
}