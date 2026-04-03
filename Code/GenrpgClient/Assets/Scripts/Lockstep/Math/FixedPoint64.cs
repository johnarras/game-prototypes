using System.Runtime.InteropServices;
using Unity.Burst;

namespace Assets.Scripts.Lockstep.Math
{
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public struct FixedPoint64
    {
        // 16 bits for fractional part means a scaling factor of 2^16
        public const int Shift = 16;
        public const long One = 1L << Shift;
        public const long Half = 1L << (Shift - 1);

        public long RawValue;

        // Internal constructor for direct raw assignment
        private FixedPoint64(long rawValue)
        {
            RawValue = rawValue;
        }

        public static FixedPoint64 FromRaw(long rawValue)
        {
            return new FixedPoint64(rawValue);
        }

        // Factory method for creating from integers
        public static FixedPoint64 FromInt(int value)
        {
            return new FixedPoint64((long)value << Shift);
        }

        // Factory method for creating from floats (non-deterministic if used at runtime, 
        // but fine for initial conversion or editor-side setup)
        public static FixedPoint64 FromFloat(float value)
        {
            return new FixedPoint64((long)(value * One));
        }

        #region Operators
        public static FixedPoint64 operator *(FixedPoint64 a, int b)
        {
            return FromRaw(a.RawValue * b);
        }

        public static FixedPoint64 operator +(FixedPoint64 a, int b)
        {
            // Shift b to match the fixed-point scale before adding
            return FromRaw(a.RawValue + ((long)b << Shift));
        }

        public static FixedPoint64 Max(FixedPoint64 a, FixedPoint64 b)
        {
            return a < b ? b : a;
        }

        public static FixedPoint64 Min(FixedPoint64 a, FixedPoint64 b)
        {
            return a > b ? b : a;
        }

        public static FixedPoint64 Max(FixedPoint64 a, int b)
        {
            return a < b ? FixedPoint64.FromInt(b) : a;
        }

        public static FixedPoint64 Min(FixedPoint64 a, int b)
        {
            return a < b ? a : FixedPoint64.FromInt(b);
        }

        // It's also good practice to define the commutative version (int + FixedPoint)
        public static FixedPoint64 operator +(int a, FixedPoint64 b)
        {
            return FromRaw(((long)a << Shift) + b.RawValue);
        }
        public static FixedPoint64 operator -(FixedPoint64 a, int b) =>
        FromRaw(a.RawValue - ((long)b << Shift));

        // Int - FixedPoint
        public static FixedPoint64 operator -(int a, FixedPoint64 b) =>
            FromRaw(((long)a << Shift) - b.RawValue);

        public static FixedPoint64 operator -(FixedPoint64 a)
        {
            return FromRaw(-a.RawValue);
        }


        public static FixedPoint64 operator +(FixedPoint64 a, FixedPoint64 b)
            => new FixedPoint64(a.RawValue + b.RawValue);

        public static FixedPoint64 operator -(FixedPoint64 a, FixedPoint64 b)
            => new FixedPoint64(a.RawValue - b.RawValue);

        // Divided by an integer scalar
        public static FixedPoint64 operator /(FixedPoint64 a, int b)
        {
            if (b == 0) throw new System.DivideByZeroException();
            return FromRaw(a.RawValue / b);
        }
        public static FixedPoint64 operator *(FixedPoint64 a, FixedPoint64 b)
        {
            // Multiplication requires shifting back down to maintain the 16-bit fractional part
            // We use (a * b) / 2^16. Note: This can overflow if both are very large.
            return new FixedPoint64((a.RawValue * b.RawValue) >> Shift);
        }

        public static FixedPoint64 operator /(FixedPoint64 a, FixedPoint64 b)
        {
            // Division requires shifting up first to maintain precision
            return new FixedPoint64((a.RawValue << Shift) / b.RawValue);
        }
        public static FixedPoint64 operator %(FixedPoint64 a, FixedPoint64 b)
        {
            if (b.RawValue == 0) throw new System.DivideByZeroException();
            // Since both have the same fractional scale, we can modulo the raw values directly
            return FromRaw(a.RawValue % b.RawValue);
        }
        public static explicit operator int(FixedPoint64 value)
        {
            return (int)(value.RawValue >> Shift);
        }

        // Explicit cast to long (if you need the whole number part)
        public static explicit operator long(FixedPoint64 value)
        {
            return value.RawValue >> Shift;
        }
        public static bool operator <(FixedPoint64 a, FixedPoint64 b) => a.RawValue < b.RawValue;
        public static bool operator <=(FixedPoint64 a, FixedPoint64 b) => a.RawValue <= b.RawValue;
        public static bool operator >(FixedPoint64 a, FixedPoint64 b) => a.RawValue > b.RawValue;
        public static bool operator >=(FixedPoint64 a, FixedPoint64 b) => a.RawValue >= b.RawValue;

        // FixedPoint vs Int
        public static bool operator <(FixedPoint64 a, int b) => a.RawValue < ((long)b << Shift);
        public static bool operator <=(FixedPoint64 a, int b) => a.RawValue <= ((long)b << Shift);
        public static bool operator >(FixedPoint64 a, int b) => a.RawValue > ((long)b << Shift);
        public static bool operator >=(FixedPoint64 a, int b) => a.RawValue >= ((long)b << Shift);

        // Int vs FixedPoint (Commutative)
        public static bool operator <(int a, FixedPoint64 b) => ((long)a << Shift) < b.RawValue;
        public static bool operator <=(int a, FixedPoint64 b) => ((long)a << Shift) <= b.RawValue;
        public static bool operator >(int a, FixedPoint64 b) => ((long)a << Shift) > b.RawValue;
        public static bool operator >=(int a, FixedPoint64 b) => ((long)a << Shift) >= b.RawValue;

        // Helper to modulo by an integer for convenience (e.g., angle % 360)
        public static FixedPoint64 operator %(FixedPoint64 a, int b)
        {
            long bRaw = (long)b << Shift;
            return FromRaw(a.RawValue % bRaw);
        }

        public static bool operator ==(FixedPoint64 a, FixedPoint64 b) => a.RawValue == b.RawValue;
        public static bool operator !=(FixedPoint64 a, FixedPoint64 b) => a.RawValue != b.RawValue;

        #endregion

        #region Conversions

        public float ToFloat() => (float)RawValue / One;
        public int ToInt() => (int)(RawValue >> Shift);

        public override bool Equals(object obj) => obj is FixedPoint64 other && this == other;
        public override int GetHashCode() => RawValue.GetHashCode();
        public override string ToString() => ToFloat().ToString();

        #endregion
    }
}