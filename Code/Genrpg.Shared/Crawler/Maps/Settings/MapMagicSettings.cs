using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Maps.Settings
{
    public class MapMagicSettings : ParentSettings<MapMagicType>
    {
        public override string Id { get; set; }
        public double EncounterChance { get; set; }
    }

    public class MapMagicType : ChildSettings, IIndexedGameItem, IWeightedItem, IItemEnchantWeight
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
        public double SpreadChance { get; set; }
        public string MapSymbol { get; set; }
        public long MinLevel { get; set; }
        public double ItemEnchantWeight { get; set; }
    }

    public class MapMagicSettingsDto : ParentSettingsDto<MapMagicSettings, MapMagicType>
    {
        public override List<MapMagicType> Children { get; set; }
        public override MapMagicSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class MapMagicSettingsLoader : ParentSettingsLoader<MapMagicSettings, MapMagicType> { }

    public class MapMagicSettingsMapper : ParentSettingsMapper<MapMagicSettings, MapMagicType, MapMagicSettingsDto> { }

    public class MapMagicEntityHelper : BaseEntityHelper<MapMagicSettings, MapMagicType>
    {
        public override long HelperKey => EntityTypes.MapMagic;
    }
}


