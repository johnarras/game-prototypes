using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Upgrades.Settings
{
    public class UpgradeReasonSettings : ParentConstantListSettings<UpgradeReason, UpgradeReasons>
    {
        public override string Id { get; set; }
    }

    public class UpgradeReason : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public int GamePoints { get; set; }
        public int RunPoints { get; set; }
        public bool AlwaysSingleLevel { get; set; }

    }


    public class UpgradeReasonSettingsDto : ParentSettingsDto<UpgradeReasonSettings, UpgradeReason>
    {
        public override List<UpgradeReason> Children { get; set; }
        public override UpgradeReasonSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class UpgradeReasonSettingsLoader : ParentSettingsLoader<UpgradeReasonSettings, UpgradeReason> { }

    public class UpgradeReasonSettingsMapper : ParentSettingsMapper<UpgradeReasonSettings, UpgradeReason, UpgradeReasonSettingsDto> { }

}


