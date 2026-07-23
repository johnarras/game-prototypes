using OxDb.Client.Lockstep.Math;
using Unity.Collections;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Actors.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct ActorSpeed : IComponentData
    {
        public FixedPoint64 Speed;
    }
}
