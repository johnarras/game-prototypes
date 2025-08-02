using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;
using System;
using System.Security;

namespace Genrpg.Shared.Crawler.Training.Settings
{
    [MessagePackObject]
    public class CrawlerTrainingSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long LinearCostPerLevel { get; set; }
        [Key(2)] public long QuadraticCostPerLevel { get; set; }

        [Key(3)] public double StartKillsNeeded { get; set; }
        [Key(4)] public double ExtraKillsNeededLinear { get; set; }
        [Key(5)] public double ExtraKillsNeededQuadratic { get; set; }

        [Key(6)] public double StartMonsterExp { get; set; }
        [Key(7)] public double ExtraMonsterExp { get; set; }

        [Key(8)] public long MaxScalingExpLevel { get; set; }

        [Key(9)] public long NewClassGoldCostMult { get; set; }

        [Key(10)] public bool AdvanceOneClassPerLevel { get; set; }
        [Key(11)] public int MaxDistinctClasses { get; set; }

        [Key(12)] public int StatGainOnLevelMult { get; set; }

        public long GetMonsterExp(long currentLevel)
        {
            if (currentLevel > MaxScalingExpLevel)
            {
                currentLevel = MaxScalingExpLevel;
            }
            return (long)(StartMonsterExp + ExtraMonsterExp * (currentLevel - 1));
        }
    }


    public class CrawlerTrainingSettingsLoader : NoChildSettingsLoader<CrawlerTrainingSettings> { }

    public class CrawlerTrainingSettingsDto : NoChildSettingsDto<CrawlerTrainingSettings> { }

    public class CrawlerTrainingSettingsMapper : NoChildSettingsMapper<CrawlerTrainingSettings, CrawlerTrainingSettingsDto> { }
}
