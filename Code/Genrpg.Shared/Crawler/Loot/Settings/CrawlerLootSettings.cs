using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Loot.Settings
{
    public class CrawlerLootType : ChildSettings, IIndexedGameItem, IItemEnchantWeight
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }

        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public long EntityTypeId { get; set; }
        public double ItemEnchantWeight { get; set; }
        public double ScalingPerLevel { get; set; }
    }


    public class CrawlerLootSettings : ParentSettings<CrawlerLootType> // No List
    {
        public override string Id { get; set; }
        public long BaseLootCost { get; set; }
        public double WeaponMult { get; set; }
        public double TwoHandWeaponMult { get; set; }
        public double ProcMult { get; set; }
        public double EffectMult { get; set; }
        public long MaxLootItems { get; set; }
        public double StatPointsPerExtraItem { get; set; }
        public double ItemChancePerMonster { get; set; }
        public double MinGoldPerLevel { get; set; }
        public double MaxGoldPerLevel { get; set; }
        public long InventoryPerPartyMember { get; set; }
        public double MinLevelExpMultDefault { get; set; }
        public double MaxLevelExpMultDefault { get; set; }
        public double MinLevelGoldMultDefault { get; set; }
        public double MaxLevelGoldMultDefault { get; set; }
        public double ItemChanceDefault { get; set; }
        public double ExtraLootScalePerMonsterBonus { get; set; }
        public bool AllowAllArmorTypes { get; set; }
        public bool AllowAllWeaponTypes { get; set; }

        /// <summary>
        /// Chance to get an item effect per extra spell chance.
        /// </summary>
        public double EnchantChancePerPowerIncrease { get; set; }

        public double BaseEnchantChance { get; set; }

        public long LevelDiffBeforeLootLoss { get; set; }
        public double LootLossPerLevelDiff { get; set; }
        public double MinLootPercent { get; set; }
        public double FirstMonsterItemDropChance { get; set; }
        public double StartStatBonusAmount { get; set; }
        public double StatBonusPerLevel { get; set; }
        public double StatBonusVariance { get; set; }

        public override CrawlerLootType Get(long idkey)
        {
            CrawlerLootType child = base.Get(idkey);

            if (child != null)
            {
                return child;
            }

            return _data.FirstOrDefault(x => x.EntityTypeId == idkey);
        }
    }

    public class CrawlerLootTypeSettingsDto : ParentSettingsDto<CrawlerLootSettings, CrawlerLootType>
    {
        public override List<CrawlerLootType> Children { get; set; }
        public override CrawlerLootSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CrawlerLootTypeSettingsLoader : ParentSettingsLoader<CrawlerLootSettings, CrawlerLootType> { }

    public class CrawlerLootTypeSettingsMapper : ParentSettingsMapper<CrawlerLootSettings, CrawlerLootType, CrawlerLootTypeSettingsDto> { }


}


