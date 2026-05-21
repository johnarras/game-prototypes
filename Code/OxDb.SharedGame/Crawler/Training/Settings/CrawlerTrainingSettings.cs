using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Crawler.Training.Settings
{
    public class CrawlerTrainingSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public long LinearCostPerLevel { get; set; }
        public long QuadraticCostPerLevel { get; set; }

        public double StartKillsNeeded { get; set; }
        public double ExtraKillsNeededLinear { get; set; }
        public double ExtraKillsNeededQuadratic { get; set; }

        public double StartMonsterExp { get; set; }
        public double ExtraMonsterExp { get; set; }

        public long MaxScalingExpLevel { get; set; }

        public long NewClassGoldCostMult { get; set; }

        public bool AdvanceOneClassPerLevel { get; set; }
        public int MaxDistinctClasses { get; set; }

        public int StatGainOnLevelMult { get; set; }

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

    public class CrawlerTrainingSettingsDto : NoChildSettingsDto<CrawlerTrainingSettings>
    {
        public override CrawlerTrainingSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CrawlerTrainingSettingsMapper : NoChildSettingsMapper<CrawlerTrainingSettings, CrawlerTrainingSettingsDto> { }
}


