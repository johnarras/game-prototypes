using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Buffs.Interfaces;
using Genrpg.Shared.Trader.Flags.Constants;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Flags.Settings
{
    public class TraderFlagSettings : ParentConstantListSettings<TraderFlag, TraderFlags>
    {
        public override string Id { get; set; }
    }

    public class TraderFlag : ChildSettings, IIndexedGameItem, ITravelBuff
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public bool IsBad { get; set; }
        public bool IsBuff { get; set; }
        public long BonusSpeed { get; set; }
        public double ForageChance { get; set; }
        public long RationsCost { get; set; }
        public double GoodEventChance { get; set; }
        public double BadEventChance { get; set; }
        public long Capacity { get; set; }
    }

    public class TraderFlagSettingsDto : ParentSettingsDto<TraderFlagSettings, TraderFlag>
    {
        public override List<TraderFlag> Children { get; set; }
        public override TraderFlagSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TraderFlagSettingsLoader : ParentSettingsLoader<TraderFlagSettings, TraderFlag> { }

    public class TraderFlagSettingsMapper : ParentSettingsMapper<TraderFlagSettings, TraderFlag, TraderFlagSettingsDto> { }

    public class TraderFlagEntityHelper : BaseEntityHelper<TraderFlagSettings, TraderFlag>
    {
        public override long HelperKey => EntityTypes.TradeGood;
    }
}


