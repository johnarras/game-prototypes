using MessagePack;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Dungeons.Constants;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Units.Settings;

namespace Genrpg.Shared.Zones.Settings
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


        public List<ZoneTextureType> Textures { get; set; } = new List<ZoneTextureType>();

        public List<WeightedName> ZoneNames { get; set; } = new List<WeightedName>();
        public List<WeightedName> ZoneAdjectives { get; set; } = new List<WeightedName>();

        public List<ZoneBridgeType> BridgeTypes { get; set; } = new List<ZoneBridgeType>();
        public List<ZoneFenceType> FenceTypes { get; set; } = new List<ZoneFenceType>();
        public List<ZoneRockType> RockTypes { get; set; } = new List<ZoneRockType>();
        public List<ZoneTreeType> TreeTypes { get; set; } = new List<ZoneTreeType>();

        public List<WeightedName> CreatureNamePrefixes { get; set; } = new List<WeightedName>();
        public List<WeightedName> CreatureDoubleNamePrefixes { get; set; } = new List<WeightedName>();

        public List<ZoneTypeOverride> Overrides { get; set; } = new List<ZoneTypeOverride>();
        public List<ZoneUnitSpawn> ZoneUnitSpawns { get; set; } = new List<ZoneUnitSpawn>();
        public List<ZoneUnitKeyword> UnitKeyWords { get; set; } = new List<ZoneUnitKeyword>();

        public string PlantChoices { get; set; }

        public float FenceChance { get; set; }

        public long WeatherTypeId { get; set; }
        public long BuildingTypeId { get; set; }

        public float CreviceCountScale { get; set; }
        public float CreviceDepthScale { get; set; }
        public float CreviceWidthScale { get; set; }

        public long MusicTypeId { get; set; }
        public long AmbientMusicTypeId { get; set; }

        public double TraveralTimeScale { get; set; } = 1.0;

        public long ZoneCategoryId { get; set; }

        public ZoneType()
        {
            ClearLists();
        }


        public void SlimForClient()
        {
            ClearLists();
        }
        private void ClearLists()
        {
            Textures = new List<ZoneTextureType>();
            ZoneNames = new List<WeightedName>();
            ZoneAdjectives = new List<WeightedName>();

            BridgeTypes = new List<ZoneBridgeType>();
            FenceTypes = new List<ZoneFenceType>();
            RockTypes = new List<ZoneRockType>();

            CreatureNamePrefixes = new List<WeightedName>();
            CreatureDoubleNamePrefixes = new List<WeightedName>();

            ZoneUnitSpawns = new List<ZoneUnitSpawn>();
            Overrides = new List<ZoneTypeOverride>();
        }
    }
}


