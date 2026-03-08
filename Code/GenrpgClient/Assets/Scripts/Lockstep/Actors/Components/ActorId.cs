using Unity.Collections;
using Unity.Entities;

[GenerateTestsForBurstCompatibility]
public struct ActorId : IComponentData
{
    public uint Value;
}