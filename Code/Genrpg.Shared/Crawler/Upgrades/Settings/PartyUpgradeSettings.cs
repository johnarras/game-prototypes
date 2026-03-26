using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Upgrades.Settings
{
    public class PartyUpgradeSettings : ParentConstantListSettings<PartyUpgrade, PartyUpgrades>
    {
        public override string Id { get; set; }
    }

    public class PartyUpgrade : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long MaxTier { get; set; }
        public double BonusPerTier { get; set; }
        public long BasePointCost { get; set; }


    }


    public class PartyUpgradeSettingsDto : ParentSettingsDto<PartyUpgradeSettings, PartyUpgrade>
    {
        public override List<PartyUpgrade> Children { get; set; }
        public override PartyUpgradeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class PartyUpgradeSettingsLoader : ParentSettingsLoader<PartyUpgradeSettings, PartyUpgrade> { }

    public class PartyUpgradeSettingsMapper : ParentSettingsMapper<PartyUpgradeSettings, PartyUpgrade, PartyUpgradeSettingsDto> { }



    public class PartyUpgradeHelper : BaseEntityHelper<PartyUpgradeSettings, PartyUpgrade>
    {
        public override long HelperKey => EntityTypes.PartyUpgrades;
    }
}


