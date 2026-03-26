using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using System.Collections.Generic;

namespace Genrpg.Shared.ProcGen.Settings.Trees
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


