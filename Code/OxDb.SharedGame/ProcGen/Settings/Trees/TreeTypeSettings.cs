using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using System.Collections.Generic;

namespace OxDb.SharedGame.ProcGen.Settings.Trees
{
    public class TreeTypeSettings : ParentSettings<TreeType>
    {
        public override string Id { get; set; }


        public float TallChance { get; set; } = 0.5f;
        public float TreeDirtRadius { get; set; } = 9.0f;
    }

    public class TreeTypeSettingsDto : ParentSettingsDto<TreeTypeSettings, TreeType>
    {
        public override List<TreeType> Children { get; set; }
        public override TreeTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class TreeTypeSettingsLoader : ParentSettingsLoader<TreeTypeSettings, TreeType> { }

    public class TreeSettingsMapper : ParentSettingsMapper<TreeTypeSettings, TreeType, TreeTypeSettingsDto> { }

    public class TreeEntityHelper : BaseEntityHelper<TreeTypeSettings, TreeType>
    {
        public override long HelperKey => EntityTypes.Tree;
    }
}


