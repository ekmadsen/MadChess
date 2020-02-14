namespace ErikTheCoder.MadChess.Engine
{
    public sealed class EvaluationDelegates
    {
        public Delegates.GetPositionCount GetPositionCount;
        public Delegates.GetPieceDestinations GetKnightDestinations;
        public Delegates.GetPieceDestinations GetBishopDestinations;
        public Delegates.GetPieceDestinations GetRookDestinations;
        public Delegates.GetPieceDestinations GetQueenDestinations;
        public Delegates.AddPiece AddPiece;
        public Delegates.RemovePiece RemovePiece;
        public Delegates.Debug Debug;
        public Delegates.WriteMessageLine WriteMessageLine;
    }
}
