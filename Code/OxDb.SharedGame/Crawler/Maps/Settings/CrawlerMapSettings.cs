using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Maps.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Maps.Settings
{
    public class CrawlerMapSettings : ParentConstantListSettings<CrawlerMapType, CrawlerMapTypes>
    {
        public override string Id { get; set; }
        public double CorridorDungeonSizeScale { get; set; }
        public int MinZoneUnitSpawns { get; set; }
        public int MaxZoneUnitSpawns { get; set; }
        public int RareSpawnCount { get; set; }
        public double QuestItemEntranceUnlockChance { get; set; }
        public double RiddleUnlockChance { get; set; }
        public double DrainHealthPercent { get; set; }
        public double DrainManaPercent { get; set; }
        public double TrapHitChance { get; set; }
        public double TrapDebuffChance { get; set; }
        public double TrapDebuffLevelScaling { get; set; }
        public int TrapMinDamPerLevel { get; set; }
        public int TrapMaxDamagePerLevel { get; set; }
        public int SharedZoneUnitCount { get; set; }
        public double ExtraTeleportChance { get; set; }
        public int MinTeleportQuantity { get; set; }
        public int MaxTeleportQuantity { get; set; }
        public double UnitKeywordChance { get; set; }
        public int MinQuestUnlockDungeonLevel { get; set; }
        public int MinQuestItemDungeonLevel { get; set; }
    }

    public class CrawlerMapType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public List<CrawlerMapGenType> GenTypes { get; set; } = new List<CrawlerMapGenType>();
        public double NpcChance { get; set; }
        public int MinNpcQuantity { get; set; }
        public int MaxNpcQuantity { get; set; }
        public int MinNpcSeparation { get; set; }
        public int MinDistanceToEntrance { get; set; }
        public double RoomIsDifferentZoneTypeChance { get; set; }

    }

    public class CrawlerMapGenType : IWeightedItem
    {
        public string Name { get; set; }
        public int MinWidth { get; set; } = 15;
        public int MaxWidth { get; set; } = 25;
        public int MinHeight { get; set; } = 15;
        public int MaxHeight { get; set; } = 25;
        public int MinFloors { get; set; } = 1;
        public int MaxFloors { get; set; } = 1;
        public double SpecialTileChance { get; set; }
        public double Weight { get; set; }
        public double RandomWallsChance { get; set; }
        public double LoopingChance { get; set; }
        public double MinWallChance { get; set; }
        public double MaxWallChance { get; set; }
        public double MinDoorChance { get; set; }
        public double MaxDoorChance { get; set; }
        public double TrapTileChance { get; set; }
        public double EffectTileChance { get; set; }
        public double MinCorridorDensity { get; set; }
        public double MaxCorridorDensity { get; set; }
        public double MinBuildingDensity { get; set; }
        public double MaxBuildingDensity { get; set; }
        public bool IsIndoors { get; set; }
        public bool NextLevelIsDown { get; set; }
        public List<WeightedZoneType> WeightedZones { get; set; } = new List<WeightedZoneType>();
    }

    public class WeightedZoneType : IWeightedItem
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public long ZoneTypeId { get; set; }
    }


    public class CrawlerMapSettingsDto : ParentSettingsDto<CrawlerMapSettings, CrawlerMapType>
    {
        public override List<CrawlerMapType> Children { get; set; }
        public override CrawlerMapSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class CrawlerMapSettingsLoader : ParentSettingsLoader<CrawlerMapSettings, CrawlerMapType> { }

    public class CrawlerMapSettingsMapper : ParentSettingsMapper<CrawlerMapSettings, CrawlerMapType, CrawlerMapSettingsDto> { }

}


