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
    public class GameplayDebuffSettings : ParentSettings<GameplayDebuff>
    {
        public override string Id { get; set; }
    }

    public class GameplayDebuff : ChildSettings, IIndexedGameItem, IEffectList<Effect>
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public long CleanseCoreCurrencyTypeId { get; set; }

        public long CleanseQuantity { get; set; }

        public long CityCoinsCleanseCost { get; set; }

        public List<Effect> Effects { get; set; } = new List<Effect>();
    }


    public class GameplayDebuffSettingsDto : ParentSettingsDto<GameplayDebuffSettings, GameplayDebuff>
    {
        public override List<GameplayDebuff> Children { get; set; }
        public override GameplayDebuffSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class GameplayDebuffSettingsLoader : ParentSettingsLoader<GameplayDebuffSettings, GameplayDebuff> { }

    public class GameplayDebuffSettingsMapper : ParentSettingsMapper<GameplayDebuffSettings, GameplayDebuff, GameplayDebuffSettingsDto> { }

    public class GameplayDebuffEntityHelper : BaseEntityHelper<GameplayDebuffSettings, GameplayDebuff>
    {
        public override long HelperKey => EntityTypes.GameplayDebuff;
    }
}


