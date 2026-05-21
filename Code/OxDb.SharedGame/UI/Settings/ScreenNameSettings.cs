using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.UI.Constants;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.UI.Settings
{
    public class ScreenName : ChildSettings, IIdName
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public long ScreenLayerId { get; set; } = ScreenLayers.Screens;
        public bool AllowMultiQueue { get; set; }
        public string Subdirectory { get; set; }


    }

    public class ScreenNameSettings : ParentConstantListSettings<ScreenName, ScreenNames>
    {
        public override string Id { get; set; }


        public ScreenName Get(string name)
        {
            return _data.FirstOrDefault(x => x.Name == name);
        }
    }

    public class ScreenNameSettingsDto : ParentSettingsDto<ScreenNameSettings, ScreenName>
    {
        public override List<ScreenName> Children { get; set; }
        public override ScreenNameSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class ScreenNameSettingsLoader : ParentSettingsLoader<ScreenNameSettings, ScreenName> { }

    public class ScreenNameSettingsMapper : ParentSettingsMapper<ScreenNameSettings, ScreenName, ScreenNameSettingsDto> { }

}


