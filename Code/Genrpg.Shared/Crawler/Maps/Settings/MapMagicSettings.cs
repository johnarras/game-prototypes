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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;

namespace Genrpg.Shared.Crawler.Maps.Settings
{
    [MessagePackObject]
    public class MapMagicSettings : ParentSettings<MapMagicType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double EncounterChance { get; set; }
    }

    [MessagePackObject]
    public class MapMagicType : ChildSettings, IIndexedGameItem, IWeightedItem
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
        [Key(9)] public double SpreadChance { get; set; }
        [Key(10)] public string MapSymbol { get; set; }
        [Key(11)] public long MinLevel { get; set; }

    }

    public class MapMagicSettingsDto : ParentSettingsDto<MapMagicSettings, MapMagicType> { }
    public class MapMagicSettingsLoader : ParentSettingsLoader<MapMagicSettings, MapMagicType> { }

    public class MapMagicSettingsMapper : ParentSettingsMapper<MapMagicSettings, MapMagicType, MapMagicSettingsDto> { }

    public class MapMagicEntityHelper : BaseEntityHelper<MapMagicSettings, MapMagicType>
    {
        public override long Key => EntityTypes.MapMagic;
    }
}
