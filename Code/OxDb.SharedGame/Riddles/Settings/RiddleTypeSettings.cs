using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Riddles.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Riddles.Settings
{

    public class RiddleTypeSettings : ParentConstantListSettings<RiddleType, RiddleTypes>
    {
        public override string Id { get; set; }
    }
    public class RiddleType : ChildSettings, IIndexedGameItem, IWeightedItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public double Weight { get; set; }
        public bool IsToggle { get; set; }
        public bool IsObject { get; set; }
    }

    public class RiddleTypeSettingsDto : ParentSettingsDto<RiddleTypeSettings, RiddleType>
    {
        public override List<RiddleType> Children { get; set; }
        public override RiddleTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class RiddleTypeSettingsLoader : ParentSettingsLoader<RiddleTypeSettings, RiddleType> { }

    public class RiddleTypeSettingsMapper : ParentSettingsMapper<RiddleTypeSettings, RiddleType, RiddleTypeSettingsDto> { }

    public class RiddleTypeEntityHelper : BaseEntityHelper<RiddleTypeSettings, RiddleType>
    {
        public override long HelperKey => EntityTypes.RiddleType;
    }

}


