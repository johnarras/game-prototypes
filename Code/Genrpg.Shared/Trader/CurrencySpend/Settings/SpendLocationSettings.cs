using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.CurrencySpend.Settings
{
    public class SpendLocationSettings : ParentSettings<SpendLocation>
    {
        public override string Id { get; set; }
    }

    public class SpendLocation : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public List<SpendType> SpendTypes { get; set; } = new List<SpendType>();
    }

    public class SpendReward : IEffect
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long Quantity { get; set; }
    }

    public class SpendType
    {
        public long Index { get; set; }

        public string Name { get; set; }

        public string Desc { get; set; }
        public long SpendCoreCurrencyTypeId { get; set; }
        public long SpendQuantity { get; set; }

        public int MinLevel { get; set; }

        public int MaxTimes { get; set; }

        public List<SpendReward> Rewards { get; set; } = new List<SpendReward>();

    }

    public class SpendCurrencyLocationSettingsDto : ParentSettingsDto<SpendLocationSettings, SpendLocation>
    {
        public override List<SpendLocation> Children { get; set; }
        public override SpendLocationSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class SpendCurrencyLocationSettingsLoader : ParentSettingsLoader<SpendLocationSettings, SpendLocation> { }

    public class SpendCurrencyLocationSettingsMapper : ParentSettingsMapper<SpendLocationSettings, SpendLocation, SpendCurrencyLocationSettingsDto> { }

    public class SpendCurrencyLocationEntityHelper : BaseEntityHelper<SpendLocationSettings, SpendLocation>
    {
        public override long HelperKey => EntityTypes.SpendCurrencyLocation;
    }
}


