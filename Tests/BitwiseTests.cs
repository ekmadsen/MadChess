// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using MadChess.Engine;
using NUnit.Framework;


namespace Tests
{
    [TestFixture]
    public sealed class BitwiseTests
    {
        [Test]
        public void TestCreateUIntMask()
        {
            UciStream uciStream = new UciStream();
            uint mask = Bitwise.CreateUIntMask(7, 11);
            uciStream.WriteMessageLine(Bitwise.ToString(mask));
            uciStream.WriteMessageLine();
            uciStream.WriteMessageLine(Board.ToString(mask));
            Assert.That(Bitwise.ToString(mask), Is.EqualTo("00000000_00000000_00001111_10000000"));
        }
    }
}