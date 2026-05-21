using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Achievements.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Achievements.Settings
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


