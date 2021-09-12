using ErikTheCoder.MadChess.Core.Game;


namespace ErikTheCoder.MadChess.Core
{
    public static class Delegates
    {
        public delegate bool ValidateMove(ref ulong move);
        public delegate bool Debug();
        public delegate void WriteMessageLine(string message);
        public delegate ulong GetPieceMovesMask(Square fromSquare, ulong occupancy);
    }
}
