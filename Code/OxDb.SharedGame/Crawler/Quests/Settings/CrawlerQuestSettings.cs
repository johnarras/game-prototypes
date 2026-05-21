using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Quests.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Quests.Settings
{
    public class CrawlerQuestSettings : ParentConstantListSettings<CrawlerQuestType, CrawlerQuestTypes>
    {
        public override string Id { get; set; }
        public double MaxDistanceFromQuestGiverToTargetMap { get; set; }
        public int MinQuestsPerNpc { get; set; }
        public int MaxQuestsPerNpc { get; set; }
        public double ExtraQuestChance { get; set; }
        public double ItemDropChance { get; set; }
        public double BaseLootMult { get; set; }
        public double ForceUnitInCombatChance { get; set; }
        public double ExpLootMult { get; set; }
        public double GoldLootMult { get; set; }
        public double ItemLootMult { get; set; }

        public long SingleDungeonMaxLevelGapForCredit { get; set; }

        public int SingleDungeonNpcQuestCount { get; set; }
    }

    public class CrawlerQuestType : ChildSettings, IIndexedGameItem, IWeightedItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public double Weight { get; set; }
        public double MonsterGroupSizeScale { get; set; }

    }

    public class CrawlerQuestSettingsDto : ParentSettingsDto<CrawlerQuestSettings, CrawlerQuestType>
    {
        public override List<CrawlerQuestType> Children { get; set; }
        public override CrawlerQuestSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class CrawlerQuestSettingsLoader : ParentSettingsLoader<CrawlerQuestSettings, CrawlerQuestType> { }

    public class CrawlerQuestSettingsMapper : ParentSettingsMapper<CrawlerQuestSettings, CrawlerQuestType, CrawlerQuestSettingsDto> { }

}


