using Unity.Collections;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Maps.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct ActorMap : IComponentData
    {
        public Entity MapEntity;
    }
}
