// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
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
    public sealed class Evaluation
    {
        public const int PawnMaterial = 100;
        private const double _passedPawnPower = 2d;
        // Select phase constants such that starting material = 256.
        // This improves integer division speed since x / 256 = x >> 8.
        private const int _middlegamePhase = 4 * (_knightPhase + _bishopPhase + _rookPhase) + 2 * _queenPhase;
        private const int _knightPhase = 14; //   4 * 14 =  56
        private const int _bishopPhase = 14; // + 4 * 14 = 112
        private const int _rookPhase = 20; //   + 4 * 20 = 192
        private const int _queenPhase = 32; //  + 2 * 32 = 256
        public readonly EvaluationConfig Config;
        public readonly EvaluationStats Stats;
        private readonly Delegates.GetPositionCount _getPositionCount;
        private readonly Delegates.IsPassedPawn _isPassedPawn;
        private readonly Delegates.IsFreePawn _isFreePawn;
        private readonly StaticScore _staticScore;
        // Piece Location
        private readonly int[] _mgPawnLocations = new int[64];
        private readonly int[] _egPawnLocations = new int[64];
        private readonly int[] _mgKnightLocations = new int[64];
        private readonly int[] _egKnightLocations = new int[64];
        private readonly int[] _mgBishopLocations = new int[64];
        private readonly int[] _egBishopLocations = new int[64];
        private readonly int[] _mgRookLocations = new int[64];
        private readonly int[] _egRookLocations = new int[64];
        private readonly int[] _mgQueenLocations = new int[64];
        private readonly int[] _egQueenLocations = new int[64];
        private readonly int[] _mgKingLocations = new int[64];
        private readonly int[] _egKingLocations = new int[64];
        // Passed Pawns
        private readonly int[] _mgPassedPawns = new int[8];
        private readonly int[] _egPassedPawns = new int[8];
        private readonly int[] _egFreePassedPawns = new int[8];
        public int DrawMoves;
        

        public Evaluation(EvaluationConfig Config, Delegates.GetPositionCount GetPositionCount, Delegates.IsPassedPawn IsPassedPawn, Delegates.IsFreePawn IsFreePawn)
        {
            this.Config = Config;
            _getPositionCount = GetPositionCount;
            _isPassedPawn = IsPassedPawn;
            _isFreePawn = IsFreePawn;
            _staticScore = new StaticScore(_middlegamePhase);
            Stats = new EvaluationStats();
            DrawMoves = 2;
            Configure();
        }


        public void Configure()
        {
            // Calculate piece location values.
            for (int square = 0; square < 64; square++)
            {
                int rank = Board.WhiteRanks[square];
                int file = Board.Files[square];
                int squareCentrality = 3 - Board.GetShortestDistance(square, Board.CentralSquares);
                int fileCentrality = 3 - Math.Min(Math.Abs(3 - file), Math.Abs(4 - file));
                int nearCorner = 3 - Board.GetShortestDistance(square, Board.CornerSquares);
                _mgPawnLocations[square] = rank * Config.MgPawnAdvancement + squareCentrality * Config.MgPawnCentrality;
                _egPawnLocations[square] = rank * Config.EgPawnAdvancement + squareCentrality * Config.EgPawnCentrality + Config.EgPawnConstant;
                _mgKnightLocations[square] = rank * Config.MgKnightAdvancement + squareCentrality * Config.MgKnightCentrality + nearCorner * Config.MgKnightCorner + Config.MgKnightConstant;
                _egKnightLocations[square] = rank * Config.EgKnightAdvancement + squareCentrality * Config.EgKnightCentrality + nearCorner * Config.EgKnightCorner + Config.EgKnightConstant;
                _mgBishopLocations[square] = rank * Config.MgBishopAdvancement + squareCentrality * Config.MgBishopCentrality + nearCorner * Config.MgBishopCorner + Config.MgBishopConstant;
                _egBishopLocations[square] = rank * Config.EgBishopAdvancement + squareCentrality * Config.EgBishopCentrality + nearCorner * Config.EgBishopCorner + Config.EgBishopConstant;
                _mgRookLocations[square] = rank * Config.MgRookAdvancement + fileCentrality * Config.MgRookCentrality + nearCorner * Config.MgRookCorner + Config.MgRookConstant;
                _egRookLocations[square] = rank * Config.EgRookAdvancement + squareCentrality * Config.EgRookCentrality + nearCorner * Config.EgRookCorner + Config.EgRookConstant;
                _mgQueenLocations[square] = rank * Config.MgQueenAdvancement + squareCentrality * Config.MgQueenCentrality + nearCorner * Config.MgQueenCorner + Config.MgQueenConstant;
                _egQueenLocations[square] = rank * Config.EgQueenAdvancement + squareCentrality * Config.EgQueenCentrality + nearCorner * Config.EgQueenCorner + Config.EgQueenConstant;
                _mgKingLocations[square] = rank * Config.MgKingAdvancement + squareCentrality * Config.MgKingCentrality + nearCorner * Config.MgKingCorner;
                _egKingLocations[square] = rank * Config.EgKingAdvancement + squareCentrality * Config.EgKingCentrality + nearCorner * Config.EgKingCorner;
            }
            // Calculate passed pawn values.
            double mgScale = Config.MgPassedPawnScalePercent / 100d;
            double egScale = Config.EgPassedPawnScalePercent / 100d;
            double egFreeScale = Config.EgFreePassedPawnScalePercent / 100d;
            for (int rank = 1; rank < 7; rank++)
            {
                _mgPassedPawns[rank] = GetNonLinearBonus(rank, mgScale, _passedPawnPower, 0);
                _egPassedPawns[rank] = GetNonLinearBonus(rank, egScale, _passedPawnPower, 0);
                _egFreePassedPawns[rank] = GetNonLinearBonus(rank, egFreeScale, _passedPawnPower, 0);
            }
        }


        public static void ConfigureStrength(int Elo)
        {
            // TODO: Configure strength of evaluation.
        }


        public int GetMaterialScore(int Piece)
        {
            // Sequence cases in order of enum integer value to improve performance of switch statement.
            switch (Piece)
            {
                case Engine.Piece.None:
                    return 0;
                case Engine.Piece.WhitePawn:
                    return PawnMaterial;
                case Engine.Piece.WhiteKnight:
                    return Config.KnightMaterial;
                case Engine.Piece.WhiteBishop:
                    return Config.BishopMaterial;
                case Engine.Piece.WhiteRook:
                    return Config.RookMaterial;
                case Engine.Piece.WhiteQueen:
                    return Config.QueenMaterial;
                case Engine.Piece.WhiteKing:
                    return 0;
                case Engine.Piece.BlackPawn:
                    return PawnMaterial;
                case Engine.Piece.BlackKnight:
                    return Config.KnightMaterial;
                case Engine.Piece.BlackBishop:
                    return Config.BishopMaterial;
                case Engine.Piece.BlackRook:
                    return Config.RookMaterial;
                case Engine.Piece.BlackQueen:
                    return Config.QueenMaterial;
                case Engine.Piece.BlackKing:
                    return 0;
                default:
                    throw new ArgumentException($"{Piece} piece not supported.");
            }
        }


        public (bool TerminalDraw, int PositionCount) IsTerminalDraw(Position Position)
        {
            // Only return true if position is drawn and no sequence of moves can make game winnable.
            int positionCount = _getPositionCount();
            if (positionCount >= DrawMoves) return (true, positionCount); // Draw by repetition of position
            if (Position.HalfMoveNumber >= 99) return (true, positionCount); // Draw by fifty moves without a capture or pawn move
            // Determine if insufficient material remains for checkmate.
            if ((Bitwise.CountSetBits(Position.WhitePawns) + Bitwise.CountSetBits(Position.BlackPawns)) == 0)
            {
                // Neither side has any pawns.
                if ((Bitwise.CountSetBits(Position.WhiteRooks) + Bitwise.CountSetBits(Position.WhiteQueens) + Bitwise.CountSetBits(Position.BlackRooks) + Bitwise.CountSetBits(Position.BlackQueens)) == 0)
                    // Neither side has any major pieces.
                    if (((Bitwise.CountSetBits(Position.WhiteKnights) + Bitwise.CountSetBits(Position.WhiteBishops)) <= 1) && ((Bitwise.CountSetBits(Position.BlackKnights) + Bitwise.CountSetBits(Position.BlackBishops)) <= 1))
                        return (true, positionCount); // Each side has one or zero minor pieces.  Draw by insufficient material.
            }
            return (false, positionCount);
        }


        public static bool IsDrawnEndgame(Position Position)
        {
            if ((Bitwise.CountSetBits(Position.WhitePawns) > 0) || (Bitwise.CountSetBits(Position.BlackPawns) > 0)) return false; // At least one pawn on board.
            return IsDrawnEndgame(Position, true) || IsDrawnEndgame(Position, false);
        }


        private static bool IsDrawnEndgame(Position Position, bool WhiteIsSide1)
        {
            // Return true if position is drawn.
            // Do not terminate search based on this method because a sequence of moves could make the game winnable.
            int side1Knights;
            int side1Bishops;
            int side1Rooks;
            int side1Queens;
            int side2Knights;
            int side2Bishops;
            int side2Rooks;
            int side2Queens;
            if (WhiteIsSide1)
            {
                // White is Side 1
                side1Knights = Bitwise.CountSetBits(Position.WhiteKnights);
                side1Bishops = Bitwise.CountSetBits(Position.WhiteBishops);
                side1Rooks = Bitwise.CountSetBits(Position.WhiteRooks);
                side1Queens = Bitwise.CountSetBits(Position.WhiteQueens);
                side2Knights = Bitwise.CountSetBits(Position.BlackKnights);
                side2Bishops = Bitwise.CountSetBits(Position.BlackBishops);
                side2Rooks = Bitwise.CountSetBits(Position.BlackRooks);
                side2Queens = Bitwise.CountSetBits(Position.BlackQueens);
            }
            else
            {
                // Black is Side 1
                side1Knights = Bitwise.CountSetBits(Position.BlackKnights);
                side1Bishops = Bitwise.CountSetBits(Position.BlackBishops);
                side1Rooks = Bitwise.CountSetBits(Position.BlackRooks);
                side1Queens = Bitwise.CountSetBits(Position.BlackQueens);
                side2Knights = Bitwise.CountSetBits(Position.WhiteKnights);
                side2Bishops = Bitwise.CountSetBits(Position.WhiteBishops);
                side2Rooks = Bitwise.CountSetBits(Position.WhiteRooks);
                side2Queens = Bitwise.CountSetBits(Position.WhiteQueens);
            }
            int side1MinorPieces = side1Knights + side1Bishops;
            int side1MajorPieces = side1Rooks + side1Queens;
            int side2MinorPieces = side2Knights + side2Bishops;
            int side2MajorPieces = side2Rooks + side2Queens;
            bool side1QueenOr2Rooks = ((side1Queens == 1) && (side1MajorPieces == 1) && (side1MinorPieces == 0)) || ((side1Rooks == 2) && (side1MajorPieces == 2) && (side1MinorPieces == 0));
            bool side2QueenOr2Rooks = ((side2Queens == 1) && (side2MajorPieces == 1) && (side2MinorPieces == 0)) || ((side2Rooks == 2) && (side2MajorPieces == 2) && (side2MinorPieces == 0));
            if (side1QueenOr2Rooks && side2QueenOr2Rooks) return true; // Both sides have queen or two rooks.
            if (side1QueenOr2Rooks)
            {
                // Side 1 has queen or two rooks.
                if ((side2Rooks == 1) && (side2MajorPieces == 1))
                {
                    if ((side2Bishops == 1) && (side2Knights == 1)) return true; // Side 2 has rook, bishop, and knight.
                    if (side2MinorPieces == 1) return true; // Side 2 has rook and minor piece.
                }
            }
            if ((side1Rooks == 1) && (side1MajorPieces == 1) && (side1MinorPieces == 1))
            {
                // Side 1 has rook and minor piece.
                switch (side2Rooks)
                {
                    case 1 when (side2MajorPieces == 1) && (side2MinorPieces == 1): // Side 2 has rook and minor piece.
                    case 1 when (side2MajorPieces == 1) && (side2MinorPieces == 0): // Side 2 has rook.
                        return true;
                }
            }
            if ((side1Bishops == 1) && (side1Knights == 1) && (side1MajorPieces == 0))
            {
                // Side 1 has bishop and knight.
                if ((side2Rooks == 1) && (side2MajorPieces == 1) && (side2MinorPieces == 0)) return true; // Side 2 has rook.
            }
            if ((side1Rooks == 1) && (side1MajorPieces == 1) && (side1MinorPieces == 0))
            {
                // Side 1 has rook.
                if ((side2Rooks == 1) && (side2MajorPieces == 1) && (side2MinorPieces == 0)) return true; // Side 2 has rook.
                if ((side2MinorPieces == 1) && (side2MajorPieces == 0)) return true; // Side 2 has minor piece.
            }
            if ((side1Knights == 2) && (side1MinorPieces == 2) && (side1MajorPieces == 0))
            {
                // Side 1 has two knights.
                if ((side2MinorPieces <= 1) && (side2MajorPieces == 0)) return true; // Side 2 has one or zero minor pieces.
            }
            return false;
        }


        public int GetStaticScore(Position Position)
        {
            Stats.Count++;
            _staticScore.Reset();
            GetMaterialScore(Position);
            EvaluatePieceLocation(Position);
            EvaluatePawns(Position);
            int phase = DetermineGamePhase(Position);
            return Position.WhiteMove ? _staticScore.TotalScore(phase) : -_staticScore.TotalScore(phase);
        }


        public int GetMaterialScore(Position Position)
        {
            _staticScore.WhiteMaterial = Bitwise.CountSetBits(Position.WhitePawns) * PawnMaterial + Bitwise.CountSetBits(Position.WhiteKnights) * Config.KnightMaterial +
                                         Bitwise.CountSetBits(Position.WhiteBishops) * Config.BishopMaterial + Bitwise.CountSetBits(Position.WhiteRooks) * Config.RookMaterial +
                                         Bitwise.CountSetBits(Position.WhiteQueens) * Config.QueenMaterial;
            _staticScore.BlackMaterial = Bitwise.CountSetBits(Position.BlackPawns) * PawnMaterial + Bitwise.CountSetBits(Position.BlackKnights) * Config.KnightMaterial +
                                         Bitwise.CountSetBits(Position.BlackBishops) * Config.BishopMaterial + Bitwise.CountSetBits(Position.BlackRooks) * Config.RookMaterial +
                                         Bitwise.CountSetBits(Position.BlackQueens) * Config.QueenMaterial;
            return Position.WhiteMove ? _staticScore.WhiteMaterial - _staticScore.BlackMaterial : _staticScore.BlackMaterial - _staticScore.WhiteMaterial;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMateScore(int Depth) =>  -StaticScore.Checkmate - StaticScore.LongestCheckmate + Depth;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMateDistance(int Score)
        {
            int mateDistance = (Score > 0)
                ? StaticScore.Checkmate + StaticScore.LongestCheckmate - Score
                : -StaticScore.Checkmate - StaticScore.LongestCheckmate - Score;
            // Convert plies to full moves.
            int quotient = Math.DivRem(mateDistance, 2, out int remainder);
            return quotient + remainder;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DetermineGamePhase(Position Position)
        {
            int phase = _knightPhase * (Bitwise.CountSetBits(Position.WhiteKnights) + Bitwise.CountSetBits(Position.BlackKnights)) +
                        _bishopPhase * (Bitwise.CountSetBits(Position.WhiteBishops) + Bitwise.CountSetBits(Position.BlackBishops)) +
                        _rookPhase * (Bitwise.CountSetBits(Position.WhiteRooks) + Bitwise.CountSetBits(Position.BlackRooks)) +
                        _queenPhase * (Bitwise.CountSetBits(Position.WhiteQueens) + Bitwise.CountSetBits(Position.BlackQueens));
            return Math.Min(phase, _middlegamePhase);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNonLinearBonus(double Bonus, double Scale, double Power, int Constant) => (int)(Scale * Math.Pow(Bonus, Power)) + Constant;


        public string ShowParameters()
        {
            StringBuilder stringBuilder = new StringBuilder();
            // Material
            stringBuilder.AppendLine("Material");
            stringBuilder.AppendLine("===========");
            stringBuilder.AppendLine($"Pawn:    {PawnMaterial}");
            stringBuilder.AppendLine($"Knight:  {Config.KnightMaterial}");
            stringBuilder.AppendLine($"Bishop:  {Config.BishopMaterial}");
            stringBuilder.AppendLine($"Rook:    {Config.RookMaterial}");
            stringBuilder.AppendLine($"Queen:   {Config.QueenMaterial}");
            stringBuilder.AppendLine();
            // Piece Location
            stringBuilder.AppendLine("Middlegame Pawn Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_mgPawnLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame Pawn Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_egPawnLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Middlegame Knight Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_mgKnightLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame Knight Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_egKnightLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Middlegame Bishop Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_mgBishopLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame Bishop Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_egBishopLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Middlegame Rook Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_mgRookLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame Rook Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_egRookLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Middlegame Queen Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_mgQueenLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame Queen Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_egQueenLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Middlegame King Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_mgKingLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame King Location");
            stringBuilder.AppendLine("==============================================");
            ShowSquareParameters(_egKingLocations, stringBuilder);
            stringBuilder.AppendLine();
            // Passed Pawns
            stringBuilder.Append("Middlegame Passed Pawns:            ");
            ShowRankParameters(_mgPassedPawns, stringBuilder);
            stringBuilder.Append("Endgame Passed Pawns:               ");
            ShowRankParameters(_egPassedPawns, stringBuilder);
            stringBuilder.Append("Endgame Free Passed Pawns:          ");
            ShowRankParameters(_egFreePassedPawns, stringBuilder);
            stringBuilder.AppendLine($"Endgame King Escorted Passed Pawn:  {Config.EgKingEscortedPassedPawn}");
            stringBuilder.AppendLine($"Unstoppable Passed Pawn:            {Config.UnstoppablePassedPawn}");
            return stringBuilder.ToString();
        }


        public void Reset(bool PreserveStats)
        {
            if (!PreserveStats) Stats.Reset();
        }


        public string ToString(Position Position)
        {
            GetStaticScore(Position);
            int phase = DetermineGamePhase(Position);
            return _staticScore.ToString(phase);
        }


        private void EvaluatePieceLocation(Position Position)
        {
            // Pawns
            int square;
            ulong pieces = Position.WhitePawns;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgPawnLocations[square];
                _staticScore.WhiteEgPieceLocation += _egPawnLocations[square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackPawns;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                int blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgPawnLocations[blackSquare];
                _staticScore.BlackEgPieceLocation += _egPawnLocations[blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Knights
            pieces = Position.WhiteKnights;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgKnightLocations[square];
                _staticScore.WhiteEgPieceLocation += _egKnightLocations[square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackKnights;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                int blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgKnightLocations[blackSquare];
                _staticScore.BlackEgPieceLocation += _egKnightLocations[blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Bishops
            pieces = Position.WhiteBishops;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgBishopLocations[square];
                _staticScore.WhiteEgPieceLocation += _egBishopLocations[square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackBishops;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                int blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgBishopLocations[blackSquare];
                _staticScore.BlackEgPieceLocation += _egBishopLocations[blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Rooks
            pieces = Position.WhiteRooks;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgRookLocations[square];
                _staticScore.WhiteEgPieceLocation += _egRookLocations[square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackRooks;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                int blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgRookLocations[blackSquare];
                _staticScore.BlackEgPieceLocation += _egRookLocations[blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Queens
            pieces = Position.WhiteQueens;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgQueenLocations[square];
                _staticScore.WhiteEgPieceLocation += _egQueenLocations[square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackQueens;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                int blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgQueenLocations[blackSquare];
                _staticScore.BlackEgPieceLocation += _egQueenLocations[blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
        }


        private void EvaluatePawns(Position Position)
        {
            // White pawns
            ulong pawns = Position.WhitePawns;
            int kingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
            int enemyKingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
            int pawnSquare;
            int rank;
            while ((pawnSquare = Bitwise.FindFirstSetBit(pawns)) != Square.Illegal)
            {
                if (_isPassedPawn(pawnSquare, true))
                {
                    rank = Board.WhiteRanks[pawnSquare];
                    _staticScore.WhiteEgKingEscortedPassedPawns += (Board.SquareDistances[pawnSquare][enemyKingSquare] - Board.SquareDistances[pawnSquare][kingSquare]) * Config.EgKingEscortedPassedPawn;
                    if (_isFreePawn(pawnSquare, true))
                    {
                        if (IsPawnUnstoppable(Position, pawnSquare, enemyKingSquare, true, true)) _staticScore.WhiteUnstoppablePassedPawns += Config.UnstoppablePassedPawn; // Pawn is unstoppable.
                        else _staticScore.WhiteEgFreePassedPawns += _egFreePassedPawns[rank]; // Pawn is passed and free.
                    }
                    else
                    {
                        // Pawn is passed.
                        _staticScore.WhiteMgPassedPawns += _mgPassedPawns[rank];
                        _staticScore.WhiteEgPassedPawns += _egPassedPawns[rank];
                    }
                }
                Bitwise.ClearBit(ref pawns, pawnSquare);
            }
            // Black pawns
            pawns = Position.BlackPawns;
            kingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
            enemyKingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
            while ((pawnSquare = Bitwise.FindFirstSetBit(pawns)) != Square.Illegal)
            {
                if (_isPassedPawn(pawnSquare, false))
                {
                    rank = Board.BlackRanks[pawnSquare];
                    _staticScore.BlackEgKingEscortedPassedPawns += (Board.SquareDistances[pawnSquare][enemyKingSquare] - Board.SquareDistances[pawnSquare][kingSquare]) * Config.EgKingEscortedPassedPawn;
                    if (_isFreePawn(pawnSquare, false))
                    {
                        if (IsPawnUnstoppable(Position, pawnSquare, enemyKingSquare, false, true)) _staticScore.BlackUnstoppablePassedPawns += Config.UnstoppablePassedPawn; // Pawn is unstoppable.
                        else _staticScore.BlackEgFreePassedPawns += _egFreePassedPawns[rank]; // Pawn is passed and free.
                    }
                    else
                    {
                        // Pawn is passed.
                        _staticScore.BlackMgPassedPawns += _mgPassedPawns[rank];
                        _staticScore.BlackEgPassedPawns += _egPassedPawns[rank];
                    }
                }
                Bitwise.ClearBit(ref pawns, pawnSquare);
            }
        }


        private static bool IsPawnUnstoppable(Position Position, int PawnSquare, int EnemyKingSquare, bool White, bool IsFree)
        {
            if (!IsFree) return false;
            // Pawn is free to advance to promotion square.
            int file = Board.Files[PawnSquare];
            int promotionSquare;
            int enemyPieces;
            if (White)
            {
                // White pawn
                promotionSquare = Board.GetSquare(file, 7);
                enemyPieces = Bitwise.CountSetBits(Position.BlackKnights) + Bitwise.CountSetBits(Position.BlackBishops) + Bitwise.CountSetBits(Position.BlackRooks) + Bitwise.CountSetBits(Position.BlackQueens);
            }
            else
            {
                // Black pawn
                promotionSquare = Board.GetSquare(file, 0);
                enemyPieces = Bitwise.CountSetBits(Position.WhiteKnights) + Bitwise.CountSetBits(Position.WhiteBishops) + Bitwise.CountSetBits(Position.WhiteRooks) + Bitwise.CountSetBits(Position.WhiteQueens);
            }
            if (enemyPieces == 0)
            {
                // Enemy has no minor or major pieces.
                int pawnDistanceToPromotionSquare = Board.SquareDistances[PawnSquare][promotionSquare];
                int kingDistanceToPromotionSquare = Board.SquareDistances[EnemyKingSquare][promotionSquare];
                if (White != Position.WhiteMove) kingDistanceToPromotionSquare--; // Enemy king can move one square closer to pawn.
                return kingDistanceToPromotionSquare > pawnDistanceToPromotionSquare; // Enemy king cannot stop pawn from promoting.
            }
            return false;
        }


        private static void ShowSquareParameters(int[] Parameters, StringBuilder StringBuilder)
        {
            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    int square = Board.GetSquare(file, rank);
                    StringBuilder.Append(Parameters[square].ToString("+000;-000").PadRight(6));
                }
                StringBuilder.AppendLine();
            }
        }


        private static void ShowRankParameters(int[] Parameters, StringBuilder StringBuilder)
        {
            for (int rank = 0; rank < 8; rank++) StringBuilder.Append(Parameters[rank].ToString("000").PadRight(5));
            StringBuilder.AppendLine();
        }
    }
}
