using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Attributes.Settings
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


