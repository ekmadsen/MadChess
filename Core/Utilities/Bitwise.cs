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
using System.Linq; // Use LINQ only for Debug.Asserts.
using System.Numerics; // Enables CPU intrinsics (popcount and bitscan).  Falls back to software implementation for CPU that lack the intrinsic operation.
using System.Runtime.CompilerServices;
using ErikTheCoder.MadChess.Core.Game;


namespace ErikTheCoder.MadChess.Core.Utilities;


public static class Bitwise
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedMember.Local
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
        Debug.Assert(indices.All(index => (index >= 0) && (index < 32)));
        var mask = 0u;
        for (var index = 0; index < indices.Length; index++) SetBit(ref mask, indices[index]);
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


    public static ulong CreateULongMask(int[] indices)
    {
        Debug.Assert(indices.All(index => (index >= 0) && (index < 64)));
        var mask = 0ul;
        for (var index = 0; index < indices.Length; index++) SetBit(ref mask, indices[index]);
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountSetBits(uint value) => BitOperations.PopCount(value);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountSetBits(ulong value) => BitOperations.PopCount(value);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FirstSetBit(uint value) => value == 0 ? -1 : BitOperations.TrailingZeroCount(value);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square FirstSetSquare(ulong value) => value == 0 ? Square.Illegal : (Square)BitOperations.TrailingZeroCount(value);


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
            while ((permutationIndex = FirstSetBit(permutationIndices)) >= 0)
            {
                var maskIndex = maskSetBits[permutationIndex];
                SetBit(ref permutation, maskIndex);
                ClearBit(ref permutationIndices, permutationIndex);
            }
            permutations.Add(permutation);
        }
        return permutations;
    }
    // ReSharper restore UnusedMember.Local
    // ReSharper restore UnusedMember.Global
    // ReSharper restore MemberCanBePrivate.Global
}