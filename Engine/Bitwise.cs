// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices; // Use LINQ only for Debug.Asserts.
using System.Text;
#if !RELEASENONPOPCOUNT
using System.Runtime.Intrinsics.X86;
#endif


namespace ErikTheCoder.MadChess.Engine
{
    // See https://graphics.stanford.edu/~seander/bithacks.html.
    public static class Bitwise
    {
        private const int _intBits = 32;
        private const int _longBits = 64;
#if RELEASENONPOPCOUNT
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


        // ReSharper disable UnusedMember.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public static uint CreateUIntMask(int Index)
        {
            Debug.Assert((Index >= 0) && (Index < _intBits));
            return 1u << Index;
        }


        public static uint CreateUIntMask(int LeastSignificantBit, int MostSignificantBit)
        {
            Debug.Assert((LeastSignificantBit >= 0) && (LeastSignificantBit < _intBits));
            Debug.Assert((MostSignificantBit >= 0) && (MostSignificantBit < _intBits));
            Debug.Assert(LeastSignificantBit <= MostSignificantBit);
            var mask = 0u;
            for (var index = LeastSignificantBit; index <= MostSignificantBit; index++) SetBit(ref mask, index);
            return mask;
        }


        // ReSharper disable once MemberCanBePrivate.Global
        public static uint CreateUIntMask(int[] Indices)
        {
            var mask = 0u;
            for (var index = 0; index < Indices.Length; index++) SetBit(ref mask, Indices[index]);
            Debug.Assert(Indices.All(Index => (Index >= 0) && (Index < _intBits)));
            return mask;
        }


        public static ulong CreateULongMask(int Index)
        {
            Debug.Assert(Index >= 0 && Index < _longBits);
            return 1ul << Index;
        }


        public static ulong CreateULongMask(int LeastSignificantBit, int MostSignificantBit)
        {
            Debug.Assert((LeastSignificantBit) >= 0 && (LeastSignificantBit < _longBits));
            Debug.Assert((MostSignificantBit >= 0) && (MostSignificantBit < _longBits));
            Debug.Assert(LeastSignificantBit <= MostSignificantBit);
            var mask = 0ul;
            for (var index = LeastSignificantBit; index <= MostSignificantBit; index++) SetBit(ref mask, index);
            return mask;
        }


        public static ulong CreateULongMask(int[] Indices)
        {
            var mask = 0ul;
            for (var index = 0; index < Indices.Length; index++) SetBit(ref mask, Indices[index]);
            Debug.Assert(Indices.All(Index => (Index >= 0) && (Index < _longBits)));
            return mask;
        }


        public static uint CreateUIntUnmask(int Index)
        {
            Debug.Assert((Index >= 0) && (Index < _intBits));
            return ~CreateUIntMask(Index);
        }


        public static uint CreateUIntUnmask(int LeastSignificantBit, int MostSignificantBit)
        {
            Debug.Assert((LeastSignificantBit >= 0) && (LeastSignificantBit < _intBits));
            Debug.Assert((MostSignificantBit >= 0) && (MostSignificantBit < _intBits));
            Debug.Assert(LeastSignificantBit <= MostSignificantBit);
            return ~CreateUIntMask(LeastSignificantBit, MostSignificantBit);
        }


        public static uint CreateUIntUnMask(int[] Indices)
        {
            Debug.Assert(Indices.All(Index => (Index >= 0) && (Index < _intBits)));
            return ~CreateUIntMask(Indices);
        }


        public static ulong CreateULongUnmask(int Index)
        {
            Debug.Assert(Index >= 0 && Index < _longBits);
            return ~CreateULongMask(Index);
        }


        public static ulong CreateULongUnmask(int LeastSignificantBit, int MostSignificantBit)
        {
            Debug.Assert((LeastSignificantBit >= 0) && (LeastSignificantBit < _longBits));
            Debug.Assert((MostSignificantBit >= 0) && (MostSignificantBit < _longBits));
            Debug.Assert(LeastSignificantBit <= MostSignificantBit);
            return ~CreateULongMask(LeastSignificantBit, MostSignificantBit);
        }


