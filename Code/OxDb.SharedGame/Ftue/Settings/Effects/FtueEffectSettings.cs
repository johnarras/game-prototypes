using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Ftue.Settings.Effects
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


