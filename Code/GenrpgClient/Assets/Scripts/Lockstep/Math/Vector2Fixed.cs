using System.Runtime.InteropServices;
using Unity.Burst;

namespace Assets.Scripts.Lockstep.Math
{
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2Fixed
    {
        public FixedPoint64 X;
        public FixedPoint64 Z;

        public static readonly Vector2Fixed Zero = new Vector2Fixed(0, 0);

        public Vector2Fixed(FixedPoint64 x, FixedPoint64 z)
        {
            X = x;
            Z = z;
        }

        // Convenience constructor for raw longs
        public Vector2Fixed(long rawX, long rawZ)
        {
            X = FixedPoint64.FromRaw(rawX);
            Z = FixedPoint64.FromRaw(rawZ);
        }

        public static Vector2Fixed operator +(Vector2Fixed a, Vector2Fixed b)
            => new Vector2Fixed(a.X + b.X, a.Z + b.Z);

        public static Vector2Fixed operator -(Vector2Fixed a, Vector2Fixed b)
            => new Vector2Fixed(a.X - b.X, a.Z - b.Z);

        public static Vector2Fixed operator *(Vector2Fixed a, FixedPoint64 b)
            => new Vector2Fixed(a.X * b, a.Z * b);

        public static Vector2Fixed operator /(Vector2Fixed a, FixedPoint64 b) => new Vector2Fixed(a.X / b, a.Z / b);

        public FixedPoint64 Magnitude => FixedPointMath.Sqrt((X * X) + (Z * Z));

        public override string ToString() => $"({X}, {Z})";
    }
}