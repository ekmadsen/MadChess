// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Runtime.CompilerServices;
using JetBrains.Annotations;


namespace ErikTheCoder.MadChess.Core.Utilities;


public static class ExtensionMethods
{
    [UsedImplicitly]
    [ContractAnnotation("text: null => true")]
    public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);
}


public static class FastMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Abs(int value)
    {
        // Ensure value is positive using technique faster than System.Math.Abs().
        // See http://graphics.stanford.edu/~seander/bithacks.html#IntegerAbs.
        var mask = value >> 31;
        return (value ^ mask) - mask;
    }
}