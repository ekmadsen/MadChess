// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Engine.Tuning;


public sealed class Parameter
{
    public readonly string Name;
    public readonly int MinValue;
    public readonly int MaxValue;
    public int Value;


    public Parameter(string name, int minValue, int maxValue) : this(name, minValue, maxValue, SafeRandom.NextInt(minValue, maxValue + 1)) // Assign random initial value.
    {
    }


    private Parameter(string name, int minValue, int maxValue, int value)
    {
        Name = name;
        MinValue = minValue;
        MaxValue = maxValue;
        Value = value;
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