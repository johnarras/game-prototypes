using System.Runtime.InteropServices;
using Unity.Burst;

namespace Assets.Scripts.Lockstep.Math
{
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2Fixed
    {
        public FixedPoint64 X;
        public FixedPoint64 Y;

        public static readonly Vector2Fixed Zero = new Vector2Fixed(0, 0);

        public Vector2Fixed(FixedPoint64 x, FixedPoint64 y)
        {
            X = x;
            Y = y;
        }

        // Convenience constructor for raw longs
        public Vector2Fixed(long rawX, long rawY)
        {
            X = FixedPoint64.FromRaw(rawX);
            Y = FixedPoint64.FromRaw(rawY);
        }

        public static Vector2Fixed operator +(Vector2Fixed a, Vector2Fixed b)
            => new Vector2Fixed(a.X + b.X, a.Y + b.Y);

        public static Vector2Fixed operator -(Vector2Fixed a, Vector2Fixed b)
            => new Vector2Fixed(a.X - b.X, a.Y - b.Y);

        public static Vector2Fixed operator *(Vector2Fixed a, FixedPoint64 b)
            => new Vector2Fixed(a.X * b, a.Y * b);

        public static Vector2Fixed operator /(Vector2Fixed a, FixedPoint64 b) => new Vector2Fixed(a.X/b,a.Y/b);

        public FixedPoint64 Magnitude => FixedPointMath.Sqrt((X * X) + (Y * Y));

        public override string ToString() => $"({X}, {Y})";
    }
}