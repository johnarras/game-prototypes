using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.ProcGen.Entities
{
    public class GenZone
    {
        public long IdKey { get; set; }
        public float RoadDipScale { get; set; }

        public float RoadDirtScale { get; set; }

        public float GrassDensity { get; set; }

        public float GrassFreq { get; set; }

        public float TreeDensity { get; set; }

        public float TreeFreq { get; set; }

        public float BushDensity { get; set; }

        public float BushFreq { get; set; }

        public float RockDensity { get; set; }

        public float RockFreq { get; set; }

        public float DetailFreq { get; set; }

        public float DetailAmp { get; set; }

        public float SpreadChance { get; set; }

        public List<ZoneRockType> RockTypes { get; set; } = new List<ZoneRockType>();
        public List<ZoneTreeType> TreeTypes { get; set; } = new List<ZoneTreeType>();

        public List<ZoneRelation> ZonesNearLevel { get; set; } = new List<ZoneRelation>();
        public List<ZoneRelation> ZonesNearPos { get; set; } = new List<ZoneRelation>();


        public ZoneTreeType GetTree(int treeTypeId)
        {
            if (TreeTypes == null)
            {
                return null;
            }

            for (int t = 0; t < TreeTypes.Count; t++)
            {
                if (TreeTypes[t].TreeTypeId == treeTypeId)
                {
                    return TreeTypes[t];
                }
            }
            return null;
        }

        public void AddNearbyZone(Zone zone, float distance)
        {

            if (zone == null || zone.ZoneTypeId < SharedMapConstants.MapZoneStartId)
            {
                return;
            }

            if (ZonesNearPos == null)
            {
                ZonesNearPos = new List<ZoneRelation>();
            }

            ZoneRelation currNearby = ZonesNearPos.FirstOrDefault(x => x.ZoneId == zone.ZoneTypeId);

            if (currNearby != null)
            {
                return;
            }


            ZonesNearPos.Add(new ZoneRelation() { ZoneId = zone.IdKey, Offset = distance });
        }
    }
}


