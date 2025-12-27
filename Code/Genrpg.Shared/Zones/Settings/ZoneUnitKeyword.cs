using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Zones.Settings
{
    public class ZoneUnitKeyword : IWeightedItem
    {
        public long UnitKeywordId { get; set; }
        public double Weight { get; set; }
    }
}


