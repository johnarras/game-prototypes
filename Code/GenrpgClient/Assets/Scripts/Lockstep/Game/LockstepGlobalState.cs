using Assets.Scripts.Lockstep.Math;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Game
{
    [GenerateTestsForBurstCompatibility]
    public struct LockstepGlobalState : IComponentData
    {
        public uint WorldSeed;
        public uint CurrentTick;
        public FixedPoint64 DeltaTime;
        public uint NextActorId;
    }
}
