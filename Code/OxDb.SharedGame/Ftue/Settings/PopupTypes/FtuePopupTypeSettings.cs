using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Ftue.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Ftue.Settings.PopupTypes
{

    public class FtuePopupType : ChildSettings, IIndexedGameItem
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

    public class FtuePopupTypeSettings : ParentConstantListSettings<FtuePopupType, FtuePopupTypes>
    {
        public override string Id { get; set; }
    }

    public class FtuePopupTypeSettingsDto : ParentSettingsDto<FtuePopupTypeSettings, FtuePopupType>
    {
        public override List<FtuePopupType> Children { get; set; }
        public override FtuePopupTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class FtuePopupTypeSettingsLoader : ParentSettingsLoader<FtuePopupTypeSettings, FtuePopupType> { }

    public class FtuePopupSettingsMapper : ParentSettingsMapper<FtuePopupTypeSettings, FtuePopupType, FtuePopupTypeSettingsDto> { }


}


