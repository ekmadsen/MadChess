namespace ErikTheCoder.MadChess.Engine
{
    public sealed class EvaluationDelegates
    {
        public Delegates.GetPositionCount GetPositionCount;
        public Delegates.IsPassedPawn IsPassedPawn;
        public Delegates.IsFreePawn IsFreePawn;
        public Delegates.GetPieceUnoccupiedDestinations GetKnightUnoccupiedDestinations;
        public Delegates.GetPieceUnoccupiedDestinations GetBishopUnoccupiedDestinations;
        public Delegates.GetPieceUnoccupiedDestinations GetRookUnoccupiedDestinations;
        public Delegates.GetPieceUnoccupiedDestinations GetQueenUnoccupiedDestinations;
        public Delegates.Debug Debug;
        public Delegates.WriteMessageLine WriteMessageLine;
    }
}
