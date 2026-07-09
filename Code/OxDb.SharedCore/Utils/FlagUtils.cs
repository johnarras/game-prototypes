using System;

namespace OxDb.SharedCore.Utils
{
    public static class FlagUtils
    {
        public static bool MatchesAnyBits(long val, long flag)
        {
            return (val & flag) != 0;
        }

        public static bool HasBitIndex(long val, long bit)
        {
            return MatchesAnyBits(val, (1 << (int)bit));
        }

        public static long GetBitCount(long value)
        {
            UInt64 x = (UInt64)value;

            // This is a "SWAR" (SIMD Within A Register) algorithm.
            // It masks and adds bits in parallel.
            x = x - ((x >> 1) & 0x5555555555555555);
            x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333);
            x = (x + (x >> 4)) & 0x0F0F0F0F0F0F0F0F;

            return (int)((x * 0x0101010101010101) >> 56);
        }
    }
}


