using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Charms.Settings
{
    public class CharmUse : ChildSettings, IIdName
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string Icon { get; set; }
    }
    public class CharmUseSettings : ParentSettings<CharmUse>
    {
        public override string Id { get; set; }
    }

    public class CharmUseSettingsDto : ParentSettingsDto<CharmUseSettings, CharmUse>
    {
        public override List<CharmUse> Children { get; set; }
        public override CharmUseSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class CharmUseSettingsLoader : ParentSettingsLoader<CharmUseSettings, CharmUse> { }

    public class CharmUseSettingsMapper : ParentSettingsMapper<CharmUseSettings, CharmUse, CharmUseSettingsDto> { }
}


