using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.MapMods.Settings
{
    public class MapModEffectType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }

        public string Art { get; set; }
    }
    public class MapModEffectTypeSettings : ParentSettings<MapModEffectType>
    {
        public override string Id { get; set; }
    }

    public class MapModEffectTypeSettingsDto : ParentSettingsDto<MapModEffectTypeSettings, MapModEffectType>
    {
        public override List<MapModEffectType> Children { get; set; }
        public override MapModEffectTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class MapModEffectSettingsLoader : ParentSettingsLoader<MapModEffectTypeSettings, MapModEffectType> { }

    public class MapModEffectSettingsMapper : ParentSettingsMapper<MapModEffectTypeSettings, MapModEffectType, MapModEffectTypeSettingsDto> { }

}


