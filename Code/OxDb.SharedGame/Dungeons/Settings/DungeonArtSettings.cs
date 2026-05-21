using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Dungeons.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Dungeons.Settings
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


