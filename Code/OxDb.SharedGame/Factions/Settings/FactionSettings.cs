using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Factions.Settings
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


