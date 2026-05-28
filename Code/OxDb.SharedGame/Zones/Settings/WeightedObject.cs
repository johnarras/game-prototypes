using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.SharedGame.Zones.Settings
{
    public class WeightedObject : IWeightedItem
    {
        public double Weight { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
    }
}
