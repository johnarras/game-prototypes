using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Interfaces;
using OxDb.SharedGame.Names.Settings;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.Zones.Entities;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Zones.Settings
{
    public class ZoneType : ChildSettings, IIndexedGameItem, IMusicRegion
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public int MinLevel { get; set; }

        public float GenChance { get; set; }

        public float GrassDensity { get; set; }
        public float GrassFreq { get; set; }
        public float TreeDensity { get; set; }
        public float TreeFreq { get; set; }
        public float BushDensity { get; set; }
        public float BushFreq { get; set; }
        public float RockDensity { get; set; }
        public float RockFreq { get; set; }
        public float DetailAmp { get; set; }
        public float DetailFreq { get; set; }

        public float RoadDetailScale { get; set; }
        public float RoadDipScale { get; set; }
        public float RoadDirtScale { get; set; }

        /// <summary>
        /// Chance this is generated when creating a new map in the list of zones.
        /// </summary>
        public float ZoneListGenScale { get; set; }

        public long BaseTextureTypeId { get; set; }
        public long DirtTextureTypeId { get; set; }
        public long RoadTextureTypeId { get; set; }
        public long SteepTextureTypeId { get; set; }


        public List<WeightedName> ZoneNames { get; set; } = new List<WeightedName>();
        public List<WeightedName> ZoneAdjectives { get; set; } = new List<WeightedName>();


        
        public List<ZoneBridgeType> BridgeTypes { get; set; } = new List<ZoneBridgeType>();
        public List<ZoneFenceType> FenceTypes { get; set; } = new List<ZoneFenceType>();

        public double LargePropChance { get; set; }
        public double SmallPropChance { get; set; }
        public int MaxSmallPropQuantity { get; set; }

        public List<WeightedEntity> Props { get; set; } = new List<WeightedEntity>();

        public List<WeightedEntity> GetPropsOfType(long entityTypeId)
        {
            return Props.Where(x => x.EntityTypeId == entityTypeId).ToList();
        }

        public List<WeightedName> CreatureNamePrefixes { get; set; } = new List<WeightedName>();
        public List<WeightedName> CreatureDoubleNamePrefixes { get; set; } = new List<WeightedName>();

        public List<ZoneTypeOverride> Overrides { get; set; } = new List<ZoneTypeOverride>();
        public List<ZoneUnitSpawn> ZoneUnitSpawns { get; set; } = new List<ZoneUnitSpawn>();
        public List<ZoneUnitKeyword> UnitKeyWords { get; set; } = new List<ZoneUnitKeyword>();

        public int MinSameAdjacentZone { get; set; }

        public string PlantChoices { get; set; }

        public float FenceChance { get; set; }

        public long WeatherTypeId { get; set; }
        public float CreviceCountScale { get; set; }
        public float CreviceDepthScale { get; set; }
        public float CreviceWidthScale { get; set; }

        public long MusicTypeId { get; set; }
        public long AmbientMusicTypeId { get; set; }

        public double TraveralTimeScale { get; set; }

        public long ZoneCategoryId { get; set; }


        public bool IsOutdoors { get; set; }
        public bool IsDungeon { get; set; }

        public ZoneType()
        {
            ClearLists();
        }


        public long GetTerrainTextureIdFromChannel(int terrainChannel)
        {
            if (terrainChannel == TerrainTexChannels.Dirt)
            {
                return DirtTextureTypeId;
            }
            else if (terrainChannel == TerrainTexChannels.Road)
            {
                return RoadTextureTypeId;
            }
            else if (terrainChannel == TerrainTexChannels.Steep)
            {
                return SteepTextureTypeId;
            }
            return BaseTextureTypeId;
        }


        public void SlimForClient()
        {
            ClearLists();
        }
        private void ClearLists()
        {
            ZoneNames = new List<WeightedName>();
            ZoneAdjectives = new List<WeightedName>();

            BridgeTypes = new List<ZoneBridgeType>();
            FenceTypes = new List<ZoneFenceType>();

            CreatureNamePrefixes = new List<WeightedName>();
            CreatureDoubleNamePrefixes = new List<WeightedName>();

            ZoneUnitSpawns = new List<ZoneUnitSpawn>();
            Overrides = new List<ZoneTypeOverride>();

            Props = new List<WeightedEntity>();
        }
    }
}


