using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Core.Utilities;


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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Max(int value1, int value2) => value1 > value2 ? value1 : value2;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Min(int value1, int value2) => value1 < value2 ? value1 : value2;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;

        return (value > max)
            ? max
            : value;
    }
}