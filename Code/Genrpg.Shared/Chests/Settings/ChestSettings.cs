using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using System.Collections.Generic;

namespace Genrpg.Shared.Chests.Settings
{
    public class ChestSettings : ParentSettings<Chest>
    {
        public override string Id { get; set; }

        /// <summary>
        /// Base loot with scaling for tiered chests.
        /// </summary>
        public List<SpawnItem> TieredCurrencyLoot { get; set; } = new List<SpawnItem>();
    }

    public class Chest : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public long UnlockMinutes { get; set; }

        public int Tier { get; set; } // Is this a tiered chest?

        public int TieredLootMult { get; set; } // Loot Mult for this tiered chest.

        public List<SpawnItem> Loot { get; set; } = new List<SpawnItem>();


    }

    public class ChestSettingsDto : ParentSettingsDto<ChestSettings, Chest>
    {
        public override List<Chest> Children { get; set; }
        public override ChestSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class ChestSettingsLoader : ParentSettingsLoader<ChestSettings, Chest> { }

    public class ChestSettingsMapper : ParentSettingsMapper<ChestSettings, Chest, ChestSettingsDto> { }


    public class ChestHelper : BaseEntityHelper<ChestSettings, Chest>
    {
        public override long HelperKey => EntityTypes.Chest;
    }
}


