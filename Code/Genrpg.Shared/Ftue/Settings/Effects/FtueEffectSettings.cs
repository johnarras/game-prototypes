using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Ftue.Settings.Effects
{

    public class FtueEffect : ChildSettings, IIndexedGameItem
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

    public class FtueEffectSettings : ParentSettings<FtueEffect>
    {
        public override string Id { get; set; }
    }

    public class FtueEffectSettingsDto : ParentSettingsDto<FtueEffectSettings, FtueEffect>
    {
        public override List<FtueEffect> Children { get; set; }
        public override FtueEffectSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class FtueEffectSettingsLoader : ParentSettingsLoader<FtueEffectSettings, FtueEffect> { }

    public class FtueEffectSettingsMapper : ParentSettingsMapper<FtueEffectSettings, FtueEffect, FtueEffectSettingsDto> { }


}


