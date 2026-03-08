using Assets.Scripts.Buildings;
using Assets.Scripts.Lockstep.Actors.Constants;
using Assets.Scripts.Lockstep.Buildings.Spawns;
using Assets.Scripts.Lockstep.Collisions.Components;
using Assets.Scripts.Lockstep.Math;
using Assets.Scripts.Lockstep.Projectiles.Spawns;
using Assets.Scripts.Lockstep.Units.Spawns;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Spawns
{
    [GenerateTestsForBurstCompatibility]
    public struct SpawnRequest : IComponentData
    {
        // --- Base Data (Everything has these) ---
        public EActorCategories Category;
        public int FactionId;
        public Entity MapEntity;
        public Vector2Fixed Position;
        public FixedPoint64 Angle;
        public uint TTLTicks;

        public CollisionShape Shape;

        // --- Specific Data Buckets ---
        public UnitSpawnData UnitData;
        public BuildingSpawnData BuildingData;
        public ProjectileSpawnData ProjectileData;
    }
}
