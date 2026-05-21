using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.UI.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.UI.Settings
{
    public class ScreenLayer : ChildSettings, IIdName
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public bool SkipInAllScreensList { get; set; }


    }

    public class ScreenLayerSettings : ParentConstantListSettings<ScreenLayer, ScreenLayers>
    {
        public override string Id { get; set; }
    }

    public class ScreenLayerSettingsDto : ParentSettingsDto<ScreenLayerSettings, ScreenLayer>
    {
        public override List<ScreenLayer> Children { get; set; }
        public override ScreenLayerSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class ScreenLayerSettingsLoader : ParentSettingsLoader<ScreenLayerSettings, ScreenLayer> { }

    public class ScreenLayerSettingsMapper : ParentSettingsMapper<ScreenLayerSettings, ScreenLayer, ScreenLayerSettingsDto> { }

}


