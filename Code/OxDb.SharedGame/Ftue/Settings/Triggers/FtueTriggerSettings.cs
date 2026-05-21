using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Ftue.Settings.Triggers
{

    public class FtueTrigger : ChildSettings, IIndexedGameItem
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

    public class FtueTriggerSettings : ParentSettings<FtueTrigger>
    {
        public override string Id { get; set; }
    }

    public class FtueTriggerSettingsDto : ParentSettingsDto<FtueTriggerSettings, FtueTrigger>
    {
        public override List<FtueTrigger> Children { get; set; }
        public override FtueTriggerSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class FtueTriggerSettingsLoader : ParentSettingsLoader<FtueTriggerSettings, FtueTrigger> { }

    public class FtueTriggerSettingsMapper : ParentSettingsMapper<FtueTriggerSettings, FtueTrigger, FtueTriggerSettingsDto> { }


}


