using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Zones.Settings
{
    public class ZoneUnitSpawn : IWeightedItem
    {
        public long UnitTypeId { get; set; }
        public double Weight { get; set; }
    }
}


