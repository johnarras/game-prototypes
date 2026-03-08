using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Factions.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct ActorFaction : IComponentData
    {
        public int FactionId;
    }
}