        public static ulong CreateULongUnMask(int[] Indices)
        {
            Debug.Assert(Indices.All(Index => (Index >= 0) && (Index < _longBits)));
            return ~CreateULongMask(Indices);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once MemberCanBePrivate.Global
        public static void SetBit(ref uint Value, int Index)
        {
            Debug.Assert((Index >= 0) && (Index < _intBits));
            Value |= 1u << Index;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(ref ulong Value, int Index)
        {
            Debug.Assert((Index >= 0) && (Index < _longBits));
            Value |= 1ul << Index;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearBit(ref uint Value, int Index)
        {
            Debug.Assert((Index >= 0) && (Index < _intBits));
            Value &= ~(1u << Index);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearBit(ref ulong Value, int Index)
        {
            Debug.Assert((Index >= 0) && (Index < _longBits));
            Value &= ~(1ul << Index);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToggleBit(ref uint Value, int Index)
        {
            Debug.Assert((Index >= 0) && (Index < _intBits));
            Value ^= 1u << Index;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToggleBit(ref ulong Value, int Index)
        {
            Debug.Assert((Index >= 0) && (Index < _longBits));
            Value ^= 1ul << Index;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once MemberCanBePrivate.Global
        public static bool IsBitSet(uint Value, int Index)
        {
            Debug.Assert((Index >= 0) && (Index < _intBits));
            return (Value & (1u << Index)) > 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(ulong Value, int Index)
        {
            Debug.Assert((Index >= 0) && (Index < _longBits));
            return (Value & (1ul << Index)) > 0;
        }


#if RELEASENONPOPCOUNT
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountSetBits(uint Value)
        {
            var count = 0;
            while (Value > 0)
            {
                count++;
                Value &= Value - 1u;
            }
            Debug.Assert((count >= 0) && (count <= _intBits));
            return count;
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountSetBits(uint Value) => (int) Popcnt.PopCount(Value);
#endif


#if RELEASENONPOPCOUNT
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountSetBits(ulong Value)
        {
            var count = 0;
            while (Value > 0)
            {
                count++;
                Value &= Value - 1ul;
            }
            Debug.Assert((count >= 0) && (count <= _longBits));
            return count;
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountSetBits(ulong Value) => (int) Popcnt.X64.PopCount(Value);
#endif


#if RELEASENONPOPCOUNT
        // See https://stackoverflow.com/questions/37083402/fastest-way-to-get-last-significant-bit-position-in-a-ulong-c
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindFirstSetBit(ulong Value)
        {
            return Value == 0 ? Square.Illegal : _multiplyDeBruijnBitPosition[((ulong)((long)Value & -(long)Value) * _deBruijnSequence) >> 58];
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindFirstSetBit(ulong Value) => Value == 0 ? Square.Illegal : _longBits - (int) Lzcnt.X64.LeadingZeroCount(Value) - 1;
#endif


        public static IEnumerable<ulong> GetAllPermutations(ulong Mask)
        {
            var setBits = new List<int>();
            for (var index = 0; index < 64; index++) if (IsBitSet(Mask, index)) setBits.Add(index);
            return GetAllPermutations(setBits, 0, 0);
        }


        private static IEnumerable<ulong> GetAllPermutations(List<int> SetBits, int Index, ulong Value)
        {
            SetBit(ref Value, SetBits[Index]);
            yield return Value;
            var index = Index + 1;
            if (index < SetBits.Count)
            {
                using (var occupancyPermutations = GetAllPermutations(SetBits, index, Value).GetEnumerator())
                {
                    while (occupancyPermutations.MoveNext()) yield return occupancyPermutations.Current;
                }
            }
            ClearBit(ref Value, SetBits[Index]);
            yield return Value;
            if (index < SetBits.Count)
            {
                using (var occupancyPermutations = GetAllPermutations(SetBits, index, Value).GetEnumerator())
                {
                    while (occupancyPermutations.MoveNext()) yield return occupancyPermutations.Current;
                }
            }
        }


        public static string ToString(uint Value)
        {
            var stringBuilder = new StringBuilder();
            for (var index = _intBits - 1; index >= 0; index--)
            {
                stringBuilder.Append(IsBitSet(Value, index) ? '1' : '0');
                if ((index <= _intBits && index > 0) && (index % 8 == 0)) stringBuilder.Append('_');
            }
            return stringBuilder.ToString();
        }


        public static string ToString(ulong Value)
        {
            var stringBuilder = new StringBuilder();
            for (var index = _longBits - 1; index >= 0; index--)
            {
                stringBuilder.Append(IsBitSet(Value, index) ? '1' : '0');
                if ((index <= _longBits && index > 0) && (index % 8 == 0)) stringBuilder.Append('_');
            }
            return stringBuilder.ToString();
        }
        // ReSharper restore UnusedMember.Global
    }
}