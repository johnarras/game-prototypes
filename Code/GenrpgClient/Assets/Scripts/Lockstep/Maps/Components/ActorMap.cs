using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Maps.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct ActorMap : IComponentData
    {
        public Entity MapEntity;
    }
}
