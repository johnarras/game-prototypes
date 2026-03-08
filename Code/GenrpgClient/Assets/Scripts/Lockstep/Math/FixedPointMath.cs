using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;

namespace Assets.Scripts.Lockstep.Math
{
    [BurstCompile]
    public static class FixedPointMath
    {
        // ... (Include the SinTable array from the previous step here) ...

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 SinRaw(int deciDegree)
        {
            // 1. Handle negative angles and wrap to 0-359.9 (0-3599 deciDegrees)
            deciDegree %= 3600;
            if (deciDegree < 0) deciDegree += 3600;

            // 2. Determine Quadrant (0-3) and Angle withQuadrant (0-899)
            int quadrant = deciDegree / 900;
            int angleInQuadrant = deciDegree % 900;

            long rawValue;

            // 3. Octant Logic: If > 45 degrees, we mirror the lookup
            if (angleInQuadrant <= 450)
            {
                rawValue = FixedPointTrigLUT.SinTable[angleInQuadrant];
            }
            else
            {
                // sin(45 + x) = sin(45 - x) ... wait, that's not right.
                // It's sin(90 - x) = cos(x). Since our table is s0-45, 
                // for 45-90 we use the "mirrored" index.
                rawValue = GetCosFromSinTable(900 - angleInQuadrant);
            }

            // 4. Quadrant Signs
            // Quadrant 0 & 1: Positive Sine
            // Quadrant 2 & 3: Negative Sine
            if (quadrant >= 2) rawValue = -rawValue;

            return FixedPoint64.FromRaw(rawValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Sin(FixedPoint64 angle)
        {// 1. Wrap angle to [0, 360]
            FixedPoint64 wrappedAngle = angle % 360;
            if (wrappedAngle < 0) wrappedAngle += 360;

            // 2. Convert to DeciDegree scale (0-3599)
            // We multiply by 10 to find our index
            FixedPoint64 deciValue = wrappedAngle * 10;
            int index = (int)deciValue;
            FixedPoint64 fraction = deciValue - index;

            // 3. Perform Lookup with Linear Interpolation
            // Sin(a + t) ≈ Sin(a) + t * (Sin(a+1) - Sin(a))
            FixedPoint64 valA = SinRaw(index);
            FixedPoint64 valB = SinRaw(index + 1);

            return valA + (fraction * (valB - valA));

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Cos(FixedPoint64 angle)
        {
            // Cosine is just Sine shifted by 90 degrees
            return Sin(angle + 90);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetCosFromSinTable(int index)
        {
            // Note: In the 0-45 range, the "Sine" of the mirrored angle 
            // is essentially providing the value we need.
            // However, to be pedantic: for 45-90, sin(theta) = cos(90-theta).
            return FixedPointTrigLUT.SinTable[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Sqrt(FixedPoint64 value)
        {
            // Safe check for Burst: No exceptions!
            if (value.RawValue <= 0) return FixedPoint64.FromRaw(0);

            // Newton-Raphson without overflow risk
            long s = value.RawValue;
            long x;

            // Initial guess: more efficient for fixed point
            if (s > (long)1 << 32) x = (long)1 << 24;
            else x = (long)1 << 12;

            for (int i = 0; i < 12; i++)
            {
                // x = (x + s/x) / 2, with proper fixed-point shifting
                // We use double to prevent overflow during the intermediate multiply
                // Or use BigInt/Int128. For 16-bit shift, this works:
                x = (x + (s << FixedPoint64.Shift) / x) >> 1;
            }

            return FixedPoint64.FromRaw(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 Atan2(FixedPoint64 y, FixedPoint64 x)
        {
            if (x.RawValue == 0)
            {
                FixedPoint64.FromInt(y.RawValue > 0 ? 90 : y.RawValue < 0 ? 270 : 0);
            }

            // Use absolute values to find the octant
            FixedPoint64 absY = y.RawValue < 0 ? FixedPoint64.FromRaw(-y.RawValue) : y;
            FixedPoint64 absX = x.RawValue < 0 ? FixedPoint64.FromRaw(-x.RawValue) : x;

            FixedPoint64 angle;

            // We want the ratio to be between 0 and 1 for the approximation to work
            if (absX >= absY)
            {
                // Ratio z = y/x
                FixedPoint64 z = y / x;
                angle = FastAtanDegrees(z);
            }
            else
            {
                // Ratio z = x/y
                FixedPoint64 z = x / y;
                // atan(y/x) = 90 - atan(x/y)
                angle = 90 - FastAtanDegrees(z);
            }

            // Adjust for the negative X quadrants (II and III)
            if (x.RawValue < 0)
            {
                angle += 180;
            }

            // Final wrap to 0..360
            angle %= 360;
            if (angle < 0) angle += 360;

            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedPoint64 FastAtanDegrees(FixedPoint64 z)
        {
            // Approximation for Atan(z) in degrees for z in [-1, 1]
            // Formula: (45 * z) - (z * (absZ - 1) * (14 + 4 * absZ))
            // This is a common curve-fit for fixed-point degrees.
            FixedPoint64 absZ = z.RawValue < 0 ? FixedPoint64.FromRaw(-z.RawValue) : z;

            // Using your overloaded operators:
            return (z*45) - (z * (absZ - 1) * (14 + (absZ * 4)));
        }
    }
}