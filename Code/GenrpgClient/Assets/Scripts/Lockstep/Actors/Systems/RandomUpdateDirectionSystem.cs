using OxDb.Client.Lockstep.Actors.Components;
using OxDb.Client.Lockstep.Maps.Components;
using OxDb.Client.Lockstep.Systems.Constants;
using Unity.Burst;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Systems
{
    [DisableAutoCreation]
    [BurstCompile]
    public partial struct RandomUpdateDirectionSystem : ISeededSystem, ISystem
    {
        public uint SystemId => SystemSeeds.RandomUpdateDirection;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (velocity, map) in
                     SystemAPI.Query<RefRW<ActorRotation>, RefRO<ActorMap>>())
            {
            }
        }
    }
}
