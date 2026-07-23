using Unity.Collections;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Factions.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct ActorFaction : IComponentData
    {
        public int FactionId;
    }
}
