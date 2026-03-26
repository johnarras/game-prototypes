using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Riddles.Settings
{

    public class AphorismSettings : ParentSettings<Aphorism>
    {
        public override string Id { get; set; }
    }
    public class Aphorism : ChildSettings, IIndexedGameItem
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

    public class AphorismSettingsDto : ParentSettingsDto<AphorismSettings, Aphorism>
    {
        public override List<Aphorism> Children { get; set; }
        public override AphorismSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class AphorismSettingsLoader : ParentSettingsLoader<AphorismSettings, Aphorism> { }

    public class AphorismSettingsMapper : ParentSettingsMapper<AphorismSettings, Aphorism, AphorismSettingsDto> { }

}


