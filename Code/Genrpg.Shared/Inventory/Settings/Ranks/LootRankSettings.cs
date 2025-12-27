using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Constants;
using System.Collections.Generic;

namespace Genrpg.Shared.Inventory.Settings.Ranks
{
    /// <summary>
    /// List of equipment slots for characters
    /// </summary>
    public class LootRank : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public long Armor { get; set; }
        public long Damage { get; set; }

        public long CostPct { get; set; } = 100;

    }

    public class LootRankSettings : ParentConstantListSettings<LootRank, LootRanks>
    {
        public override string Id { get; set; }
        public double LevelsPerQuality { get; set; }
        public double ExtraQualityChance { get; set; }
        public double ArmorChance { get; set; }
    }

    public class LootRankSettingsDto : ParentSettingsDto<LootRankSettings, LootRank>
    {
        public override List<LootRank> Children { get; set; }
        public override LootRankSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class LootRankSettingsLoader : ParentSettingsLoader<LootRankSettings, LootRank> { }

    public class ItemSettingsMapper : ParentSettingsMapper<LootRankSettings, LootRank, LootRankSettingsDto> { }

}


