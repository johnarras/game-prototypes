using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace OxDb.Client.Lockstep.Maps.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct MapVisualOffset : IComponentData
    {
        public float3 Value; // The (X, Z, Z) shift for this specific map instance
    }
}
