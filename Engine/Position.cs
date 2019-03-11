// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Text;


namespace ErikTheCoder.MadChess.Engine
{
    // Use a struct instead of a class to ensure allocating an array of Positions uses a contiguous block of memory.
    // This improves data locality, decreasing chance that accessing a Position causes a CPU cache miss.
    // See https://stackoverflow.com/questions/16699247/what-is-a-cache-friendly-code.
    public struct Position
    {
        public const int MaxMoves = 128;
        public readonly ulong[] Moves;
        public ulong WhitePawns;
        public ulong WhiteKnights;
        public ulong WhiteBishops;
        public ulong WhiteRooks;
        public ulong WhiteQueens;
        public ulong WhiteKing;
        public ulong OccupancyWhite;
        public ulong BlackPawns;
        public ulong BlackKnights;
        public ulong BlackBishops;
        public ulong BlackRooks;
        public ulong BlackQueens;
        public ulong BlackKing;
        public ulong OccupancyBlack;
        public ulong Occupancy;
        public ulong PotentiallyPinnedPieces;
        public bool WhiteMove;
        public uint Castling;
        public int EnPassantSquare;
        public int HalfMoveNumber;
        public int FullMoveNumber;
        public bool KingInCheck;
        public int CurrentMoveIndex;
        public int MoveIndex;
        public MoveGenerationStage MoveGenerationStage;
        public ulong PiecesSquaresKey;
        public ulong Key;
        public ulong PlayedMove;
        private readonly Board _board;


        public Position(Board Board)
        {
            _board = Board;
            Moves = new ulong[MaxMoves];
            WhitePawns = 0;
            WhiteKnights = 0;
            WhiteBishops = 0;
            WhiteRooks = 0;
            WhiteQueens = 0;
            WhiteKing = 0;
            OccupancyWhite = 0;
            BlackPawns = 0;
            BlackKnights = 0;
            BlackBishops = 0;
            BlackRooks = 0;
            BlackQueens = 0;
            BlackKing = 0;
            OccupancyBlack = 0;
            Occupancy = 0;
            PotentiallyPinnedPieces = 0;
            WhiteMove = true;
            Castling = 0;
            EnPassantSquare = Square.Illegal;
            HalfMoveNumber = 0;
            FullMoveNumber = 0;
            KingInCheck = false;
            CurrentMoveIndex = 0;
            MoveIndex = 0;
            MoveGenerationStage = MoveGenerationStage.BestMove;
            PiecesSquaresKey = 0;
            Key = 0;
            PlayedMove = Move.Null;
            Reset();
        }


        public int GetPiece(int Square)
        {
            ulong squareMask = Board.SquareMasks[Square];
            if ((Occupancy & squareMask) == 0) return Piece.None;
            if ((OccupancyWhite & squareMask) > 0)
            {
                // Locate white piece.
                if (Bitwise.FindFirstSetBit(WhitePawns & squareMask) != Engine.Square.Illegal) return Piece.WhitePawn;
                if (Bitwise.FindFirstSetBit(WhiteKnights & squareMask) != Engine.Square.Illegal) return Piece.WhiteKnight;
                if (Bitwise.FindFirstSetBit(WhiteBishops & squareMask) != Engine.Square.Illegal) return Piece.WhiteBishop;
                if (Bitwise.FindFirstSetBit(WhiteRooks & squareMask) != Engine.Square.Illegal) return Piece.WhiteRook;
                if (Bitwise.FindFirstSetBit(WhiteQueens & squareMask) != Engine.Square.Illegal) return Piece.WhiteQueen;
                if (Bitwise.FindFirstSetBit(WhiteKing & squareMask) != Engine.Square.Illegal) return Piece.WhiteKing;
                throw new Exception($"White piece not found at {Board.SquareLocations[Square]}.");
            }
            // Locate black piece.
            if (Bitwise.FindFirstSetBit(BlackPawns & squareMask) != Engine.Square.Illegal) return Piece.BlackPawn;
            if (Bitwise.FindFirstSetBit(BlackKnights & squareMask) != Engine.Square.Illegal) return Piece.BlackKnight;
            if (Bitwise.FindFirstSetBit(BlackBishops & squareMask) != Engine.Square.Illegal) return Piece.BlackBishop;
            if (Bitwise.FindFirstSetBit(BlackRooks & squareMask) != Engine.Square.Illegal) return Piece.BlackRook;
            if (Bitwise.FindFirstSetBit(BlackQueens & squareMask) != Engine.Square.Illegal) return Piece.BlackQueen;
            if (Bitwise.FindFirstSetBit(BlackKing & squareMask) != Engine.Square.Illegal) return Piece.BlackKing;
            throw new Exception($"Black piece not found at {Square}.");
        }


