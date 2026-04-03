namespace Assets.Scripts.Lockstep.Math
{
    namespace Assets.Scripts.Lockstep.Math
    {
        public static class SeedFactory
        {
            /// <summary>
            /// Generates a high-entropy seed for Unity.Mathematics.Random.
            /// Optimized for lockstep ECS simulations where input values are often small.
            /// </summary>
            public static uint CreateSeed(uint tick, uint systemId, uint entityId, uint worldSeed)
            {
                // 1. Initial mix of the components using XOR and offsets
                // We shift systemId and entityId to prevent direct bit-overlap 
                // with small tick numbers.
                uint h = worldSeed ^ tick;
                h ^= (systemId << 24);
                h ^= (entityId << 8);

                // 2. The "Avalanche" step
                // These constants are large primes that force bit-flipping 
                // across the entire 32-bit range.
                h ^= h >> 16;
                h *= 0x85ebca6b;
                h ^= h >> 13;
                h *= 0xc2b2ae35;
                h ^= h >> 16;

                // 3. Ensure we never return 0
                // Unity.Mathematics.Random(0) is invalid and will throw/fail.
                return h == 0 ? 0x12345678 : h;
            }
        }

        public readonly struct SeedContext
        {
            public readonly uint CurrentTick;
            public readonly uint SystemId;
            public readonly uint WorldSeed;

            public SeedContext(uint currentTick, uint worldSeed, uint systemId)
            {
                CurrentTick = currentTick;
                SystemId = systemId;
                WorldSeed = worldSeed;
            }

            // Helper method to generate the seed directly from the context
            public uint GetSeed(int entityIndex)
            {
                return SeedFactory.CreateSeed(CurrentTick, SystemId, (uint)entityIndex, WorldSeed);
            }
        }
    }
}
