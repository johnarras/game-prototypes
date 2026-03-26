using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Spells.Settings.Procs
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


