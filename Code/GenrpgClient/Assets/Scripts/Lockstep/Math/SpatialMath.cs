using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
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
            int y = (int)(pos.Y / cellSize);

            // The core hashing formula
            return (x * P1) ^ (y * P2);
        }
    }
}
