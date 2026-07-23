using OxDb.SharedCore.MapServer.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Zones.Entities;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.ProcGen.Entities
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

        public List<WeightedEntity> Props { get; set; } = new List<WeightedEntity>(); 

        public List<WeightedEntity> GetPropsOfType(long entityTypeId)
        {
            return Props.Where(x=>x.EntityTypeId == entityTypeId).ToList();
        }

        public List<ZoneRelation> ZonesNearLevel { get; set; } = new List<ZoneRelation>();
        public List<ZoneRelation> ZonesNearPos { get; set; } = new List<ZoneRelation>();


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


