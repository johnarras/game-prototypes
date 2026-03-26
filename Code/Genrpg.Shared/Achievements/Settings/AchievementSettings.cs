using Genrpg.Shared.Achievements.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Achievements.Settings
{
    public class AchievementSettings : ParentConstantListSettings<AchievementType, AchievementTypes>
    {
        public override string Id { get; set; }
    }

    public class AchievementType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long Category { get; set; }
    }

    public class AchievementSettingsDto : ParentSettingsDto<AchievementSettings, AchievementType>
    {
        public override List<AchievementType> Children { get; set; }
        public override AchievementSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class AchievementSettingsLoader : ParentSettingsLoader<AchievementSettings, AchievementType> { }

    public class AchievementSettingsMapper : ParentSettingsMapper<AchievementSettings, AchievementType, AchievementSettingsDto> { }

}


