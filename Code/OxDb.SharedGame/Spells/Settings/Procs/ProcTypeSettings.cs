using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Spells.Settings.Procs
{
    public class ProcType : ChildSettings, IIndexedGameItem
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
    public class ProcTypeSettings : ParentSettings<ProcType>
    {
        public override string Id { get; set; }
    }

    public class ProcTypeSettingsDto : ParentSettingsDto<ProcTypeSettings, ProcType>
    {
        public override List<ProcType> Children { get; set; }
        public override ProcTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class ProcTypeSettingsLoader : ParentSettingsLoader<ProcTypeSettings, ProcType> { }

    public class ProcTypeSettingsMapper : ParentSettingsMapper<ProcTypeSettings, ProcType, ProcTypeSettingsDto> { }


}


