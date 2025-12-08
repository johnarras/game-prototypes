using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Stats.Constants;
using MessagePack;

namespace Genrpg.Shared.Trader.Stats.Settings
{
    [MessagePackObject]
    public class TraderStatSettings : ParentConstantListSettings<TraderStat, TraderStats>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class TraderStat : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
    }

    public class TraderStatSettingsDto : ParentSettingsDto<TraderStatSettings, TraderStat> { }

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
