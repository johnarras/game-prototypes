using OxDb.Client.Lockstep.Actors.Factory;
using OxDb.Client.Lockstep.Game;
using OxDb.Client.Lockstep.Spawns;
using Unity.Burst;
using Unity.Entities;

[DisableAutoCreation]
[BurstCompile]
public partial struct ActorSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. Get the Singleton Entity and State
        Entity globalEntity = SystemAPI.GetSingletonEntity<LockstepGlobalState>();
        LockstepGlobalState globalState = SystemAPI.GetSingleton<LockstepGlobalState>();

        // 2. Access the deterministic buffer
        DynamicBuffer<SpawnRequestBuffer> spawnQueue = state.EntityManager.GetBuffer<SpawnRequestBuffer>(globalEntity);

        if (spawnQueue.IsEmpty)
        {
            return;
        }

        // 3. Prepare the Command Buffer
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        EntityCommandBuffer.ParallelWriter parallelEcb = ecb.AsParallelWriter();

        // 4. Process the sorted buffer
        uint currentId = globalState.NextActorId;
        uint currentTick = globalState.CurrentTick;

        for (int i = 0; i < spawnQueue.Length; i++)
        {
            // Because the buffer was sorted by ActorId in the previous systems,
            // 'i' is now a deterministic sequence for ID assignment.
            uint assignedId = currentId + (uint)i;
            SpawnRequest request = spawnQueue[i].Value;

            // Execute the factory logic using 'i' as the sortKey
            ActorFactory.ExecuteSpawn(i, Entity.Null, parallelEcb, request, assignedId, currentTick);
        }

        // 5. Finalize State
        globalState.NextActorId += (uint)spawnQueue.Length;
        SystemAPI.SetSingleton(globalState);

        // ClearFullCell for the next tick
        spawnQueue.Clear();
    }
    [InternalBufferCapacity(16)] // Initial memory allocation for 16 requests before heap expansion
    public struct SpawnRequestBuffer : IBufferElementData
    {
        public SpawnRequest Value;
    }
}