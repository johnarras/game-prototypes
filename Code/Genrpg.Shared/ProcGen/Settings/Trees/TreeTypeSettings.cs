using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.ProcGen.Settings.Trees
{
    [MessagePackObject]
    public class TreeTypeSettings : ParentSettings<TreeType>
    {
        [Key(0)] public override string Id { get; set; }


        [Key(1)] public float TallChance { get; set; } = 0.5f;
        [Key(2)] public float TreeDirtRadius { get; set; } = 9.0f;
    }

    public class TreeTypeSettingsDto : ParentSettingsDto<TreeTypeSettings, TreeType> { }
    public class TreeTypeSettingsLoader : ParentSettingsLoader<TreeTypeSettings, TreeType> { }

    public class TreeSettingsMapper : ParentSettingsMapper<TreeTypeSettings, TreeType, TreeTypeSettingsDto> { }

    public class TreeEntityHelper : BaseEntityHelper<TreeTypeSettings,TreeType>
    {
        public override long Key => EntityTypes.Tree;
    }
}
