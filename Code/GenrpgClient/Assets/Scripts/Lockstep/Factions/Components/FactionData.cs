using Assets.Scripts.Lockstep.Math;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Factions.Components
{
    public struct FactionData : IComponentData
    {
        public int FactionId;
        public uint SpawnInterval;      // How often they try to spawn (in ticks)
        public int SpawnChance;         // 0-100 percentage
        public FixedPoint64 UnitSpeed;  // The base speed for units of this faction
                                        // Add other stats here...
    }
}
