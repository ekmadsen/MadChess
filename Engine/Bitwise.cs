// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Diagnostics;
using System.Linq; // Use LINQ only for Debug.Asserts.
using System.Runtime.CompilerServices;
using System.Text;
using System.Runtime.Intrinsics.X86;


namespace ErikTheCoder.MadChess.Engine
{
    // See https://graphics.stanford.edu/~seander/bithacks.html.
    public static class Bitwise
    {
        private const int _intBits = 32;
        private const int _longBits = 64;
        

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
            uint mask = 0;
            for (int index = LeastSignificantBit; index <= MostSignificantBit; index++) SetBit(ref mask, index);
            return mask;
        }


        // ReSharper disable once MemberCanBePrivate.Global
        public static uint CreateUIntMask(int[] Indices)
        {
            uint mask = 0;
            for (int index = 0; index < Indices.Length; index++) SetBit(ref mask, Indices[index]);
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
            ulong mask = 0;
            for (int index = LeastSignificantBit; index <= MostSignificantBit; index++) SetBit(ref mask, index);
            return mask;
        }


        public static ulong CreateULongMask(int[] Indices)
        {
            ulong mask = 0;
            for (int index = 0; index < Indices.Length; index++) SetBit(ref mask, Indices[index]);
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountSetBits(uint Value) => (int) Popcnt.PopCount(Value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountSetBits(ulong Value) => (int) Popcnt.X64.PopCount(Value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindFirstSetBit(ulong Value) => Value == 0 ? Square.Illegal : _longBits - (int) Lzcnt.X64.LeadingZeroCount(Value) - 1;


        public static IEnumerable<ulong> GetAllPermutations(ulong Mask)
        {
            List<int> setBits = new List<int>();
            for (int index = 0; index < 64; index++) if (IsBitSet(Mask, index)) setBits.Add(index);
            return GetAllPermutations(setBits, 0, 0);
        }


        private static IEnumerable<ulong> GetAllPermutations(List<int> SetBits, int Index, ulong Value)
        {
            SetBit(ref Value, SetBits[Index]);
            yield return Value;
            int index = Index + 1;
            if (index < SetBits.Count)
                using (IEnumerator<ulong> occupancyPermutations = GetAllPermutations(SetBits, index, Value).GetEnumerator()) { while (occupancyPermutations.MoveNext()) yield return occupancyPermutations.Current; }
            ClearBit(ref Value, SetBits[Index]);
            yield return Value;
            if (index < SetBits.Count)
                using (IEnumerator<ulong> occupancyPermutations = GetAllPermutations(SetBits, index, Value).GetEnumerator()) { while (occupancyPermutations.MoveNext()) yield return occupancyPermutations.Current; }
        }


        public static string ToString(uint Value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = _intBits - 1; index >= 0; index--)
            {
                stringBuilder.Append(IsBitSet(Value, index) ? '1' : '0');
                if (index <= _intBits && index > 0) if (index % 8 == 0) stringBuilder.Append('_');
            }
            return stringBuilder.ToString();
        }


        public static string ToString(ulong Value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = _longBits - 1; index >= 0; index--)
            {
                stringBuilder.Append(IsBitSet(Value, index) ? '1' : '0');
                if (index <= _longBits && index > 0) if (index % 8 == 0) stringBuilder.Append('_');
            }
            return stringBuilder.ToString();
        }
        // ReSharper restore UnusedMember.Global
    }
}