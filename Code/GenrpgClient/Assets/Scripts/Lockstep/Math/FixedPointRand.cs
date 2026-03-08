namespace Assets.Scripts.Lockstep.Math
{
    public class FixedPointRand
    {
        private uint _state;

        public FixedPointRand(uint seed)
        {
            // State must never be zero for Xorshift
            _state = seed == 0 ? 0xACE12481 : seed;
        }

        /// <summary>
        /// Generates the next raw pseudo-random 32-bit unsigned integer.
        /// </summary>
        public uint NextRaw()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        /// <summary>
        /// Returns a FixedPoint64 between 0 and 1.
        /// </summary>
        public FixedPoint64 NextFixed()
        {
            // We take the raw uint and map it to our 16-bit fractional space.
            // 0xFFFFFFFF is the max uint. We want 0 to 65536 (FixedPoint64.One).
            return FixedPoint64.FromRaw((long)(NextRaw() % (uint)FixedPoint64.One));
        }

        /// <summary>
        /// Returns an integer between min [inclusive] and max [exclusive].
        /// </summary>
        public int Range(int min, int max)
        {
            if (min >= max) return min;
            uint range = (uint)(max - min);
            return (int)(min + (NextRaw() % range));
        }
    }
}