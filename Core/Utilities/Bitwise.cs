// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Diagnostics;
using System.Linq; // Use LINQ only for Debug.Asserts.
using System.Runtime.CompilerServices;
using ErikTheCoder.MadChess.Core.Game;
#if POPCOUNT
using System.Numerics;
#endif


namespace ErikTheCoder.MadChess.Core.Utilities;


public static class Bitwise
{
#if (!POPCOUNT)
    private const ulong _deBruijnSequence = 0x37E84A99DAE458F;
    private static readonly int[] _multiplyDeBruijnBitPosition;


    static Bitwise()
    {
        _multiplyDeBruijnBitPosition = new[]
        {
            00, 01, 17, 02, 18, 50, 03, 57,
            47, 19, 22, 51, 29, 04, 33, 58,
            15, 48, 20, 27, 25, 23, 52, 41,
            54, 30, 38, 05, 43, 34, 59, 08,
            63, 16, 49, 56, 46, 21, 28, 32,
            14, 26, 24, 40, 53, 37, 42, 07,
            62, 55, 45, 31, 13, 39, 36, 06,
            61, 44, 12, 35, 60, 11, 10, 09
        };
    }
#endif


    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedMember.Global
    public static uint CreateUIntMask(int index)
    {
        Debug.Assert((index >= 0) && (index < 32));
        return 1u << index;
    }


    public static uint CreateUIntMask(int leastSignificantBit, int mostSignificantBit)
    {
        Debug.Assert((leastSignificantBit >= 0) && (leastSignificantBit < 32));
        Debug.Assert((mostSignificantBit >= 0) && (mostSignificantBit < 32));
        Debug.Assert(leastSignificantBit <= mostSignificantBit);
        var mask = 0u;
        for (var index = leastSignificantBit; index <= mostSignificantBit; index++) SetBit(ref mask, index);
        return mask;
    }


    public static uint CreateUIntMask(int[] indices)
    {
        var mask = 0u;
        for (var index = 0; index < indices.Length; index++) SetBit(ref mask, indices[index]);
        Debug.Assert(indices.All(index => (index >= 0) && (index < 32)));
        return mask;
    }


    public static ulong CreateULongMask(int index)
    {
        Debug.Assert(index >= 0 && index < 64);
        return 1ul << index;
    }


    public static ulong CreateULongMask(Square square) => 1ul << (int)square;


    public static ulong CreateULongMask(int leastSignificantBit, int mostSignificantBit)
    {
        Debug.Assert((leastSignificantBit) >= 0 && (leastSignificantBit < 64));
        Debug.Assert((mostSignificantBit >= 0) && (mostSignificantBit < 64));
        Debug.Assert(leastSignificantBit <= mostSignificantBit);
        var mask = 0ul;
        for (var index = leastSignificantBit; index <= mostSignificantBit; index++) SetBit(ref mask, index);
        return mask;
    }


    public static ulong CreateULongMask(Square[] squares)
    {
        var mask = 0ul;
        for (var index = 0; index < squares.Length; index++) SetBit(ref mask, squares[index]);
        return mask;
    }


    public static uint CreateUIntUnmask(int index)
    {
        Debug.Assert((index >= 0) && (index < 32));
        return ~CreateUIntMask(index);
    }


    public static uint CreateUIntUnmask(int leastSignificantBit, int mostSignificantBit)
    {
        Debug.Assert((leastSignificantBit >= 0) && (leastSignificantBit < 32));
        Debug.Assert((mostSignificantBit >= 0) && (mostSignificantBit < 32));
        Debug.Assert(leastSignificantBit <= mostSignificantBit);
        return ~CreateUIntMask(leastSignificantBit, mostSignificantBit);
    }


    public static uint CreateUIntUnMask(int[] indices)
    {
        Debug.Assert(indices.All(index => (index >= 0) && (index < 32)));
        return ~CreateUIntMask(indices);
    }


    public static ulong CreateULongUnmask(int index)
    {
        Debug.Assert(index >= 0 && index < 64);
        return ~CreateULongMask(index);
    }


    public static ulong CreateULongUnmask(Square square) => ~CreateULongMask(square);


