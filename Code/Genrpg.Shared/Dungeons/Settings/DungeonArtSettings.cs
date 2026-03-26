using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Dungeons.Constants;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Dungeons.Settings
{
    public class DungeonArtSettings : ParentConstantListSettings<DungeonArt, DungeonArtTypes>
    {
        public override string Id { get; set; }
    }
    public class DungeonArt : ChildSettings, IIndexedGameItem
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

    public class DungeonArtSettingsDto : ParentSettingsDto<DungeonArtSettings, DungeonArt>
    {
        public override List<DungeonArt> Children { get; set; }
        public override DungeonArtSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class DungeonArtSettingsLoader : ParentSettingsLoader<DungeonArtSettings, DungeonArt> { }

    public class DungeonArtSettingsMapper : ParentSettingsMapper<DungeonArtSettings, DungeonArt, DungeonArtSettingsDto> { }


}


