using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Actors.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct Lifetime : IComponentData
    {
        // The specific tick number when this entity should die
        public uint ExpiryTick;
    }
}
