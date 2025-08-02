using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Zones.Settings
{
    [MessagePackObject]
    public class ZoneUnitKeyword : IWeightedItem
    {
        [Key(0)] public long UnitKeywordId { get; set; }
        [Key(1)] public double Weight { get; set; }
    }
}
