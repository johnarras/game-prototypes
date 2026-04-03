// ... existing imports
using Assets.Scripts.Lockstep.Actors.Components;
using Assets.Scripts.Lockstep.Collisions.Components;
using Assets.Scripts.Lockstep.Collisions.Constants;
using Assets.Scripts.Lockstep.Factions.Components;
using Assets.Scripts.Lockstep.Math;
using Assets.Scripts.Lockstep.Systems.Constants;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Systems
{
    // Added ShapeType to the entry for faster narrow-phase branching
    public struct SpatialEntry
    {
        public Entity Actor;
        public Vector2Fixed Pos;
        public int FactionId;
        public CollisionShape Shape;
    }
    public struct SpatialHashSingleton : IComponentData
    {
        public NativeParallelMultiHashMap<int, SpatialEntry> SpatialMap;
        public int CellSize;
    }

    [BurstCompile]
    public partial struct BuildSpatialHashSystem : ISeededSystem, ISystem
    {

        public uint SystemId => SystemSeeds.BuildSpatialHash;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {// 1. Allocate the hash map
            NativeParallelMultiHashMap<int, SpatialEntry> map = new NativeParallelMultiHashMap<int, SpatialEntry>(1024, Allocator.Persistent);

            // 2. Create an entity to hold the singleton component
            Entity entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new SpatialHashSingleton
            {
                SpatialMap = map,
                CellSize = SpatialMath.DefaultCellSize,
            });
        }

        public void OnDestroy(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<SpatialHashSingleton>())
            {
                var singleton = SystemAPI.GetSingleton<SpatialHashSingleton>();
                if (singleton.SpatialMap.IsCreated)
                {
                    singleton.SpatialMap.Dispose();
                }
            }
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var singleton = SystemAPI.GetSingletonRW<SpatialHashSingleton>();

            // 1. Clear the map from the previous tick
            singleton.ValueRW.SpatialMap.Clear();

            // 2. Schedule the build job in parallel
            var buildJob = new BuildSpatialHashJob
            {
                SpatialMap = singleton.ValueRW.SpatialMap.AsParallelWriter(),
                CellSize = singleton.ValueRO.CellSize
            };

            state.Dependency = buildJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct BuildSpatialHashJob : IJobEntity
        {
            // We use the ParallelWriter to allow multiple threads to add entries simultaneously
            public NativeParallelMultiHashMap<int, SpatialEntry>.ParallelWriter SpatialMap;
            public int CellSize;

            public void Execute(Entity entity, in ActorPosition pos, in ActorFaction faction, in CollisionShape shape)
            {
                // Calculate the bounding box in world units
                FixedPoint64 range = shape.ShapeType == ECollisionShapeType.Circle
                    ? shape.Extents.X
                    : FixedPoint64.Max(shape.Extents.X, shape.Extents.Y);

                // Convert world bounds to grid cell bounds
                int minX = (int)((pos.Pos.X - range) / CellSize);
                int maxX = (int)((pos.Pos.X + range) / CellSize);
                int minY = (int)((pos.Pos.Y - range) / CellSize);
                int maxY = (int)((pos.Pos.Y + range) / CellSize);

                // Add the entity to every cell it overlaps
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        int hash = SpatialMath.GetCellHash(new Vector2Fixed(x * CellSize, y * CellSize), CellSize); // Using your utility

                        SpatialMap.Add(hash, new SpatialEntry
                        {
                            Actor = entity,
                            Pos = pos.Pos,
                            FactionId = faction.FactionId,
                            Shape = shape
                        });
                    }
                }
            }
        }
    }
}