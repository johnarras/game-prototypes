using Assets.Scripts.Lockstep.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Lockstep.Factions.Setup
{
    public class FactionConfig
    {
        public int FactionId { get; set; }
        public uint SpawnInterval { get; set; }
        public int SpawnPercent { get; set; }

        public FixedPoint64 UnitSpeed { get; set; }
    }
}
