using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Ads.Constants;
using OxDb.SharedGame.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Ads.Settings
{
    public class AdUnitSettings : ParentConstantListSettings<AdUnit,AdUnits>
    {
        public override string Id { get; set; }
    }

    public class AdUnit : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public float RegenHours { get; set; }

        public List<Reward> Rewards { get; set; } = new List<Reward>();
    }


    public class AdUnitSettingsDto : ParentSettingsDto<AdUnitSettings, AdUnit>
    {
        public override List<AdUnit> Children { get; set; }
        public override AdUnitSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class AdUnitSettingsLoader : ParentSettingsLoader<AdUnitSettings, AdUnit> { }

    public class AdUnitSettingsMapper : ParentSettingsMapper<AdUnitSettings, AdUnit, AdUnitSettingsDto> { }

    public class AdUnitEntityHelper : BaseEntityHelper<AdUnitSettings, AdUnit>
    {
        public override long HelperKey => EntityTypes.AdUnit;
    }
}


