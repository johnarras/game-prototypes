using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Crawler.Crafting.Settings
{

    public class CrawlerCraftingSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        /// <summary>
        /// Chance for each monster to drop some reagent. currently just random.
        /// </summary>
        public double MonsterDropReagentChance { get; set; }
        /// <summary>
        /// Chance to get a reagent from each current stat on something being scrapped
        /// </summary>
        public double ScrapReagentChance { get; set; }
        /// <summary>
        /// Percent of current stats needed in terms of reagents to upgrade
        /// </summary>
        public double CurrentStatUpgradeCostScale { get; set; }
        /// <summary>
        /// Percent of stats added (rounded up) when upgrading
        /// </summary>
        public double UpgradeStatIncreaseScale { get; set; }
    }

    public class CrawlerCraftingSettingsLoader : NoChildSettingsLoader<CrawlerCraftingSettings> { }

    public class CrawlerCraftingSettingsDto : NoChildSettingsDto<CrawlerCraftingSettings>
    {
        public override CrawlerCraftingSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CrawlerCraftingSettingsMapper : NoChildSettingsMapper<CrawlerCraftingSettings, CrawlerCraftingSettingsDto> { }
}


