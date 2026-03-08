using Assets.Scripts.Lockstep.Math;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Actors.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct ActorSpeed : IComponentData
    {
        public FixedPoint64 Speed;
    }
}