        public void Set(ref Position CopyFromPosition)
        {
            // Copy bitboards.
            WhitePawns = CopyFromPosition.WhitePawns;
            WhiteKnights = CopyFromPosition.WhiteKnights;
            WhiteBishops = CopyFromPosition.WhiteBishops;
            WhiteRooks = CopyFromPosition.WhiteRooks;
            WhiteQueens = CopyFromPosition.WhiteQueens;
            WhiteKing = CopyFromPosition.WhiteKing;
            OccupancyWhite = CopyFromPosition.OccupancyWhite;
            BlackPawns = CopyFromPosition.BlackPawns;
            BlackKnights = CopyFromPosition.BlackKnights;
            BlackBishops = CopyFromPosition.BlackBishops;
            BlackRooks = CopyFromPosition.BlackRooks;
            BlackQueens = CopyFromPosition.BlackQueens;
            BlackKing = CopyFromPosition.BlackKing;
            OccupancyBlack = CopyFromPosition.OccupancyBlack;
            Occupancy = CopyFromPosition.Occupancy;
            // Copy board state.  Do not copy values that will be set after move is played.
            WhiteMove = CopyFromPosition.WhiteMove;
            Castling = CopyFromPosition.Castling;
            EnPassantSquare = CopyFromPosition.EnPassantSquare;
            HalfMoveNumber = CopyFromPosition.HalfMoveNumber;
            FullMoveNumber = CopyFromPosition.FullMoveNumber;
            PiecesSquaresKey = CopyFromPosition.PiecesSquaresKey;
        }


        public void GenerateMoves()
        {
            PrepareMoveGeneration();
            FindPotentiallyPinnedPieces();
            GenerateMoves(MoveGeneration.AllMoves, Board.AllSquaresMask, Board.AllSquaresMask);
        }


        public void GenerateMoves(MoveGeneration MoveGeneration) => GenerateMoves(MoveGeneration, Board.AllSquaresMask, Board.AllSquaresMask);


        public void GenerateMoves(MoveGeneration MoveGeneration, ulong FromSquareMask, ulong ToSquareMask)
        {
            GeneratePawnMoves(MoveGeneration, FromSquareMask, ToSquareMask);
            GenerateKnightMoves(MoveGeneration, FromSquareMask, ToSquareMask);
            GenerateBishopMoves(MoveGeneration, FromSquareMask, ToSquareMask);
            GenerateRookMoves(MoveGeneration, FromSquareMask, ToSquareMask);
            GenerateQueenMoves(MoveGeneration, FromSquareMask, ToSquareMask);
            GenerateKingMoves(MoveGeneration, FromSquareMask, ToSquareMask);
        }


        public void PrepareMoveGeneration()
        {
            CurrentMoveIndex = 0;
            MoveIndex = 0;
            MoveGenerationStage = MoveGenerationStage.BestMove;
        }


