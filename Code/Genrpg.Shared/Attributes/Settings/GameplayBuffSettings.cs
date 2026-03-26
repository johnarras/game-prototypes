using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Attributes.Settings
{
    public class GameplayBuffSettings : ParentSettings<GameplayBuff>
    {
        public override string Id { get; set; }
    }

    public class GameplayBuff : ChildSettings, IIndexedGameItem, IEffectList<Effect>
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public List<Effect> Effects { get; set; } = new List<Effect>();
    }


    public class GameplayBuffSettingsDto : ParentSettingsDto<GameplayBuffSettings, GameplayBuff>
    {
        public override List<GameplayBuff> Children { get; set; }
        public override GameplayBuffSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class GameplayBuffSettingsLoader : ParentSettingsLoader<GameplayBuffSettings, GameplayBuff> { }

    public class GameplayBuffSettingsMapper : ParentSettingsMapper<GameplayBuffSettings, GameplayBuff, GameplayBuffSettingsDto> { }

    public class GameplayBuffEntityHelper : BaseEntityHelper<GameplayBuffSettings, GameplayBuff>
    {
        public override long HelperKey => EntityTypes.GameplayBuff;
    }
}


