using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using MessagePack;

namespace Genrpg.Shared.Crawler.Loot.Settings
{
    [MessagePackObject]
    public class CrawlerLootType : ChildSettings, IIndexedGameItem, IItemEnchantWeight
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }

        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

        [Key(8)] public long EntityTypeId { get; set; }
        [Key(9)] public double ItemEnchantWeight { get; set; }
        [Key(10)] public double ScalingPerLevel { get; set; }
    }


    [MessagePackObject]
    public class CrawlerLootSettings : ParentSettings<CrawlerLootType> // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long BaseLootCost { get; set; }
        [Key(2)] public double WeaponMult { get; set; }
        [Key(3)] public double TwoHandWeaponMult { get; set; }
        [Key(4)] public double ProcMult { get; set; }
        [Key(5)] public double EffectMult { get; set; }
        [Key(6)] public long MaxLootItems { get; set; }
        [Key(7)] public double StatPointsPerExtraItem { get; set; }
        [Key(8)] public double ItemChancePerMonster { get; set; }
        [Key(9)] public double MinGoldPerLevel { get; set; }
        [Key(10)] public double MaxGoldPerLevel { get; set; }
        [Key(11)] public long InventoryPerPartyMember { get; set; }
        [Key(12)] public double MinLevelExpMultDefault { get; set; }
        [Key(13)] public double MaxLevelExpMultDefault { get; set; }
        [Key(14)] public double MinLevelGoldMultDefault { get; set; }
        [Key(15)] public double MaxLevelGoldMultDefault { get; set; }
        [Key(16)] public double ItemChanceDefault { get; set; }
        [Key(17)] public double ExtraLootScalePerMonsterBonus { get; set; }
        [Key(18)] public bool AllowAllArmorTypes { get; set; }
        [Key(19)] public bool AllowAllWeaponTypes { get; set; }
        /// <summary>
        /// Chance to get an item effect per extra spell chance.
        /// </summary>
        [Key(20)] public double ItemEnchantChance { get; set; }

        [Key(21)] public long LevelDiffBeforeLootLoss { get; set; }
        [Key(22)] public double LootLossPerLevelDiff { get; set; }
        [Key(23)] public double MinLootPercent { get; set; }
    }

    public class CrawlerLootTypeSettingsDto : ParentSettingsDto<CrawlerLootSettings, CrawlerLootType> { }

    public class CrawlerLootTypeSettingsLoader : ParentSettingsLoader<CrawlerLootSettings, CrawlerLootType> { }

    public class CrawlerLootTypeSettingsMapper : ParentSettingsMapper<CrawlerLootSettings, CrawlerLootType, CrawlerLootTypeSettingsDto> { }


}
