using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Constants;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Zones.WorldData
{

    public class Zone : BaseWorldData, IIndexedGameItem, IMapOwnerId
    {
        public override string Id { get; set; }
        public string OwnerId { get; set; }
        public string MapId { get; set; }
        public long IdKey { get; set; }
        public long ZoneTypeId { get; set; }


        public string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public long Seed { get; set; }


        public void SetTextureTypeId(long textureChannel, long textureTypeId)
        {
            if (textureChannel == TerrainTexChannels.Base)
            {
                BaseTextureTypeId = textureTypeId;
            }
            else if (textureChannel == TerrainTexChannels.Dirt)
            {
                DirtTextureTypeId = textureTypeId;
            }
            else if (textureChannel == TerrainTexChannels.Road)
            {
                RoadTextureTypeId = textureTypeId;
            }
            else if (textureChannel == TerrainTexChannels.Steep)
            {
                SteepTextureTypeId = textureTypeId;
            }
        }

        public long BaseTextureTypeId { get; set; }
        public long DirtTextureTypeId { get; set; }
        public long RoadTextureTypeId { get; set; }
        public long SteepTextureTypeId { get; set; }

        public long Level { get; set; }

        public string Art { get; set; }

        public int XMin { get; set; }
        public int ZMin { get; set; }
        public int XMax { get; set; }
        public int ZMax { get; set; }

        public List<Location> Locations { get; set; }
        public List<ZoneUnitStatus> Units { get; set; }
        public List<ZonePlantType> PlantTypes { get; set; }

        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }
        public Zone()
        {
            Locations = new List<Location>();
            CleanData();
        }

        public void CleanForClient()
        {
            CleanData();
        }

        private void CleanData()
        {
            Units = new List<ZoneUnitStatus>();
            foreach (Location loc in Locations)
            {
                loc.CleanForClient();
            }


        }

        public Location GetLocation(string id)
        {
            if (Locations == null)
            {
                return null;
            }

            for (int l = 0; l < Locations.Count; l++)
            {
                if (Locations[l].Id == id)
                {
                    return Locations[l];
                }
            }
            return null;
        }

        public ZoneUnitStatus GetUnit(long unitTypeId)
        {
            if (Units == null)
            {
                Units = new List<ZoneUnitStatus>();
            }

            ZoneUnitStatus unit = Units.FirstOrDefault(x => x.UnitTypeId == unitTypeId);
            if (unit != null)
            {
                return unit;
            }
            ZoneUnitStatus zc = new ZoneUnitStatus();
            zc.UnitTypeId = unitTypeId;
            Units.Add(zc);
            return zc;

        }

        public ZonePlantType GetPlant(int plantTypeId)
        {
            if (PlantTypes == null)
            {
                return null;
            }

            for (int p = 0; p < PlantTypes.Count; p++)
            {
                if (PlantTypes[p].PlantTypeId == plantTypeId)
                {
                    return PlantTypes[p];
                }
            }
            return null;
        }

        public string GetTitle()
        {
            return Name + " [#" + IdKey + "]";
        }

        public long GetFinalUnitLevel(IRandom rand, float x, float z, long startLevel, long mapMaxLevel)
        {
            float dmaxx = XMax - x;
            float dmaxz = ZMax - z;
            float dminx = XMin - x;
            float dminz = ZMin - z;

            float distFromMax = dmaxx * dmaxx + dmaxz * dmaxz;
            float distFromMin = dminx * dminx + dminz * dminz;

            float totalDist = distFromMax + distFromMin;

            if (totalDist > 1)
            {
                float minPct = distFromMin / totalDist;
                float maxPct = 1 - minPct;

                int levelOffset = (int)(4 * (maxPct - minPct) + rand.Next(-1, 1));
                return MathUtil.Clamp(1, startLevel + levelOffset, mapMaxLevel);
            }
            return 1;
        }


        public long GetTerrainTextureByIndex(int index)
        {
            if (index == 1)
            {
                return DirtTextureTypeId;
            }
            else if (index == 2)
            {
                return RoadTextureTypeId;
            }
            else if (index == 3)
            {
                return SteepTextureTypeId;
            }
            return BaseTextureTypeId;
        }
    }
}


