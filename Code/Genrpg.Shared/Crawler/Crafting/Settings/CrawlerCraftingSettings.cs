using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Crawler.Crafting.Settings
{

    [MessagePackObject]
    public class CrawlerCraftingSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        /// <summary>
        /// Chance for each monster to drop some reagent. currently just random.
        /// </summary>
        [Key(1)] public double MonsterDropReagentChance { get; set; }
        /// <summary>
        /// Chance to get a reagent from each current stat on something being scrapped
        /// </summary>
        [Key(2)] public double ScrapReagentChance { get; set; }
        /// <summary>
        /// Percent of current stats needed in terms of reagents to upgrade
        /// </summary>
        [Key(3)] public double CurrentStatUpgradeCostScale { get; set; }
        /// <summary>
        /// Percent of stats added (rounded up) when upgrading
        /// </summary>
        [Key(4)] public double UpgradeStatIncreaseScale { get; set; }
    }

    public class CrawlerCraftingSettingsLoader : NoChildSettingsLoader<CrawlerCraftingSettings> { }

    public class CrawlerCraftingSettingsDto : NoChildSettingsDto<CrawlerCraftingSettings> { }

    public class CrawlerCraftingSettingsMapper : NoChildSettingsMapper<CrawlerCraftingSettings, CrawlerCraftingSettingsDto> { }
}
