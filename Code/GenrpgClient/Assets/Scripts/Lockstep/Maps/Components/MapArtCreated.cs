
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Maps.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct MapArtCreated : IComponentData
    {
        // Tag components are usually empty; their presence on 
        // an entity is the data itself.
    }
}
