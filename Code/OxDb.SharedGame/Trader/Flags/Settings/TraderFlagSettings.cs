using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Trader.Flags.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.Flags.Settings
{
    public class TraderFlagSettings : ParentConstantListSettings<TraderFlag, TraderFlags>
    {
        public override string Id { get; set; }
    }

    public class TraderFlag : ChildSettings, IIndexedGameItem
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
        public override long HelperKey => EntityTypes.TraderFlag;
    }
}


