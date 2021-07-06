// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;
using System.Text;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class Position
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
        public ulong PinnedPieces;
        public bool WhiteMove;
        public uint Castling;
        public int EnPassantSquare;
        public int PlySinceCaptureOrPawnMove;
        public int FullMoveNumber;
        public bool KingInCheck;
        public int CurrentMoveIndex;
        public int MoveIndex;
        public MoveGenerationStage MoveGenerationStage;
        public ulong PiecesSquaresKey;
        public ulong Key;
        public int StaticScore;
        public ulong PlayedMove;
        private readonly Board _board;


        public Position(Board board)
        {
            _board = board;
            Moves = new ulong[MaxMoves];
            Reset();
        }


        public int GetPiece(int square)
        {
            var squareMask = Board.SquareMasks[square];
            if ((Occupancy & squareMask) == 0) return Piece.None;
            if ((OccupancyWhite & squareMask) > 0)
            {
                // Locate white piece.
                if ((WhitePawns & squareMask) > 0) return Piece.WhitePawn;
                if ((WhiteKnights & squareMask) > 0) return Piece.WhiteKnight;
                if ((WhiteBishops & squareMask) > 0) return Piece.WhiteBishop;
                if ((WhiteRooks & squareMask) > 0) return Piece.WhiteRook;
                if ((WhiteQueens & squareMask) > 0) return Piece.WhiteQueen;
                if ((WhiteKing & squareMask) > 0) return Piece.WhiteKing;
                throw new Exception($"White piece not found at {Board.SquareLocations[square]}.");
            }
            // Locate black piece.
            if ((BlackPawns & squareMask) > 0) return Piece.BlackPawn;
            if ((BlackKnights & squareMask) > 0) return Piece.BlackKnight;
            if ((BlackBishops & squareMask) > 0) return Piece.BlackBishop;
            if ((BlackRooks & squareMask) > 0) return Piece.BlackRook;
            if ((BlackQueens & squareMask) > 0) return Piece.BlackQueen;
            if ((BlackKing & squareMask) > 0) return Piece.BlackKing;
            throw new Exception($"Black piece not found at {square}.");
        }


        public void Set(Position copyFromPosition)
        {
            // Copy bitboards.
            WhitePawns = copyFromPosition.WhitePawns;
            WhiteKnights = copyFromPosition.WhiteKnights;
            WhiteBishops = copyFromPosition.WhiteBishops;
            WhiteRooks = copyFromPosition.WhiteRooks;
            WhiteQueens = copyFromPosition.WhiteQueens;
            WhiteKing = copyFromPosition.WhiteKing;
            OccupancyWhite = copyFromPosition.OccupancyWhite;
            BlackPawns = copyFromPosition.BlackPawns;
            BlackKnights = copyFromPosition.BlackKnights;
            BlackBishops = copyFromPosition.BlackBishops;
            BlackRooks = copyFromPosition.BlackRooks;
            BlackQueens = copyFromPosition.BlackQueens;
            BlackKing = copyFromPosition.BlackKing;
            OccupancyBlack = copyFromPosition.OccupancyBlack;
            Occupancy = copyFromPosition.Occupancy;
            // Copy board state.  Do not copy values that will be set after moves are generated or played.
            WhiteMove = copyFromPosition.WhiteMove;
            Castling = copyFromPosition.Castling;
            EnPassantSquare = copyFromPosition.EnPassantSquare;
            PlySinceCaptureOrPawnMove = copyFromPosition.PlySinceCaptureOrPawnMove;
            FullMoveNumber = copyFromPosition.FullMoveNumber;
            PiecesSquaresKey = copyFromPosition.PiecesSquaresKey;
        }


        public void GenerateMoves()
        {
            PrepareMoveGeneration();
            FindPinnedPieces();
            GenerateMoves(MoveGeneration.AllMoves, Board.AllSquaresMask, Board.AllSquaresMask);
        }


        public void GenerateMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
        {
            GeneratePawnMoves(moveGeneration, fromSquareMask, toSquareMask);
            GenerateKnightMoves(moveGeneration, fromSquareMask, toSquareMask);
            GenerateBishopMoves(moveGeneration, fromSquareMask, toSquareMask);
            GenerateRookMoves(moveGeneration, fromSquareMask, toSquareMask);
            GenerateQueenMoves(moveGeneration, fromSquareMask, toSquareMask);
            GenerateKingMoves(moveGeneration, fromSquareMask, toSquareMask);
        }


        public void PrepareMoveGeneration()
        {
            CurrentMoveIndex = 0;
            MoveIndex = 0;
            MoveGenerationStage = MoveGenerationStage.BestMove;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GeneratePawnMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
        {
            ulong pawns;
            ulong[] pawnMoveMasks;
            ulong[] pawnDoubleMoveMasks;
            ulong[] pawnAttackMasks;
            ulong enemyOccupiedSquares;
            var unoccupiedSquares = ~Occupancy;
            int[] ranks;
            int attacker;
            int queen;
            int rook;
            int bishop;
            int knight;
            int enPassantVictim;
            if (WhiteMove)
            {
                // White Move
                pawns = WhitePawns & fromSquareMask;
                pawnMoveMasks = Board.WhitePawnMoveMasks;
                pawnDoubleMoveMasks = Board.WhitePawnDoubleMoveMasks;
                pawnAttackMasks = Board.WhitePawnAttackMasks;
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
                // Black Move
                pawns = BlackPawns & fromSquareMask;
                pawnMoveMasks = Board.BlackPawnMoveMasks;
                pawnDoubleMoveMasks = Board.BlackPawnDoubleMoveMasks;
                pawnAttackMasks = Board.BlackPawnAttackMasks;
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
            if ((EnPassantSquare != Square.Illegal) && ((Board.SquareMasks[EnPassantSquare] & toSquareMask) > 0) && (moveGeneration != MoveGeneration.OnlyNonCaptures))
            {
                var enPassantAttackers = Board.EnPassantAttackerMasks[EnPassantSquare] & pawns;
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
                if (moveGeneration != MoveGeneration.OnlyCaptures)
                {
                    // Pawns may move forward one square (or two if on initial square) if forward squares are unoccupied.
                    pawnDestinations = pawnMoveMasks[fromSquare] & unoccupiedSquares & toSquareMask;
                    while ((toSquare = Bitwise.FindFirstSetBit(pawnDestinations)) != Square.Illegal)
                    {
                        var doubleMove = Board.SquareDistances[fromSquare][toSquare] == 2;
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
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                            // Promote pawn to rook.
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, rook);
                            Move.SetIsPawnMove(ref move, true);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                            // Promote pawn to bishop.
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, bishop);
                            Move.SetIsPawnMove(ref move, true);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                            // Promote pawn to knight.
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, knight);
                            Move.SetIsPawnMove(ref move, true);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                        }
                        Bitwise.ClearBit(ref pawnDestinations, toSquare);
                    }
                }
                if (moveGeneration != MoveGeneration.OnlyNonCaptures)
                {
                    // Pawns may attack diagonally forward one square if occupied by enemy.
                    pawnDestinations = pawnAttackMasks[fromSquare] & enemyOccupiedSquares & toSquareMask;
                    while ((toSquare = Bitwise.FindFirstSetBit(pawnDestinations)) != Square.Illegal)
                    {
                        toSquareRank = ranks[toSquare];
                        var victim = GetPiece(toSquare);
                        if (toSquareRank < 7)
                        {
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetCaptureAttacker(ref move, attacker);
                            Move.SetCaptureVictim(ref move, victim);
                            Move.SetIsPawnMove(ref move, true);
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
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                        }
                        Bitwise.ClearBit(ref pawnDestinations, toSquare);
                    }
                }
                Bitwise.ClearBit(ref pawns, fromSquare);
            }
        }


        // TODO: Refactor move generation into sliders and non-sliders using a delegate to get move masks.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GenerateKnightMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
        {
            ulong knights;
            ulong unOrEnemyOccupiedSquares;
            ulong enemyOccupiedSquares;
            int attacker;
            if (WhiteMove)
            {
                // White Move
                knights = WhiteKnights & fromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyWhite;
                enemyOccupiedSquares = OccupancyBlack;
                attacker = Piece.WhiteKnight;
            }
            else
            {
                // Black Move
                knights = BlackKnights & fromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyBlack;
                enemyOccupiedSquares = OccupancyWhite;
                attacker = Piece.BlackKnight;
            }
            int fromSquare;
            while ((fromSquare = Bitwise.FindFirstSetBit(knights)) != Square.Illegal)
            {
                var knightDestinations = moveGeneration switch
                {
                    MoveGeneration.AllMoves => Board.KnightMoveMasks[fromSquare] & unOrEnemyOccupiedSquares & toSquareMask,
                    MoveGeneration.OnlyCaptures => Board.KnightMoveMasks[fromSquare] & enemyOccupiedSquares & toSquareMask,
                    MoveGeneration.OnlyNonCaptures => Board.KnightMoveMasks[fromSquare] & ~Occupancy & toSquareMask,
                    _ => throw new Exception($"{moveGeneration} move generation not supported.")
                };
                int toSquare;
                while ((toSquare = Bitwise.FindFirstSetBit(knightDestinations)) != Square.Illegal)
                {
                    var victim = GetPiece(toSquare);
                    var move = Move.Null;
                    Move.SetFrom(ref move, fromSquare);
                    Move.SetTo(ref move, toSquare);
                    if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                    Move.SetCaptureVictim(ref move, victim);
                    Moves[MoveIndex] = move;
                    MoveIndex++;
                    Bitwise.ClearBit(ref knightDestinations, toSquare);
                }
                Bitwise.ClearBit(ref knights, fromSquare);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GenerateBishopMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
        {
            ulong bishops;
            ulong unOrEnemyOccupiedSquares;
            ulong enemyOccupiedSquares;
            int attacker;
            if (WhiteMove)
            {
                // White Move
                bishops = WhiteBishops & fromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyWhite;
                enemyOccupiedSquares = OccupancyBlack;
                attacker = Piece.WhiteBishop;
            }
            else
            {
                // Black Move
                bishops = BlackBishops & fromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyBlack;
                enemyOccupiedSquares = OccupancyWhite;
                attacker = Piece.BlackBishop;
            }
            int fromSquare;
            while ((fromSquare = Bitwise.FindFirstSetBit(bishops)) != Square.Illegal)
            {
                var occupancy = Board.BishopMoveMasks[fromSquare] & Occupancy;
                var bishopDestinations = moveGeneration switch
                {
                    MoveGeneration.AllMoves => Board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, occupancy) & unOrEnemyOccupiedSquares & toSquareMask,
                    MoveGeneration.OnlyCaptures => Board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, occupancy) & enemyOccupiedSquares & toSquareMask,
                    MoveGeneration.OnlyNonCaptures => Board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, occupancy) & ~Occupancy & toSquareMask,
                    _ => throw new Exception($"{moveGeneration} move generation not supported.")
                };
                int toSquare;
                while ((toSquare = Bitwise.FindFirstSetBit(bishopDestinations)) != Square.Illegal)
                {
                    var victim = GetPiece(toSquare);
                    var move = Move.Null;
                    Move.SetFrom(ref move, fromSquare);
                    Move.SetTo(ref move, toSquare);
                    if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                    Move.SetCaptureVictim(ref move, victim);
                    Moves[MoveIndex] = move;
                    MoveIndex++;
                    Bitwise.ClearBit(ref bishopDestinations, toSquare);
                }
                Bitwise.ClearBit(ref bishops, fromSquare);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GenerateRookMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
        {
            ulong rooks;
            ulong unOrEnemyOccupiedSquares;
            ulong enemyOccupiedSquares;
            int attacker;
            if (WhiteMove)
            {
                // White Move
                rooks = WhiteRooks & fromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyWhite;
                enemyOccupiedSquares = OccupancyBlack;
                attacker = Piece.WhiteRook;
            }
            else
            {
                // Black Move
                rooks = BlackRooks & fromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyBlack;
                enemyOccupiedSquares = OccupancyWhite;
                attacker = Piece.BlackRook;
            }
            int fromSquare;
            while ((fromSquare = Bitwise.FindFirstSetBit(rooks)) != Square.Illegal)
            {
                var occupancy = Board.RookMoveMasks[fromSquare] & Occupancy;
                var rookDestinations = moveGeneration switch
                {
                    MoveGeneration.AllMoves => Board.PrecalculatedMoves.GetRookMovesMask(fromSquare, occupancy) & unOrEnemyOccupiedSquares & toSquareMask,
                    MoveGeneration.OnlyCaptures => Board.PrecalculatedMoves.GetRookMovesMask(fromSquare, occupancy) & enemyOccupiedSquares & toSquareMask,
                    MoveGeneration.OnlyNonCaptures => Board.PrecalculatedMoves.GetRookMovesMask(fromSquare, occupancy) & ~Occupancy & toSquareMask,
                    _ => throw new Exception($"{moveGeneration} move generation not supported.")
                };
                int toSquare;
                while ((toSquare = Bitwise.FindFirstSetBit(rookDestinations)) != Square.Illegal)
                {
                    var victim = GetPiece(toSquare);
                    var move = Move.Null;
                    Move.SetFrom(ref move, fromSquare);
                    Move.SetTo(ref move, toSquare);
                    if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                    Move.SetCaptureVictim(ref move, victim);
                    Moves[MoveIndex] = move;
                    MoveIndex++;
                    Bitwise.ClearBit(ref rookDestinations, toSquare);
                }
                Bitwise.ClearBit(ref rooks, fromSquare);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GenerateQueenMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
        {
            ulong queens;
            ulong unOrEnemyOccupiedSquares;
            ulong enemyOccupiedSquares;
            int attacker;
            if (WhiteMove)
            {
                // White Move
                queens = WhiteQueens & fromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyWhite;
                enemyOccupiedSquares = OccupancyBlack;
                attacker = Piece.WhiteQueen;
            }
            else
            {
                // Black Move
                queens = BlackQueens & fromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyBlack;
                enemyOccupiedSquares = OccupancyWhite;
                attacker = Piece.BlackQueen;
            }
            int fromSquare;
            while ((fromSquare = Bitwise.FindFirstSetBit(queens)) != Square.Illegal)
            {
                var bishopOccupancy = Board.BishopMoveMasks[fromSquare] & Occupancy;
                var rookOccupancy = Board.RookMoveMasks[fromSquare] & Occupancy;
                var queenDestinations = moveGeneration switch
                {
                    MoveGeneration.AllMoves => (Board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, bishopOccupancy) | Board.PrecalculatedMoves.GetRookMovesMask(fromSquare, rookOccupancy)) & unOrEnemyOccupiedSquares & toSquareMask,
                    MoveGeneration.OnlyCaptures => (Board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, bishopOccupancy) | Board.PrecalculatedMoves.GetRookMovesMask(fromSquare, rookOccupancy)) & enemyOccupiedSquares & toSquareMask,
                    MoveGeneration.OnlyNonCaptures => (Board.PrecalculatedMoves.GetBishopMovesMask(fromSquare, bishopOccupancy) | Board.PrecalculatedMoves.GetRookMovesMask(fromSquare, rookOccupancy)) & ~Occupancy & toSquareMask,
                    _ => throw new Exception($"{moveGeneration} move generation not supported.")
                };
                int toSquare;
                while ((toSquare = Bitwise.FindFirstSetBit(queenDestinations)) != Square.Illegal)
                {
                    var victim = GetPiece(toSquare);
                    var move = Move.Null;
                    Move.SetFrom(ref move, fromSquare);
                    Move.SetTo(ref move, toSquare);
                    if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                    Move.SetCaptureVictim(ref move, victim);
                    Moves[MoveIndex] = move;
                    MoveIndex++;
                    Bitwise.ClearBit(ref queenDestinations, toSquare);
                }
                Bitwise.ClearBit(ref queens, fromSquare);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GenerateKingMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
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
                // White Move
                king = WhiteKing & fromSquareMask;
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
                // Black Move
                king = BlackKing & fromSquareMask;
                unOrEnemyOccupiedSquares = ~OccupancyBlack;
                enemyOccupiedSquares = OccupancyWhite;
                attacker = Piece.BlackKing;
                castleQueenside = Engine.Castling.BlackQueenside(Castling);
                castleQueensideMask = Board.BlackCastleQEmptySquaresMask;
                castleKingside = Engine.Castling.BlackKingside(Castling);
                castleKingsideMask = Board.BlackCastleKEmptySquaresMask;
            }
            ulong move;
            var fromSquare = Bitwise.FindFirstSetBit(king);
            if (fromSquare == Square.Illegal) return;
            var kingDestinations = moveGeneration switch
            {
                MoveGeneration.AllMoves => Board.KingMoveMasks[fromSquare] & unOrEnemyOccupiedSquares & toSquareMask,
                MoveGeneration.OnlyCaptures => Board.KingMoveMasks[fromSquare] & enemyOccupiedSquares & toSquareMask,
                MoveGeneration.OnlyNonCaptures => Board.KingMoveMasks[fromSquare] & ~Occupancy & toSquareMask,
                _ => throw new Exception($"{moveGeneration} move generation not supported.")
            };
            int toSquare;
            while ((toSquare = Bitwise.FindFirstSetBit(kingDestinations)) != Square.Illegal)
            {
                var victim = GetPiece(toSquare);
                move = Move.Null;
                Move.SetFrom(ref move, fromSquare);
                Move.SetTo(ref move, toSquare);
                if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                Move.SetCaptureVictim(ref move, victim);
                Move.SetIsKingMove(ref move, true);
                Moves[MoveIndex] = move;
                MoveIndex++;
                Bitwise.ClearBit(ref kingDestinations, toSquare);
            }
            if (moveGeneration != MoveGeneration.OnlyCaptures)
            {
                if (castleQueenside && ((Occupancy & castleQueensideMask) == 0))
                {
                    // Castle Queenside
                    if (WhiteMove)
                    {
                        // White Move
                        fromSquare = Square.e1;
                        toSquare = Square.c1;
                    }
                    else
                    {
                        // Black Move
                        fromSquare = Square.e8;
                        toSquare = Square.c8;
                    }
                    if ((Board.SquareMasks[toSquare] & toSquareMask) > 0)
                    {
                        move = Move.Null;
                        Move.SetFrom(ref move, fromSquare);
                        Move.SetTo(ref move, toSquare);
                        Move.SetIsCastling(ref move, true);
                        Move.SetIsKingMove(ref move, true);
                        Moves[MoveIndex] = move;
                        MoveIndex++;
                    }
                }
                if (castleKingside && ((Occupancy & castleKingsideMask) == 0))
                {
                    // Castle Kingside
                    if (WhiteMove)
                    {
                        // White Move
                        fromSquare = Square.e1;
                        toSquare = Square.g1;
                    }
                    else
                    {
                        // Black Move
                        fromSquare = Square.e8;
                        toSquare = Square.g8;
                    }
                    if ((Board.SquareMasks[toSquare] & toSquareMask) > 0)
                    {
                        move = Move.Null;
                        Move.SetFrom(ref move, fromSquare);
                        Move.SetTo(ref move, toSquare);
                        Move.SetIsCastling(ref move, true);
                        Move.SetIsKingMove(ref move, true);
                        Moves[MoveIndex] = move;
                        MoveIndex++;
                    }
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void FindPinnedPieces()
        {
            int ownKingSquare;
            ulong ownPieces;
            ulong enemyRankFileAttackers;
            ulong enemyDiagonalAttackers;
            ulong enemyPieces;
            if (WhiteMove)
            {
                // White Move
                ownKingSquare = Bitwise.FindFirstSetBit(WhiteKing);
                ownPieces = OccupancyWhite;
                enemyRankFileAttackers = BlackRooks | BlackQueens;
                enemyDiagonalAttackers = BlackBishops | BlackQueens;
                enemyPieces = OccupancyBlack;
            }
            else
            {
                // Black Move
                ownKingSquare = Bitwise.FindFirstSetBit(BlackKing);
                ownPieces = OccupancyBlack;
                enemyRankFileAttackers = WhiteRooks | WhiteQueens;
                enemyDiagonalAttackers = WhiteBishops | WhiteQueens;
                enemyPieces = OccupancyWhite;
            }
            // Find pieces pinned to own king by enemy rank / file attackers.
            PinnedPieces = 0;
            int attackerSquare;
            while ((attackerSquare = Bitwise.FindFirstSetBit(enemyRankFileAttackers)) != Square.Illegal)
            {
                var betweenSquares = Board.RankFileBetweenSquares[attackerSquare][ownKingSquare];
                if (betweenSquares == 0)
                {
                    Bitwise.ClearBit(ref enemyRankFileAttackers, attackerSquare);
                    continue;
                }
                if ((betweenSquares & enemyPieces) == 0)
                {
                    // No enemy pieces between enemy attacker and own king.
                    var potentiallyPinnedPieces = betweenSquares & ownPieces;
                    if (Bitwise.CountSetBits(potentiallyPinnedPieces) == 1)
                    {
                        // Exactly one own piece between enemy attacker and own king.
                        // Piece is pinned to own king.
                        PinnedPieces |= potentiallyPinnedPieces;
                    }
                }
                Bitwise.ClearBit(ref enemyRankFileAttackers, attackerSquare);
            }
            // Find pieces pinned to own king by enemy diagonal attackers.
            while ((attackerSquare = Bitwise.FindFirstSetBit(enemyDiagonalAttackers)) != Square.Illegal)
            {
                var betweenSquares = Board.DiagonalBetweenSquares[attackerSquare][ownKingSquare];
                if (betweenSquares == 0)
                {
                    Bitwise.ClearBit(ref enemyDiagonalAttackers, attackerSquare);
                    continue;
                }
                if ((betweenSquares & enemyPieces) == 0)
                {
                    // No enemy pieces between enemy attacker and own king.
                    var potentiallyPinnedPieces = betweenSquares & ownPieces;
                    if (Bitwise.CountSetBits(potentiallyPinnedPieces) == 1)
                    {
                        // Exactly one own piece between enemy attacker and own king.
                        // Piece is pinned to own king.
                        PinnedPieces |= potentiallyPinnedPieces;
                    }
                }
                Bitwise.ClearBit(ref enemyDiagonalAttackers, attackerSquare);
            }
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
            PinnedPieces = 0;
            WhiteMove = true;
            Castling = 0;
            EnPassantSquare = Square.Illegal;
            PlySinceCaptureOrPawnMove = 0;
            FullMoveNumber = 0;
            KingInCheck = false;
            CurrentMoveIndex = 0;
            MoveIndex = 0;
            MoveGenerationStage = MoveGenerationStage.BestMove;
            PiecesSquaresKey = 0;
            Key = 0;
            StaticScore = -Engine.StaticScore.Max;
            PlayedMove = Move.Null;
        }


        public string ToFen()
        {
            var stringBuilder = new StringBuilder();
            // Position
            var x = 0;
            var unoccupiedSquares = 0;
            for (var square = 0; square < 64; square++)
            {
                x++;
                if (x == 9)
                {
                    stringBuilder.Append("/");
                    x = 1;
                }
                var piece = GetPiece(square);
                if (piece == Piece.None)
                {
                    // Unoccupied Square
                    unoccupiedSquares++;
                    if (x == 8)
                    {
                        // Last File
                        // Display count of unoccupied squares.
                        stringBuilder.Append(unoccupiedSquares);
                        unoccupiedSquares = 0;
                    }
                }
                else
                {
                    // Occupied Square
                    if (unoccupiedSquares > 0)
                    {
                        // Display count of unoccupied squares.
                        stringBuilder.Append(unoccupiedSquares);
                        unoccupiedSquares = 0;
                    }
                    stringBuilder.Append(Piece.GetChar(piece));
                }
            }
            // Display side to move, castling rights, en passant square, ply, and full move number.
            stringBuilder.Append(" ");
            stringBuilder.Append(WhiteMove ? "w" : "b");
            stringBuilder.Append(" ");
            stringBuilder.Append(Engine.Castling.ToString(Castling));
            stringBuilder.Append(" ");
            stringBuilder.Append(EnPassantSquare == Square.Illegal ? "-" : Board.SquareLocations[EnPassantSquare]);
            stringBuilder.Append(" ");
            stringBuilder.Append(PlySinceCaptureOrPawnMove);
            stringBuilder.Append(" ");
            stringBuilder.Append(FullMoveNumber);
            return stringBuilder.ToString();
        }


        public static string ToString(ulong occupancy)
        {
            var stringBuilder = new StringBuilder();
            for (var rank = 7; rank >= 0; rank--)
            {
                for (var file = 0; file < 8; file++)
                {
                    var square = Board.GetSquare(file, rank);
                    stringBuilder.Append(Bitwise.IsBitSet(occupancy, square) ? " 1 " : " . ");
                }
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }


        public override string ToString()
        {
            // Iterate over the piece array to construct an 8 x 8 text display of the chessboard.
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("  +---+---+---+---+---+---+---+---+");
            var square = 0;
            for (var y = 8; y > 0; y--)
            {
                // Add rank.
                stringBuilder.Append(y);
                stringBuilder.Append(" ");
                for (var x = 1; x <= 8; x++)
                {
                    stringBuilder.Append("| ");
                    var piece = GetPiece(square);
                    if (piece == Piece.None) stringBuilder.Append(" ");
                    else stringBuilder.Append(Piece.GetChar(piece));
                    stringBuilder.Append(" ");
                    square++;
                }
                stringBuilder.AppendLine("|");
                stringBuilder.AppendLine("  +---+---+---+---+---+---+---+---+");
            }
            stringBuilder.Append("  ");
            for (var x = 1; x <= 8; x++)
            {
                // Add file.
                var chrFile = (char)(x + 96);
                stringBuilder.Append($"  {chrFile} ");
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            // Display FEN, key, position count, and check.
            stringBuilder.AppendLine($"FEN:             {ToFen()}");
            stringBuilder.AppendLine($"Key:             {Key:X16}");
            stringBuilder.AppendLine($"Position Count:  {_board.GetPositionCount()}");
            stringBuilder.AppendLine($"King in Check:   {(KingInCheck ? "Yes" : "No")}");
            stringBuilder.AppendLine($"Static Score:    {StaticScore}");
            stringBuilder.AppendLine($"Played Move:     {Move.ToString(PlayedMove)}");
            return stringBuilder.ToString();
        }
    }
}