        private void GeneratePawnMoves(MoveGeneration MoveGeneration, ulong FromSquareMask, ulong ToSquareMask)
        {
            ulong pawns;
            ulong[] pawnMoveMasks;
            ulong[] pawnDoubleMoveMasks;
            ulong[] pawnAttackMasks;
            ulong enemyOccupiedSquares;
            ulong unoccupiedSquares = ~Occupancy;
            int[] ranks;
            int attacker;
            int queen;
            int rook;
            int bishop;
            int knight;
            int enPassantVictim;
            if (WhiteMove)
            {
                // White move
                pawns = WhitePawns & FromSquareMask;
                pawnMoveMasks = _board.WhitePawnMoveMasks;
                pawnDoubleMoveMasks = _board.WhitePawnDoubleMoveMasks;
                pawnAttackMasks = _board.WhitePawnAttackMasks;
                enemyOccupiedSquares = OccupancyBlack;
                ranks = Board.WhiteRanks;
                attacker = Piece.WhitePawn;
                queen = Piece.WhiteQueen;
                rook = Piece.WhiteRook;
                bishop = Piece.WhiteBishop;
                knight = Piece.WhiteKnight;
                enPassantVictim = Piece.BlackPawn;
            }
            else
            {
                // Black move
                pawns = BlackPawns & FromSquareMask;
                pawnMoveMasks = _board.BlackPawnMoveMasks;
                pawnDoubleMoveMasks = _board.BlackPawnDoubleMoveMasks;
                pawnAttackMasks = _board.BlackPawnAttackMasks;
                enemyOccupiedSquares = OccupancyWhite;
                ranks = Board.BlackRanks;
                attacker = Piece.BlackPawn;
                queen = Piece.BlackQueen;
                rook = Piece.BlackRook;
                bishop = Piece.BlackBishop;
                knight = Piece.BlackKnight;
                enPassantVictim = Piece.WhitePawn;
            }
            int fromSquare;
            ulong move;
            if ((EnPassantSquare != Square.Illegal) && ((Board.SquareMasks[EnPassantSquare] & ToSquareMask) > 0) && (MoveGeneration != MoveGeneration.OnlyNonCaptures))
            {
                ulong enPassantAttackers = _board.EnPassantAttackerMasks[EnPassantSquare] & pawns;
                while ((fromSquare = Bitwise.FindFirstSetBit(enPassantAttackers)) != Square.Illegal)
                {
                    // Capture pawn en passant.
                    move = Move.Null;
                    Move.SetFrom(ref move, fromSquare);
                    Move.SetTo(ref move, EnPassantSquare);
                    Move.SetCaptureAttacker(ref move, attacker);
                    Move.SetCaptureVictim(ref move, enPassantVictim);
                    Move.SetIsEnPassantCapture(ref move, true);
                    Move.SetIsPawnMove(ref move, true);
                    Move.SetIsQuiet(ref move, false);
                    Moves[MoveIndex] = move;
                    MoveIndex++;
                    Bitwise.ClearBit(ref enPassantAttackers, fromSquare);
                }
            }
            while ((fromSquare = Bitwise.FindFirstSetBit(pawns)) != Square.Illegal)
            {
                ulong pawnDestinations;
                int toSquare;
                int toSquareRank;
                if (MoveGeneration != MoveGeneration.OnlyCaptures)
                {
                    // Pawns may move forward one square (or two if on initial square) if forward squares are unoccupied.
                    pawnDestinations = pawnMoveMasks[fromSquare] & unoccupiedSquares & ToSquareMask;
                    while ((toSquare = Bitwise.FindFirstSetBit(pawnDestinations)) != Square.Illegal)
                    {
                        bool doubleMove = Board.SquareDistances[fromSquare][toSquare] == 2;
                        if (doubleMove && ((Occupancy & pawnDoubleMoveMasks[fromSquare]) > 0))
                        {
                            // Double move is blocked.
                            Bitwise.ClearBit(ref pawnDestinations, toSquare);
                            continue;
                        }
                        toSquareRank = ranks[toSquare];
                        if (toSquareRank < 7)
                        {
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetIsDoublePawnMove(ref move, doubleMove);
                            Move.SetIsPawnMove(ref move, true);
                            Move.SetIsQuiet(ref move, true);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                        }
                        else
                        {
                            // Promote pawn to queen.
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, queen);
                            Move.SetIsPawnMove(ref move, true);
                            Move.SetIsQuiet(ref move, false);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                            // Promote pawn to rook.
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, rook);
                            Move.SetIsPawnMove(ref move, true);
                            Move.SetIsQuiet(ref move, false);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                            // Promote pawn to bishop.
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, bishop);
                            Move.SetIsPawnMove(ref move, true);
                            Move.SetIsQuiet(ref move, false);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                            // Promote pawn to knight.
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, knight);
                            Move.SetIsPawnMove(ref move, true);
                            Move.SetIsQuiet(ref move, false);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                        }
                        Bitwise.ClearBit(ref pawnDestinations, toSquare);
                    }
                }
                if (MoveGeneration != MoveGeneration.OnlyNonCaptures)
                {
                    // Pawns may attack diagonally forward one square if occupied by enemy.
                    pawnDestinations = pawnAttackMasks[fromSquare] & enemyOccupiedSquares & ToSquareMask;
                    while ((toSquare = Bitwise.FindFirstSetBit(pawnDestinations)) != Square.Illegal)
                    {
                        toSquareRank = ranks[toSquare];
                        int victim = GetPiece(toSquare);
                        if (toSquareRank < 7)
                        {
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetCaptureAttacker(ref move, attacker);
                            Move.SetCaptureVictim(ref move, victim);
                            Move.SetIsPawnMove(ref move, true);
                            Move.SetIsQuiet(ref move, false);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                        }
                        else
                        {
                            // Promote pawn to queen.
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, queen);
                            Move.SetCaptureAttacker(ref move, attacker);
                            Move.SetCaptureVictim(ref move, victim);
                            Move.SetIsPawnMove(ref move, true);
                            Move.SetIsQuiet(ref move, false);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                            // Promote pawn to rook.
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, rook);
                            Move.SetCaptureAttacker(ref move, attacker);
                            Move.SetCaptureVictim(ref move, victim);
                            Move.SetIsPawnMove(ref move, true);
                            Move.SetIsQuiet(ref move, false);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                            // Promote pawn to bishop.
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, bishop);
                            Move.SetCaptureAttacker(ref move, attacker);
                            Move.SetCaptureVictim(ref move, victim);
                            Move.SetIsPawnMove(ref move, true);
                            Move.SetIsQuiet(ref move, false);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                            // Promote pawn to knight.
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, knight);
                            Move.SetCaptureAttacker(ref move, attacker);
                            Move.SetCaptureVictim(ref move, victim);
                            Move.SetIsPawnMove(ref move, true);
                            Move.SetIsQuiet(ref move, false);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                        }
                        Bitwise.ClearBit(ref pawnDestinations, toSquare);
                    }
                }
                Bitwise.ClearBit(ref pawns, fromSquare);
            }
        }


        private void GenerateKnightMoves(MoveGeneration MoveGeneration, ulong FromSquareMask, ulong ToSquareMask)
        {
            ulong knights;
            ulong unOrEnemyOccupiedSquares;
            ulong enemyOccupiedSquares;
            int attacker;
            if (WhiteMove)
            {
                // White move
                knights = WhiteKnights & FromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyWhite;
                enemyOccupiedSquares = OccupancyBlack;
                attacker = Piece.WhiteKnight;
            }
            else
            {
                // Black move
                knights = BlackKnights & FromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyBlack;
                enemyOccupiedSquares = OccupancyWhite;
                attacker = Piece.BlackKnight;
            }
            int fromSquare;
            while ((fromSquare = Bitwise.FindFirstSetBit(knights)) != Square.Illegal)
            {
                ulong knightDestinations;
                switch (MoveGeneration)
                {
                    case MoveGeneration.AllMoves:
                        knightDestinations = _board.KnightMoveMasks[fromSquare] & unOrEnemyOccupiedSquares & ToSquareMask;
                        break;
                    case MoveGeneration.OnlyCaptures:
                        knightDestinations = _board.KnightMoveMasks[fromSquare] & enemyOccupiedSquares & ToSquareMask;
                        break;
                    case MoveGeneration.OnlyNonCaptures:
                        knightDestinations = _board.KnightMoveMasks[fromSquare] & ~Occupancy & ToSquareMask;
                        break;
                    default:
                        throw new Exception($"{MoveGeneration} move generation not supported.");
                }
                int toSquare;
                while ((toSquare = Bitwise.FindFirstSetBit(knightDestinations)) != Square.Illegal)
                {
                    int victim = GetPiece(toSquare);
                    ulong move = Move.Null;
                    Move.SetFrom(ref move, fromSquare);
                    Move.SetTo(ref move, toSquare);
                    if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                    Move.SetCaptureVictim(ref move, victim);
                    Move.SetIsQuiet(ref move, victim == Piece.None);
                    Moves[MoveIndex] = move;
                    MoveIndex++;
                    Bitwise.ClearBit(ref knightDestinations, toSquare);
                }
                Bitwise.ClearBit(ref knights, fromSquare);
            }
        }


        private void GenerateBishopMoves(MoveGeneration MoveGeneration, ulong FromSquareMask, ulong ToSquareMask)
        {
            ulong bishops;
            ulong unOrEnemyOccupiedSquares;
            ulong enemyOccupiedSquares;
            int attacker;
            if (WhiteMove)
            {
                // White move
                bishops = WhiteBishops & FromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyWhite;
                enemyOccupiedSquares = OccupancyBlack;
                attacker = Piece.WhiteBishop;
            }
            else
            {
                // Black move
                bishops = BlackBishops & FromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyBlack;
                enemyOccupiedSquares = OccupancyWhite;
                attacker = Piece.BlackBishop;
            }
            int fromSquare;
            while ((fromSquare = Bitwise.FindFirstSetBit(bishops)) != Square.Illegal)
            {
                ulong occupancy = _board.BishopMoveMasks[fromSquare] & Occupancy;
                ulong bishopDestinations;
                switch (MoveGeneration)
                {
                    case MoveGeneration.AllMoves:
                        bishopDestinations = _board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, occupancy) & unOrEnemyOccupiedSquares & ToSquareMask;
                        break;
                    case MoveGeneration.OnlyCaptures:
                        bishopDestinations = _board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, occupancy) & enemyOccupiedSquares & ToSquareMask;
                        break;
                    case MoveGeneration.OnlyNonCaptures:
                        bishopDestinations = _board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, occupancy) & ~Occupancy & ToSquareMask;
                        break;
                    default:
                        throw new Exception($"{MoveGeneration} move generation not supported.");
                }
                int toSquare;
                while ((toSquare = Bitwise.FindFirstSetBit(bishopDestinations)) != Square.Illegal)
                {
                    int victim = GetPiece(toSquare);
                    ulong move = Move.Null;
                    Move.SetFrom(ref move, fromSquare);
                    Move.SetTo(ref move, toSquare);
                    if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                    Move.SetCaptureVictim(ref move, victim);
                    Move.SetIsQuiet(ref move, victim == Piece.None);
                    Moves[MoveIndex] = move;
                    MoveIndex++;
                    Bitwise.ClearBit(ref bishopDestinations, toSquare);
                }
                Bitwise.ClearBit(ref bishops, fromSquare);
            }
        }


        private void GenerateRookMoves(MoveGeneration MoveGeneration, ulong FromSquareMask, ulong ToSquareMask)
        {
            ulong rooks;
            ulong unOrEnemyOccupiedSquares;
            ulong enemyOccupiedSquares;
            int attacker;
            if (WhiteMove)
            {
                // White move
                rooks = WhiteRooks & FromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyWhite;
                enemyOccupiedSquares = OccupancyBlack;
                attacker = Piece.WhiteRook;
            }
            else
            {
                // Black move
                rooks = BlackRooks & FromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyBlack;
                enemyOccupiedSquares = OccupancyWhite;
                attacker = Piece.BlackRook;
            }
            int fromSquare;
            while ((fromSquare = Bitwise.FindFirstSetBit(rooks)) != Square.Illegal)
            {
                ulong occupancy = _board.RookMoveMasks[fromSquare] & Occupancy;
                ulong rookDestinations;
                switch (MoveGeneration)
                {
                    case MoveGeneration.AllMoves:
                        rookDestinations = _board.PrecalculatedMoves.GetRookMovesMask(fromSquare, occupancy) & unOrEnemyOccupiedSquares & ToSquareMask;
                        break;
                    case MoveGeneration.OnlyCaptures:
                        rookDestinations = _board.PrecalculatedMoves.GetRookMovesMask(fromSquare, occupancy) & enemyOccupiedSquares & ToSquareMask;
                        break;
                    case MoveGeneration.OnlyNonCaptures:
                        rookDestinations = _board.PrecalculatedMoves.GetRookMovesMask(fromSquare, occupancy) & ~Occupancy & ToSquareMask;
                        break;
                    default:
                        throw new Exception($"{MoveGeneration} move generation not supported.");
                }
                int toSquare;
                while ((toSquare = Bitwise.FindFirstSetBit(rookDestinations)) != Square.Illegal)
                {
                    int victim = GetPiece(toSquare);
                    ulong move = Move.Null;
                    Move.SetFrom(ref move, fromSquare);
                    Move.SetTo(ref move, toSquare);
                    if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                    Move.SetCaptureVictim(ref move, victim);
                    Move.SetIsQuiet(ref move, victim == Piece.None);
                    Moves[MoveIndex] = move;
                    MoveIndex++;
                    Bitwise.ClearBit(ref rookDestinations, toSquare);
                }
                Bitwise.ClearBit(ref rooks, fromSquare);
            }
        }


        private void GenerateQueenMoves(MoveGeneration MoveGeneration, ulong FromSquareMask, ulong ToSquareMask)
        {
            ulong queens;
            ulong unOrEnemyOccupiedSquares;
            ulong enemyOccupiedSquares;
            int attacker;
            if (WhiteMove)
            {
                // White move
                queens = WhiteQueens & FromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyWhite;
                enemyOccupiedSquares = OccupancyBlack;
                attacker = Piece.WhiteQueen;
            }
            else
            {
                // Black move
                queens = BlackQueens & FromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyBlack;
                enemyOccupiedSquares = OccupancyWhite;
                attacker = Piece.BlackQueen;
            }
            int fromSquare;
            while ((fromSquare = Bitwise.FindFirstSetBit(queens)) != Square.Illegal)
            {
                ulong bishopOccupancy = _board.BishopMoveMasks[fromSquare] & Occupancy;
                ulong rookOccupancy = _board.RookMoveMasks[fromSquare] & Occupancy;
                ulong queenDestinations;
                switch (MoveGeneration)
                {
                    case MoveGeneration.AllMoves:
                        queenDestinations = (_board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, bishopOccupancy) | _board.PrecalculatedMoves.GetRookMovesMask(fromSquare, rookOccupancy)) & unOrEnemyOccupiedSquares & ToSquareMask; 
                        break;
                    case MoveGeneration.OnlyCaptures:
                        queenDestinations = (_board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, bishopOccupancy) | _board.PrecalculatedMoves.GetRookMovesMask(fromSquare, rookOccupancy)) & enemyOccupiedSquares & ToSquareMask;
                        break;
                    case MoveGeneration.OnlyNonCaptures:
                        queenDestinations = (_board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, bishopOccupancy) | _board.PrecalculatedMoves.GetRookMovesMask(fromSquare, rookOccupancy)) & ~Occupancy & ToSquareMask;
                        break;
                    default:
                        throw new Exception($"{MoveGeneration} move generation not supported.");
                }
                int toSquare;
                while ((toSquare = Bitwise.FindFirstSetBit(queenDestinations)) != Square.Illegal)
                {
                    int victim = GetPiece(toSquare);
                    ulong move = Move.Null;
                    Move.SetFrom(ref move, fromSquare);
                    Move.SetTo(ref move, toSquare);
                    if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                    Move.SetCaptureVictim(ref move, victim);
                    Move.SetIsQuiet(ref move, victim == Piece.None);
                    Moves[MoveIndex] = move;
                    MoveIndex++;
                    Bitwise.ClearBit(ref queenDestinations, toSquare);
                }
                Bitwise.ClearBit(ref queens, fromSquare);
            }
        }


        private void GenerateKingMoves(MoveGeneration MoveGeneration, ulong FromSquareMask, ulong ToSquareMask)
        {
            ulong king;
            ulong unOrEnemyOccupiedSquares;
            ulong enemyOccupiedSquares;
            int attacker;
            bool castleQueenside;
            ulong castleQueensideMask;
            bool castleKingside;
            ulong castleKingsideMask;
            if (WhiteMove)
            {
                // White move
                king = WhiteKing & FromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyWhite;
                enemyOccupiedSquares = OccupancyBlack;
                attacker = Piece.WhiteKing;
                castleQueenside = Engine.Castling.WhiteQueenside(Castling);
                castleQueensideMask = Board.WhiteCastleQEmptySquaresMask;
                castleKingside = Engine.Castling.WhiteKingside(Castling);
                castleKingsideMask = Board.WhiteCastleKEmptySquaresMask;
            }
            else
            {
                // Black move
                king = BlackKing & FromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyBlack;
                enemyOccupiedSquares = OccupancyWhite;
                attacker = Piece.BlackKing;
                castleQueenside = Engine.Castling.BlackQueenside(Castling);
                castleQueensideMask = Board.BlackCastleQEmptySquaresMask;
                castleKingside = Engine.Castling.BlackKingside(Castling);
                castleKingsideMask = Board.BlackCastleKEmptySquaresMask;
            }
            ulong move;
            int fromSquare = Bitwise.FindFirstSetBit(king);
            ulong kingDestinations;
            switch (MoveGeneration)
            {
                case MoveGeneration.AllMoves:
                    kingDestinations = _board.KingMoveMasks[fromSquare] & unOrEnemyOccupiedSquares & ToSquareMask;
                    break;
                case MoveGeneration.OnlyCaptures:
                    kingDestinations = _board.KingMoveMasks[fromSquare] & enemyOccupiedSquares & ToSquareMask;
                    break;
                case MoveGeneration.OnlyNonCaptures:
                    kingDestinations = _board.KingMoveMasks[fromSquare] & ~Occupancy & ToSquareMask;
                    break;
                default:
                    throw new Exception($"{MoveGeneration} move generation not supported.");
            }
            int toSquare;
            while ((toSquare = Bitwise.FindFirstSetBit(kingDestinations)) != Square.Illegal)
            {
                int victim = GetPiece(toSquare);
                move = Move.Null;
                Move.SetFrom(ref move, fromSquare);
                Move.SetTo(ref move, toSquare);
                if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                Move.SetCaptureVictim(ref move, victim);
                Move.SetIsKingMove(ref move, true);
                Move.SetIsQuiet(ref move, victim == Piece.None);
                Moves[MoveIndex] = move;
                MoveIndex++;
                Bitwise.ClearBit(ref kingDestinations, toSquare);
            }
            if (MoveGeneration != MoveGeneration.OnlyCaptures)
            {
                if (castleQueenside && ((Occupancy & castleQueensideMask) == 0))
                {
                    // Castle queenside
                    if (WhiteMove)
                    {
                        // White move
                        fromSquare = Square.e1;
                        toSquare = Square.c1;
                    }
                    else
                    {
                        // Black move
                        fromSquare = Square.e8;
                        toSquare = Square.c8;
                    }
                    if ((Board.SquareMasks[toSquare] & ToSquareMask) > 0)
                    {
                        move = Move.Null;
                        Move.SetFrom(ref move, fromSquare);
                        Move.SetTo(ref move, toSquare);
                        Move.SetIsCastling(ref move, true);
                        Move.SetIsKingMove(ref move, true);
                        Move.SetIsQuiet(ref move, false);
                        Moves[MoveIndex] = move;
                        MoveIndex++;
                    }
                }
                if (castleKingside && ((Occupancy & castleKingsideMask) == 0))
                {
                    // Castle kingside
                    if (WhiteMove)
                    {
                        // White move
                        fromSquare = Square.e1;
                        toSquare = Square.g1;
                    }
                    else
                    {
                        // Black move
                        fromSquare = Square.e8;
                        toSquare = Square.g8;
                    }
                    if ((Board.SquareMasks[toSquare] & ToSquareMask) > 0)
                    {
                        move = Move.Null;
                        Move.SetFrom(ref move, fromSquare);
                        Move.SetTo(ref move, toSquare);
                        Move.SetIsCastling(ref move, true);
                        Move.SetIsKingMove(ref move, true);
                        Move.SetIsQuiet(ref move, false);
                        Moves[MoveIndex] = move;
                        MoveIndex++;
                    }
                }
            }
        }


        public void FindPotentiallyPinnedPieces()
        {
            int kingSquare;
            ulong pieces;
            ulong enemyBishopsQueens;
            ulong enemyRooksQueens;
            if (WhiteMove)
            {
                // White move
                kingSquare = Bitwise.FindFirstSetBit(WhiteKing);
                pieces = OccupancyWhite;
                enemyBishopsQueens = BlackBishops | BlackQueens;
                enemyRooksQueens = BlackRooks | BlackQueens;
            }
            else
            {
                // Black move
                kingSquare = Bitwise.FindFirstSetBit(BlackKing);
                pieces = OccupancyBlack;
                enemyBishopsQueens = WhiteBishops | WhiteQueens;
                enemyRooksQueens = WhiteRooks | WhiteQueens;
            }
            PotentiallyPinnedPieces = 0;
            ulong fileAttackers = Board.FileMasks[Board.Files[kingSquare]] & enemyRooksQueens;
            if (fileAttackers > 0) PotentiallyPinnedPieces |= Board.FileMasks[Board.Files[kingSquare]] & pieces;
            ulong rankAttackers = Board.RankMasks[Board.WhiteRanks[kingSquare]] & enemyRooksQueens;
            if (rankAttackers > 0) PotentiallyPinnedPieces |= Board.RankMasks[Board.WhiteRanks[kingSquare]] & pieces;
            ulong upDiagonalAttackers = Board.UpDiagonalMasks[Board.UpDiagonals[kingSquare]] & enemyBishopsQueens;
            if (upDiagonalAttackers > 0) PotentiallyPinnedPieces |= Board.UpDiagonalMasks[Board.UpDiagonals[kingSquare]] & pieces;
            ulong downDiagonalAttackers = Board.DownDiagonalMasks[Board.DownDiagonals[kingSquare]] & enemyBishopsQueens;
            if (downDiagonalAttackers > 0) PotentiallyPinnedPieces |= Board.DownDiagonalMasks[Board.DownDiagonals[kingSquare]] & pieces;
        }


        public void Reset()
        {
            WhitePawns = 0;
            WhiteKnights = 0;
            WhiteBishops = 0;
            WhiteRooks = 0;
            WhiteQueens = 0;
            WhiteKing = 0;
            OccupancyWhite = 0;
            BlackPawns = 0;
            BlackKnights = 0;
            BlackBishops = 0;
            BlackRooks = 0;
            BlackQueens = 0;
            BlackKing = 0;
            OccupancyBlack = 0;
            Occupancy = 0;
            PotentiallyPinnedPieces = 0;
            WhiteMove = true;
            Castling = 0;
            EnPassantSquare = Square.Illegal;
            HalfMoveNumber = 0;
            FullMoveNumber = 0;
            KingInCheck = false;
            CurrentMoveIndex = 0;
            MoveIndex = 0;
            MoveGenerationStage = MoveGenerationStage.BestMove;
            PiecesSquaresKey = 0;
            Key = 0;
            PlayedMove = Move.Null;
        }


        public string ToFen()
        {
            StringBuilder stringBuilder = new StringBuilder();
            // Position
            int x = 0;
            int unoccupiedSquares = 0;
            for (int square = 0; square < 64; square++)
            {
                x++;
                if (x == 9)
                {
                    stringBuilder.Append("/");
                    x = 1;
                }
                int piece = GetPiece(square);
                if (piece == Piece.None)
                {
                    // Unoccupied square
                    unoccupiedSquares++;
                    if (x == 8)
                    {
                        // Last file
                        // Display count of unoccupied squares.
                        stringBuilder.Append(unoccupiedSquares);
                        unoccupiedSquares = 0;
                    }
                }
                else
                {
                    // Occupied square
                    if (unoccupiedSquares > 0)
                    {
                        // Display count of unoccupied squares.
                        stringBuilder.Append(unoccupiedSquares);
                        unoccupiedSquares = 0;
                    }
                    stringBuilder.Append(Piece.GetChar(piece));
                }
            }
            // Display side to move, castling rights, en passant square, half and full move numbers.
            stringBuilder.Append(" ");
            stringBuilder.Append(WhiteMove ? "w" : "b");
            stringBuilder.Append(" ");
            stringBuilder.Append(Engine.Castling.ToString(Castling));
            stringBuilder.Append(" ");
            stringBuilder.Append(EnPassantSquare == Square.Illegal ? "-" : Board.SquareLocations[EnPassantSquare]);
            stringBuilder.Append(" ");
            stringBuilder.Append(HalfMoveNumber);
            stringBuilder.Append(" ");
            stringBuilder.Append(FullMoveNumber);
            return stringBuilder.ToString();
        }


        public override string ToString()
        {
            // Iterate over the piece array to construct an  8 x 8 text display of the chessboard.
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("  ----+---+---+---+---+---+---+---+");
            int square = 0;
            for (int y = 8; y > 0; y--)
            {
                // Add rank.
                stringBuilder.Append(y);
                stringBuilder.Append(" ");
                for (int x = 1; x <= 8; x++)
                {
                    stringBuilder.Append("| ");
                    int piece = GetPiece(square);
                    if (piece == Piece.None) stringBuilder.Append(" ");
                    else stringBuilder.Append(Piece.GetChar(piece));
                    stringBuilder.Append(" ");
                    square++;
                }
                stringBuilder.AppendLine("|");
                stringBuilder.AppendLine("  ----+---+---+---+---+---+---+---+");
            }
            stringBuilder.Append("  ");
            for (int x = 1; x <= 8; x++)
            {
                // Add file.
                char chrFile = (char)(x + 96);
                stringBuilder.Append($"  {chrFile} ");
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            // Display FEN, key, position count, and check.
            stringBuilder.AppendLine($"FEN:             {ToFen()}");
            stringBuilder.AppendLine($"Key:             {Key:X16}");
            stringBuilder.AppendLine($"Position Count:  {_board.GetPositionCount()}");
            stringBuilder.AppendLine($"King in Check:   {(KingInCheck ? "Yes" : "No")}");
            return stringBuilder.ToString();
        }
    }
}