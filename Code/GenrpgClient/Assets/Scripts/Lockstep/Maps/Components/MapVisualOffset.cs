using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Lockstep.Maps.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct MapVisualOffset : IComponentData
    {
        public float3 Value; // The (X, Y, Z) shift for this specific map instance
    }
}
