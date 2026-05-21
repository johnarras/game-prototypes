using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Maps.Settings
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


