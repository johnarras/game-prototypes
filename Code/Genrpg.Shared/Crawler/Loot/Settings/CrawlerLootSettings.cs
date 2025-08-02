using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Crawler.Loot.Settings
{
    [MessagePackObject]
    public class CrawlerLootSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long BaseLootCost { get; set; }
        [Key(2)] public double WeaponMult { get; set; }
        [Key(3)] public double TwoHandWeaponMult { get; set; }
        [Key(4)] public double ProcMult { get; set; }
        [Key(5)] public double EffectMult { get; set; }
        [Key(6)] public long MaxLootItems { get; set; }
        [Key(7)] public double ItemChancePerMonster { get; set; }
        [Key(8)] public double MinGoldPerLevel { get; set; }
        [Key(9)] public double MaxGoldPerLevel { get; set; }
        [Key(10)] public long InventoryPerPartyMember { get; set; }
        [Key(11)] public double MinLevelExpMultDefault { get; set; }
        [Key(12)] public double MaxLevelExpMultDefault { get; set; }
        [Key(13)] public double MinLevelGoldMultDefault { get; set; }
        [Key(14)] public double MaxLevelGoldMultDefault { get; set; }
        [Key(15)] public double ItemChanceDefault { get; set; }
        [Key(16)] public double ExtraLootScalePerMonsterBonus { get; set; }
    }

    public class CrawlerLootSettingsLoader : NoChildSettingsLoader<CrawlerLootSettings> { }


    public class CrawlerLootSettingsDto : NoChildSettingsDto<CrawlerLootSettings> { }

    public class CrawlerLootSettingsMapper : NoChildSettingsMapper<CrawlerLootSettings, CrawlerLootSettingsDto> { }
}
