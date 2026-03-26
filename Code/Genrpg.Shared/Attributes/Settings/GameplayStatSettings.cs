using Genrpg.Shared.Attributes.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Attributes.Settings
{
    public class GameplayStatSettings : ParentConstantListSettings<GameplayStat, GameplayStats>
    {
        public override string Id { get; set; }
    }

    public class GameplayStat : ChildSettings, IIndexedGameItem
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

    public class GameplayStatSettingsDto : ParentSettingsDto<GameplayStatSettings, GameplayStat>
    {
        public override List<GameplayStat> Children { get; set; }
        public override GameplayStatSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class GameplayStatSettingsLoader : ParentSettingsLoader<GameplayStatSettings, GameplayStat> { }

    public class GameplayStatSettingsMapper : ParentSettingsMapper<GameplayStatSettings, GameplayStat, GameplayStatSettingsDto> { }

    public class GameplayStatEntityHelper : BaseEntityHelper<GameplayStatSettings, GameplayStat>
    {
        public override long HelperKey => EntityTypes.GameplayStat;
    }
    public class BaseGameplayStatEntityHelper : BaseEntityHelper<GameplayStatSettings, GameplayStat>
    {
        public override long HelperKey => EntityTypes.BaseGameplayStat;
    }
    public class BonusGameplayStatEntityHelper : BaseEntityHelper<GameplayStatSettings, GameplayStat>
    {
        public override long HelperKey => EntityTypes.BonusGameplayStat;
    }

    public class GameplayStatBuffEntityHelper : BaseEntityHelper<GameplayStatSettings, GameplayStat>
    {
        public override long HelperKey => EntityTypes.GameplayStatBuff;
    }
}


