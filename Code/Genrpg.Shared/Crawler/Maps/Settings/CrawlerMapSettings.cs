using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Dungeons.Constants;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using MessagePack;
using System.Collections.Generic;
using System.Transactions;

namespace Genrpg.Shared.Crawler.Maps.Settings
{
    [MessagePackObject]
    public class CrawlerMapSettings : ParentConstantListSettings<CrawlerMapType,CrawlerMapTypes>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double CorridorDungeonSizeScale { get; set; }
        [Key(2)] public int MinZoneUnitSpawns { get; set; }
        [Key(3)] public int MaxZoneUnitSpawns { get; set; }
        [Key(4)] public int RareSpawnCount { get; set; }
        [Key(5)] public double QuestItemEntranceUnlockChance { get; set; }
        [Key(6)] public double RiddleUnlockChance { get; set; }
        [Key(7)] public double DrainHealthPercent { get; set; }
        [Key(8)] public double DrainManaPercent { get; set; }
        [Key(9)] public double TrapHitChance { get; set; }
        [Key(10)] public double TrapDebuffChance { get; set; }
        [Key(11)] public double TrapDebuffLevelScaling { get; set; }
        [Key(12)] public int TrapMinDamPerLevel { get; set; }
        [Key(13)] public int TrapMaxDamagePerLevel { get; set; }
        [Key(14)] public int SharedZoneUnitCount { get; set; }
        [Key(15)] public double ExtraTeleportChance { get; set; }
        [Key(16)] public int MinTeleportQuantity { get; set; }
        [Key(17)] public int MaxTeleportQuantity { get; set; }
        [Key(18)] public double UnitKeywordChance { get; set; }
        [Key(19)] public int MinQuestUnlockDungeonLevel { get; set; }
    }

    [MessagePackObject]
    public class CrawlerMapType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public List<CrawlerMapGenType> GenTypes { get; set; } = new List<CrawlerMapGenType>();
        [Key(9)] public double NpcChance { get; set; }
        [Key(10)] public int MinNpcQuantity { get; set; }
        [Key(11)] public int MaxNpcQuantity { get; set; }
        [Key(12)] public int MinNpcSeparation { get; set; }
        [Key(13)] public int MinDistanceToEntrance { get; set; }
        
    }

    [MessagePackObject]
    public class CrawlerMapGenType : IWeightedItem
    { 
        [Key(0)] public string Name { get; set; }
        [Key(1)] public int MinWidth { get; set; } = 15;
        [Key(2)] public int MaxWidth { get; set; } = 25;
        [Key(3)] public int MinHeight { get; set; } = 15;
        [Key(4)] public int MaxHeight { get; set; } = 25;
        [Key(5)] public int MinFloors { get; set; } = 1;
        [Key(6)] public int MaxFloors { get; set; } = 1;
        [Key(7)] public double SpecialTileChance { get; set; }
        [Key(8)] public double Weight { get; set; }
        [Key(9)] public double RandomWallsChance { get; set; }
        [Key(10)] public double LoopingChance { get; set; }
        [Key(11)] public double MinWallChance { get; set; }
        [Key(12)] public double MaxWallChance { get; set; }
        [Key(13)] public double MinDoorChance { get; set; } 
        [Key(14)] public double MaxDoorChance { get; set; }
        [Key(15)] public double TrapTileChance { get; set; } 
        [Key(16)] public double EffectTileChance { get; set; }
        [Key(17)] public double MinCorridorDensity { get; set; }
        [Key(18)] public double MaxCorridorDensity { get; set; }
        [Key(19)] public double MinBuildingDensity { get; set; }
        [Key(20)] public double MaxBuildingDensity { get; set; }
        [Key(21)] public bool IsIndoors { get; set; }
        [Key(22)] public bool NextLevelIsDown { get; set; }
        [Key(23)] public List<WeightedZoneType> WeightedZones { get; set; } = new List<WeightedZoneType>();
    }

    [MessagePackObject]
    public class WeightedZoneType : IWeightedItem
    {
        [Key(0)] public string Name { get; set; }
        [Key(1)] public double Weight { get; set; }
        [Key(2)] public long ZoneTypeId { get; set; }
    }


    public class CrawlerMapSettingsDto : ParentSettingsDto<CrawlerMapSettings, CrawlerMapType> { }
    public class CrawlerMapSettingsLoader : ParentSettingsLoader<CrawlerMapSettings, CrawlerMapType> { }

    public class CrawlerMapSettingsMapper : ParentSettingsMapper<CrawlerMapSettings, CrawlerMapType, CrawlerMapSettingsDto> { }

}
