using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using System.Collections.Generic;

namespace OxDb.SharedGame.Inventory.Settings.ItemSets
{
    public class SetTypeSettings : ParentSettings<SetType>
    {
        public override string Id { get; set; }
    }

    public class SetTypeSettingsDto : ParentSettingsDto<SetTypeSettings, SetType>
    {
        public override List<SetType> Children { get; set; }
        public override SetTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class SetTypeSettingsLoader : ParentSettingsLoader<SetTypeSettings, SetType> { }

    public class SetSettingsMapper : ParentSettingsMapper<SetTypeSettings, SetType, SetTypeSettingsDto> { }


}


