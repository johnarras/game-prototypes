using OxDb.Client.Lockstep.Actors.Components;
using OxDb.Client.Lockstep.Game;
using OxDb.Client.Lockstep.Systems;
using OxDb.Client.Lockstep.Systems.Constants;
using Unity.Burst;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Spawns.Systems
{
    [BurstCompile]
    [DisableAutoCreation]
    public partial struct ActorDespawnSystem : ISystem, ISeededSystem
    {
        public uint SystemId => SystemSeeds.ActorDespawn;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var global = SystemAPI.GetSingleton<LockstepGlobalState>();
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            // Path A: Handle Lifetime Expiry (TTL)
            state.Dependency = new LifetimeJob
            {
                CurrentTick = global.CurrentTick,
                ECB = ecb
            }.ScheduleParallel(state.Dependency);

            // Path B: Handle Explicit Destruction (e.g. Health <= 0)
            // We use a query for entities that have a specific 'PendingRemoval' tag
            state.Dependency = new ExplicitDespawnJob
            {
                ECB = ecb
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct LifetimeJob : IJobEntity
        {
            public uint CurrentTick;
            public EntityCommandBuffer.ParallelWriter ECB;

            // Note: Using EntityIndexInQuery for a more unique sort key
            void Execute([EntityIndexInQuery] int sortKey, Entity entity, in Lifetime lifetime)
            {
                if (CurrentTick >= lifetime.ExpiryTick)
                {
                    ECB.DestroyEntity(sortKey, entity);
                }
            }
        }

        [BurstCompile]
        // This job only targets things that were manually flagged for removal
        public partial struct ExplicitDespawnJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute([EntityIndexInQuery] int sortKey, Entity entity, in PendingRemoval pending)
            {
                ECB.DestroyEntity(sortKey, entity);
            }
        }
    }
}
