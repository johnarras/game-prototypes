using OxDb.Client.Lockstep.Math;
using System.Runtime.InteropServices;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Projectiles.Components
{
    public enum EProjectileTypes : byte
    {
        Linear,
        Homing,
        Orbit,
        Boomerang,
        Laser
    }

    public struct ProjectileData : IComponentData
    {
        // Deterministic Movement
        public Vector2Fixed Velocity;
        public FixedPoint64 Acceleration;
        public FixedPoint64 RotationSpeed;

        // Logic State
        public EProjectileTypes ProjectileType;
        public Entity TargetEntity;
        public Vector2Fixed Origin;
        public FixedPoint64 Range;

        // Lifecycle
        public FixedPoint64 TimeAlive;
        public FixedPoint64 MaxLifetime;

        [MarshalAs(UnmanagedType.U1)] // Explicitly marshal as a 1-byte boolean (or U4 for 4-byte)
        public bool IsReturning;
    }
}
