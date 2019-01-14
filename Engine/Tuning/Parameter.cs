// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine.Tuning
{
    public sealed class Parameter
    {
        public readonly string Name;
        public readonly int MinValue;
        public readonly int MaxValue;
        public int Value;


        public Parameter(string Name, int MinValue, int MaxValue) : this(Name, MinValue, MaxValue, SafeRandom.NextInt(MinValue, MaxValue + 1)) // Assign random initial value.
        {
        }


        private Parameter(string Name, int MinValue, int MaxValue, int Value)
        {
            this.Name = Name;
            this.MinValue = MinValue;
            this.MaxValue = MaxValue;
            this.Value = Value;
        }


        public Parameter DuplicateWithSameValue()
        {
            return new Parameter(Name, MinValue, MaxValue, Value);
        }


        public Parameter DuplicateWithRandomValue()
        {
            return new Parameter(Name, MinValue, MaxValue);
        }
    }
}
