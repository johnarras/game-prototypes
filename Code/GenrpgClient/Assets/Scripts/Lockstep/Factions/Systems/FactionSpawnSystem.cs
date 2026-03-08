using Assets.Scripts.Lockstep.Collisions.Components;
using Assets.Scripts.Lockstep.Collisions.Constants;
using Assets.Scripts.Lockstep.Factions.Components;
using Assets.Scripts.Lockstep.Game;
using Assets.Scripts.Lockstep.Maps.Components;
using Assets.Scripts.Lockstep.Math;
using Assets.Scripts.Lockstep.Math.Assets.Scripts.Lockstep.Math;
using Assets.Scripts.Lockstep.Spawns;
using Assets.Scripts.Lockstep.Systems.Constants;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Systems
{
    [DisableAutoCreation]
    [BurstCompile]
    public partial struct FactionSpawnSystem : ISeededSystem, ISystem
    {
        // Ensure this is defined in your LockstepSystemIds registry
        public uint SystemId => SystemSeeds.FactionSpawns;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 1. Data Retrieval
            LockstepGlobalState globalState = SystemAPI.GetSingleton<LockstepGlobalState>();

            // We use BeginSimulation so these requests are processed before logic systems run
            BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton =
                SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

            EntityCommandBuffer.ParallelWriter ecb =
                ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();


            // 2. Map Collection
            // Collect all entities that represent a Map into a NativeArray
            EntityQuery mapQuery = SystemAPI.QueryBuilder().WithAll<ActiveMap>().Build();

            // Allocator.TempJob is vital here: it lives long enough for the job to finish, 
            // but is faster than Persistent memory.
            NativeArray<Entity> mapEntities = mapQuery.ToEntityArray(Allocator.TempJob);


            // 3. The Job Object Declaration
            // We explicitly create the instance of our struct here
            FactionParallelSpawnJob spawnJob = new FactionParallelSpawnJob
            {
                Context = new SeedContext(globalState.CurrentTick, globalState.WorldSeed, SystemId),
                ECB = ecb,
                MapEntities = mapEntities
            };

            // 4. Scheduling and Dependencies
            // state.Dependency is the "System-wide Baton"
            // We tell the job to start only when previous systems are done.
            state.Dependency = spawnJob.ScheduleParallel(state.Dependency);

            // 5. Deallocation
            // This tells Unity: "Once the job stored in state.Dependency is finished, 
            // immediately free the memory for mapEntities."
            state.Dependency = mapEntities.Dispose(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct FactionParallelSpawnJob : IJobEntity
    {
        public SeedContext Context;
        public EntityCommandBuffer.ParallelWriter ECB;

        // Use [ReadOnly] for performance and thread-safety
        [ReadOnly] public NativeArray<Entity> MapEntities;

        public void Execute([EntityIndexInQuery] int entityIndex, in FactionData faction)
        {
            uint seed = Context.GetSeed(entityIndex);
            var rand = new Unity.Mathematics.Random(seed == 0 ? 1 : seed);

            if (Context.CurrentTick % faction.SpawnInterval == 0)
            {
                if (rand.NextInt(0, 100) < faction.SpawnChance)
                {
                    // Logic: Pick a random map from the available ones
                    // Note: We check if MapEntities is empty to avoid errors
                    if (MapEntities.Length > 0)
                    {
                        int randomMapIndex = rand.NextInt(0, MapEntities.Length);
                        Entity chosenMapEntity = MapEntities[randomMapIndex];

                        Entity pending = ECB.CreateEntity(entityIndex);
                        ECB.AddComponent(entityIndex, pending, new SpawnRequest
                        {
                            FactionId = faction.FactionId,
                            MapEntity = chosenMapEntity, // Now passing the Entity!
                            Position = new Vector2Fixed(3, 3), // Or random pos
                            Angle = FixedPoint64.FromFloat(0),
                            Shape = new CollisionShape()
                            {
                                Extents = new Vector2Fixed(1, 1),
                                ShapeType = ECollisionShapeType.Circle,
                            }
                        });
                    }
                }
            }
        }
    }
}