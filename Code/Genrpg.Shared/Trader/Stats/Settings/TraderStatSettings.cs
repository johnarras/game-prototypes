using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Stats.Constants;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Stats.Settings
{
    public class TraderStatSettings : ParentConstantListSettings<TraderStat, TraderStats>
    {
        public override string Id { get; set; }
    }

    public class TraderStat : ChildSettings, IIndexedGameItem
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

    public class TraderStatSettingsDto : ParentSettingsDto<TraderStatSettings, TraderStat>
    {
        public override List<TraderStat> Children { get; set; }
        public override TraderStatSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TraderStatSettingsLoader : ParentSettingsLoader<TraderStatSettings, TraderStat> { }

    public class TraderStatSettingsMapper : ParentSettingsMapper<TraderStatSettings, TraderStat, TraderStatSettingsDto> { }

    public class TraderStatEntityHelper : BaseEntityHelper<TraderStatSettings, TraderStat>
    {
        public override long HelperKey => EntityTypes.TraderStat;
    }
    public class BaseTraderStatEntityHelper : BaseEntityHelper<TraderStatSettings, TraderStat>
    {
        public override long HelperKey => EntityTypes.BaseTraderStat;
    }
    public class BonusTraderStatEntityHelper : BaseEntityHelper<TraderStatSettings, TraderStat>
    {
        public override long HelperKey => EntityTypes.BonusTraderStat;
    }
}


