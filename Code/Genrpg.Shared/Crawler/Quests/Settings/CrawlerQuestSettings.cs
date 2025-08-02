using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Crawler.Quests.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Dungeons.Constants;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using MessagePack;

namespace Genrpg.Shared.Crawler.Quests.Settings
{
    [MessagePackObject]
    public class CrawlerQuestSettings : ParentConstantListSettings<CrawlerQuestType, CrawlerQuestTypes>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double MaxDistanceFromQuestGiverToTargetMap { get; set; }
        [Key(2)] public int MinQuestsPerNpc { get; set; }
        [Key(3)] public int MaxQuestsPerNpc { get; set; }
        [Key(4)] public double ExtraQuestChance { get; set; }
        [Key(5)] public double ItemDropChance { get; set; }
        [Key(6)] public double BaseLootMult { get; set; }
        [Key(7)] public double ForceUnitInCombatChance { get; set; }
    }

    [MessagePackObject]
    public class CrawlerQuestType : ChildSettings, IIndexedGameItem, IWeightedItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public double Weight { get; set; }
        [Key(9)] public double MonsterGroupSizeScale { get; set; }

    }

    public class CrawlerQuestSettingsDto : ParentSettingsDto<CrawlerQuestSettings, CrawlerQuestType> { }
    public class CrawlerQuestSettingsLoader : ParentSettingsLoader<CrawlerQuestSettings, CrawlerQuestType> { }

    public class CrawlerQuestSettingsMapper : ParentSettingsMapper<CrawlerQuestSettings, CrawlerQuestType, CrawlerQuestSettingsDto> { }

}
