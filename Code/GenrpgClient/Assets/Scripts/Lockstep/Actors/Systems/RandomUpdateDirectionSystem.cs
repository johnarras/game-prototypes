using Assets.Scripts.Lockstep.Actors.Components;
using Assets.Scripts.Lockstep.Maps.Components;
using Assets.Scripts.Lockstep.Systems.Constants;
using Unity.Burst;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Systems
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
