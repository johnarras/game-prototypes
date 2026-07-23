using OxDb.Client.Lockstep.Actors.Constants;
using OxDb.Client.Lockstep.Buildings.Spawns;
using OxDb.Client.Lockstep.Collisions.Components;
using OxDb.Client.Lockstep.Math;
using OxDb.Client.Lockstep.Projectiles.Spawns;
using OxDb.Client.Lockstep.Units.Spawns;
using Unity.Collections;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Spawns
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
