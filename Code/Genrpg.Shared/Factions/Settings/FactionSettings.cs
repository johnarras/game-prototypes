using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Factions.Settings
{
    public class FactionSettings : ParentSettings<FactionType>
    {
        public override string Id { get; set; }
    }
    public class FactionType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }

        public string Art { get; set; }

        public int StartRepLevelId { get; set; }

    }

    public class FactionSettingsDto : ParentSettingsDto<FactionSettings, FactionType>
    {
        public override List<FactionType> Children { get; set; }
        public override FactionSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class FactionSettingsLoader : ParentSettingsLoader<FactionSettings, FactionType> { }

    public class FactionSettingsMapper : ParentSettingsMapper<FactionSettings, FactionType, FactionSettingsDto> { }

}


