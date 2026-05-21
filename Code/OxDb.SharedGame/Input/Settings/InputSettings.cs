using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using System.Collections.Generic;

namespace OxDb.SharedGame.Input.Settings
{
    public class InputSettings : ParentSettings<ActionInputSetting>
    {
        public override string Id { get; set; }
    }


    public class ActionInputSetting : ChildSettings
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public int Index { get; set; }
        public long SpellId { get; set; }
        public override string Name { get; set; }
    }

    public class ActionInputSettingsDto : ParentSettingsDto<InputSettings, ActionInputSetting>
    {
        public override List<ActionInputSetting> Children { get; set; }
        public override InputSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class ActionInputSettingsLoader : ParentSettingsLoader<InputSettings, ActionInputSetting> { }

    public class ActionInputSettingsMapper : ParentSettingsMapper<InputSettings, ActionInputSetting, ActionInputSettingsDto> { }


}


