using OxDb.Client.Lockstep.Brains.Constants;
using Unity.Collections;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Brains.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct ActorBrainType : IComponentData
    {
        public EBrainLogic Type;
    }
}
