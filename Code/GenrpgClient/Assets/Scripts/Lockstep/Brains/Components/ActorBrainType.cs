using Assets.Scripts.Lockstep.Brains.Constants;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Brains.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct ActorBrainType : IComponentData
    {
        public EBrainLogic Type;
    }
}
