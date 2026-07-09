using OxDb.SharedCore.Interfaces;
using System;
using System.Runtime.ConstrainedExecution;

namespace OxDb.SharedCore.Utils
{

    public interface IRandomContainer
    {
        IRandom Rand { get; }
    }
    /// <summary>
    /// I am not using Random.Shared because I want more control over the random numbers,
    /// so this interface is passed everywhere since the environment is multithreaded
    /// I can't just inject a ref.
    /// </summary>
    public interface IRandom
    {
        int Next();
        long NextLong();
        int Next(int maxVal);
        int Next(int minValue, int maxValue);
        long NextLong(long minValue, long maxValue);
        double NextDouble();
    }

    public interface IWeightedItem
    {
        double Weight { get; }
    }

    public interface IWeightedItemId : IWeightedItem
    {
        long GetId();
    }

    public interface IItemEnchantWeight
    {
        double ItemEnchantWeight { get; }
    }

    public class MyRandom : IRandom
    {
        private uint _s0;
        private uint _s1;
        private uint _s2;
        private uint _s3;

        /// <summary>
        /// Initializes the PRNG. If no seed is provided, defaults to DateTime.UtcNow.Ticks.
        /// </summary>
        public MyRandom(long seed = 0)
        {
            if (seed == 0)
            {
                seed = DateTime.UtcNow.Ticks;
            }

            uint low = (uint)(seed & 0xFFFFFFFF);
            uint high = (uint)(seed >> 32);

            _s0 = low;
            _s1 = high;
            _s2 = low + 0x9E3779B9;
            _s3 = high + 0xBB67AE85;
        }

        /// <summary>
        /// Core 32-bit PRNG advancement step.
        /// </summary>
        public uint NextUint()
        {
            uint result = Rotl(_s1 * 5, 7) * 9;
            uint t = _s1 << 9;

            _s2 ^= _s0;
            _s3 ^= _s1;
            _s1 ^= _s2;
            _s0 ^= _s3;

            _s2 ^= t;
            _s3 = Rotl(_s3, 11);

            return result;
        }

        public int Next()
        {
            // Mask out the sign bit to ensure a non-negative integer
            return (int)(NextUint() & 0x7FFFFFFF);
        }

        public long NextLong()
        {
            // Combine two 32-bit generations into one non-negative 64-bit long
            ulong high = NextUint();
            ulong low = NextUint();
            return (long)(((high << 32) | low) & 0x7FFFFFFFFFFFFFFF);
        }

        public int Next(int maxVal)
        {
            if (maxVal <= 0) return 0;
            return (int)(NextUint() % (uint)maxVal);
        }

        public int Next(int minValue, int maxValue)
        {
            if (minValue >= maxValue) return minValue;
            uint range = (uint)(maxValue - minValue);
            return minValue + (int)(NextUint() % range);
        }

        public long NextLong(long minValue, long maxValue)
        {
            if (minValue >= maxValue) return minValue;
            ulong range = (ulong)(maxValue - minValue);

            ulong high = NextUint();
            ulong low = NextUint();
            ulong combined = (high << 32) | low;

            return minValue + (long)(combined % range);
        }

        public double NextDouble()
        {
            return (NextUint() & 0xFFFFFFFF) / (double)uint.MaxValue;
        }

        private static uint Rotl(uint x, int k)
        {
            return (x << k) | (x >> (32 - k));
        }
    }
}