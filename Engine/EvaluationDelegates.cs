namespace ErikTheCoder.MadChess.Engine
{
    public sealed class EvaluationDelegates
    {
        public Delegates.GetPositionCount GetPositionCount;
        public Delegates.IsPassedPawn IsPassedPawn;
        public Delegates.IsFreePawn IsFreePawn;
        public Delegates.GetPieceDestinations GetKnightDestinations;
        public Delegates.GetPieceDestinations GetBishopDestinations;
        public Delegates.GetPieceDestinations GetRookDestinations;
        public Delegates.GetPieceDestinations GetQueenDestinations;
        public Delegates.Debug Debug;
        public Delegates.WriteMessageLine WriteMessageLine;
    }
}
