using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.ProcGen.Settings.Props
{
    public class PropType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public int NumChoices { get; set; }
    }
    public class PropTypeSettings : ParentSettings<PropType>
    {
        public override string Id { get; set; }
    }

    public class PropTypeSettingsDto : ParentSettingsDto<PropTypeSettings, PropType>
    {
        public override List<PropType> Children { get; set; }
        public override PropTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class PropTypeSettingsLoader : ParentSettingsLoader<PropTypeSettings, PropType> { }

    public class PropSettingsMapper : ParentSettingsMapper<PropTypeSettings, PropType, PropTypeSettingsDto> { }

    public class PropEntityHelper : BaseEntityHelper<PropTypeSettings, PropType>
    {
        public override long HelperKey => EntityTypes.Prop;
    }



}


