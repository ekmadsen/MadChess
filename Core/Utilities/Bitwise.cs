// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics; // Enables CPU intrinsics (popcount and bitscan).  Falls back to software implementation when CPU lacks the intrinsic operation.
using System.Runtime.CompilerServices;
using ErikTheCoder.MadChess.Core.Game;


namespace ErikTheCoder.MadChess.Core.Utilities;


public static class Bitwise
{
    public static uint CreateUIntMask(int index)
    {
        Debug.Assert((index >= 0) && (index < 32));
        return 1u << index;
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
    private static void SetBit(ref ulong value, int index)
    {
        Debug.Assert((index >= 0) && (index < 64));
        value |= 1ul << index;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBit(ref ulong value, Square square) => value |= 1ul << (int)square;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsBitSet(ulong value, int index)
    {
        Debug.Assert((index >= 0) && (index < 64));
        return (value & (1ul << index)) > 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBitSet(ulong value, Square square) => (value & (1ul << (int)square)) > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountSetBits(ulong value) => BitOperations.PopCount(value);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int PopFirstSetBit(ref uint value)
    {
        if (value == 0) return -1;
        var bit = BitOperations.TrailingZeroCount(value);
        value &= (value - 1); // Clear the first set bit.
        return bit;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square FirstSetSquare(ulong value) => value == 0 ? Square.Illegal : (Square)BitOperations.TrailingZeroCount(value);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square PopFirstSetSquare(ref ulong value)
    {
        if (value == 0) return Square.Illegal;
        var square = (Square)BitOperations.TrailingZeroCount(value);
        value &= (value - 1); // Clear the first set square.
        return square;
    }


    public static List<ulong> GetAllPermutations(ulong mask)
    {
        var maskSetBitCount = CountSetBits(mask);
        Debug.Assert(maskSetBitCount <= 14); // Greatest number of moves in rank / file or diagonal direction.
        // Determine which bits are set in the mask.
        var maskSetBits = new List<int>();
        for (var maskIndex = 0; maskIndex < 64; maskIndex++) if (IsBitSet(mask, maskIndex)) maskSetBits.Add(maskIndex);
        // The binary representation of integers from 0 to ((2 Pow n) - 1) contains all permutations of n bits.
        var permutationCount = (int)Math.Pow(2, maskSetBitCount);
        var permutations = new List<ulong>(permutationCount);
        for (var index = 0; index < permutationCount; index++)
        {
            var permutationIndices = (uint) index;
            var permutation = 0ul;
            // Map the permutation index to the mask index and set the bit located at the mask index.
            int permutationIndex;
            while ((permutationIndex = PopFirstSetBit(ref permutationIndices)) >= 0)
            {
                var maskIndex = maskSetBits[permutationIndex];
                SetBit(ref permutation, maskIndex);
            }
            permutations.Add(permutation);
        }
        return permutations;
    }
}