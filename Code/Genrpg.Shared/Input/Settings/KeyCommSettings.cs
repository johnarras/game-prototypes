using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Input.Settings
{
    public class KeyCommSettings : ParentSettings<KeyCommSetting>
    {
        public override string Id { get; set; }
    }
    public class KeyCommSetting : ChildSettings, IId
    {
        public override string Id { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public override string ParentId { get; set; }
        public string KeyPress { get; set; }
        public string KeyCommand { get; set; }
        public int Modifiers { get; set; }
    }

    public class KeyCommSettingsDto : ParentSettingsDto<KeyCommSettings, KeyCommSetting>
    {
        public override List<KeyCommSetting> Children { get; set; }
        public override KeyCommSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class KeyCommSettingsLoader : ParentSettingsLoader<KeyCommSettings, KeyCommSetting> { }

    public class KeyCommSettingsMapper : ParentSettingsMapper<KeyCommSettings, KeyCommSetting, KeyCommSettingsDto> { }


}


