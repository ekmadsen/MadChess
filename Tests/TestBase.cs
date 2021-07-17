using System;


namespace ErikTheCoder.MadChess.Tests
{
    public abstract class TestBase
    {
        protected static void WriteMessageLine(string message) => Console.WriteLine(message);
        protected static void WriteMessageLine() => Console.WriteLine();
    }
}
