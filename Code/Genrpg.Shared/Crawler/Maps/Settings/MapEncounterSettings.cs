using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Dungeons.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using MessagePack;

namespace Genrpg.Shared.Crawler.Maps.Settings
{
    [MessagePackObject]
    public class MapEncounterSettings : ParentSettings<MapEncounterType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double EncounterChance { get; set; }
    }

    [MessagePackObject]
    public class MapEncounterType : ChildSettings, IIndexedGameItem, IWeightedItem
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
        [Key(9)] public bool CanRepeat { get; set; }
        [Key(10)] public bool CanBeCleansed { get; set; }

    }

    public class MapEncounterSettingsDto : ParentSettingsDto<MapEncounterSettings, MapEncounterType> { }
    public class MapEncounterSettingsLoader : ParentSettingsLoader<MapEncounterSettings, MapEncounterType> { }

    public class MapEncounterSettingsMapper : ParentSettingsMapper<MapEncounterSettings, MapEncounterType, MapEncounterSettingsDto> { }

    public class MapEncounterEntityHelper : BaseEntityHelper<MapEncounterSettings, MapEncounterType>
    {
        public override long Key => EntityTypes.MapEncounter;
    }
}
