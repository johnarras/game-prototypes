using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Monsters.Settings
{
    public class CrawlerMonsterSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public int BaseMinDam { get; set; }
        public int BaseMaxDam { get; set; }
        public double MinDamPerLevel { get; set; }
        public double MaxDamPerLevel { get; set; }
        public double ScalingPerLevel { get; set; }

        public int BaseMinHealth { get; set; }
        public int BaseMaxHealth { get; set; }
        public double MinHealthPerLevel { get; set; }
        public double MaxHealthPerLevel { get; set; }


        public long ManaPerLevel { get; set; }
        public double ExtraHealthScalePerLevel { get; set; }
        public double ExtraDamageScalePerLevel { get; set; }

        public double MapUnitKeywordChance { get; set; }
        public double UnitKeywordChance { get; set; }

        public double PrimaryStatsPointsPerLevel { get; set; }

        public List<SpawnItem> BasicLoot { get; set; } = new List<SpawnItem>();
    }


    public class CrawlerMonsterSettingsLoader : NoChildSettingsLoader<CrawlerMonsterSettings> { }


    public class CrawlerMonsterSettingsDto : NoChildSettingsDto<CrawlerMonsterSettings>
    {
        public override CrawlerMonsterSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CrawlerMonsterSettingsMapper : NoChildSettingsMapper<CrawlerMonsterSettings, CrawlerMonsterSettingsDto> { }
}


