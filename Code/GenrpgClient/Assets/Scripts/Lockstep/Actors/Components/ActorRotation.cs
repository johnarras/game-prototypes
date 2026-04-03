using Assets.Scripts.Lockstep.Math;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Actors.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct ActorRotation : IComponentData
    {
        public FixedPoint64 Angle;
    }
}
