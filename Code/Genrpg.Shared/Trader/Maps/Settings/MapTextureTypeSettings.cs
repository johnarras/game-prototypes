using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Maps.Settings
{
    public class MapTextureSettings : ParentSettings<MapTexture>
    {
        public override string Id { get; set; }
    }

    public class MapTexture : ChildSettings, IIdName
    {

        public override string Id { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public override string ParentId { get; set; }
        public long ZoneTypeId { get; set; }
    }

    public class MapTextureSettingsDto : ParentSettingsDto<MapTextureSettings, MapTexture>
    {
        public override List<MapTexture> Children { get; set; }
        public override MapTextureSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class MapTextureSettingsLoader : ParentSettingsLoader<MapTextureSettings, MapTexture> { }

    public class MapTextureSettingsMapper : ParentSettingsMapper<MapTextureSettings, MapTexture, MapTextureSettingsDto> { }

}