    public static ulong CreateULongUnmask(int leastSignificantBit, int mostSignificantBit)
    {
        Debug.Assert((leastSignificantBit >= 0) && (leastSignificantBit < 64));
        Debug.Assert((mostSignificantBit >= 0) && (mostSignificantBit < 64));
        Debug.Assert(leastSignificantBit <= mostSignificantBit);
        return ~CreateULongMask(leastSignificantBit, mostSignificantBit);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit(ref uint value, int index)
    {
        Debug.Assert((index >= 0) && (index < 32));
        value |= 1u << index;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetBit(ref ulong value, int index)
    {
        Debug.Assert((index >= 0) && (index < 64));
        value |= 1ul << index;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit(ref ulong value, Square square) => value |= 1ul << (int)square;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClearBit(ref uint value, int index)
    {
        Debug.Assert((index >= 0) && (index < 32));
        value &= ~(1u << index);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ClearBit(ref ulong value, int index)
    {
        Debug.Assert((index >= 0) && (index < 64));
        value &= ~(1ul << index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClearBit(ref ulong value, Square square) => value &= ~(1ul << (int)square);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ToggleBit(ref uint value, int index)
    {
        Debug.Assert((index >= 0) && (index < 32));
        value ^= 1u << index;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ToggleBit(ref ulong value, int index)
    {
        Debug.Assert((index >= 0) && (index < 32));
        value ^= 1ul << index;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBitSet(uint value, int index)
    {
        Debug.Assert((index >= 0) && (index < 32));
        return (value & (1u << index)) > 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsBitSet(ulong value, int index)
    {
        Debug.Assert((index >= 0) && (index < 64));
        return (value & (1ul << index)) > 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBitSet(ulong value, Square square) => (value & (1ul << (int)square)) > 0;


#if POPCOUNT
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountSetBits(uint value) => BitOperations.PopCount(value);
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountSetBits(uint value)
    {
        var count = 0;
        while (value > 0)
        {
            count++;
            value &= value - 1u;
        }
        Debug.Assert((count >= 0) && (count <= _intBits));
        return count;
    }
#endif


#if POPCOUNT
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountSetBits(ulong value) => BitOperations.PopCount(value);
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountSetBits(ulong value)
    {
        var count = 0;
        while (value > 0)
        {
            count++;
            value &= value - 1ul;
        }
        Debug.Assert((count >= 0) && (count <= 64));
        return count;
    }
#endif


#if POPCOUNT
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square FirstSetSquare(ulong value) => value == 0 ? Square.Illegal : (Square)BitOperations.TrailingZeroCount(value);
#else
    // See https://stackoverflow.com/questions/37083402/fastest-way-to-get-last-significant-bit-position-in-a-ulong-c
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square FirstSetSquare(ulong value)
    {
        return value == 0 ? Square.Illegal : (Square)_multiplyDeBruijnBitPosition[((ulong)((long)value & -(long)value) * _deBruijnSequence) >> 58];
    }
#endif


    public static IEnumerable<ulong> GetAllPermutations(ulong mask)
    {
        var setBits = new List<int>();
        for (var index = 0; index < 64; index++) if (IsBitSet(mask, index)) setBits.Add(index);
        return GetAllPermutations(setBits, 0, 0);
    }


    private static IEnumerable<ulong> GetAllPermutations(List<int> setBits, int index, ulong value)
    {
        SetBit(ref value, setBits[index]);
        yield return value;
        var index2 = index + 1;
        if (index2 < setBits.Count)
        {
            using (var occupancyPermutations = GetAllPermutations(setBits, index2, value).GetEnumerator())
            {
                while (occupancyPermutations.MoveNext()) yield return occupancyPermutations.Current;
            }
        }
        ClearBit(ref value, setBits[index]);
        yield return value;
        if (index2 < setBits.Count)
        {
            using (var occupancyPermutations = GetAllPermutations(setBits, index2, value).GetEnumerator())
            {
                while (occupancyPermutations.MoveNext()) yield return occupancyPermutations.Current;
            }
        }
    }
    // ReSharper restore UnusedMember.Global
    // ReSharper restore MemberCanBePrivate.Global
}