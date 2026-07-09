using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Assets.Scripts.Lockstep.Math
{
    [BurstCompile]
    public static class SpatialMath
    {
        // Centralized constants to prevent desyncs
        public const int DefaultCellSize = 8;
        private const int P1 = 73856093;
        private const int P2 = 19349663;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCellHash(Vector2Fixed pos, int cellSize)
        {
            // Deterministic integer floor via fixed-point division
            int x = (int)(pos.X / cellSize);
            int z = (int)(pos.Z / cellSize);

            // The core hashing formula
            return (x * P1) ^ (z * P2);
        }
    }
}
