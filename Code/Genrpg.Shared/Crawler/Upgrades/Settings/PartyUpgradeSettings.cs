using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Crawler.Upgrades.Constants;

namespace Genrpg.Shared.Crawler.Upgrades.Settings
{
    [MessagePackObject]
    public class PartyUpgradeSettings : ParentConstantListSettings<PartyUpgrade,PartyUpgrades>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class PartyUpgrade : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string NameId { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string AtlasPrefix { get; set; }
        [Key(7)] public string Icon { get; set; }
        [Key(8)] public string Art { get; set; }
        [Key(9)] public long MaxTier { get; set; }
        [Key(10)] public double BonusPerTier { get; set; }
        [Key(11)] public long BasePointCost { get; set; }


    }


    public class PartyUpgradeSettingsDto : ParentSettingsDto<PartyUpgradeSettings, PartyUpgrade> { }
    public class PartyUpgradeSettingsLoader : ParentSettingsLoader<PartyUpgradeSettings, PartyUpgrade> { }

    public class PartyUpgradeSettingsMapper : ParentSettingsMapper<PartyUpgradeSettings, PartyUpgrade, PartyUpgradeSettingsDto> { }



    public class PartyUpgradeHelper : BaseEntityHelper<PartyUpgradeSettings, PartyUpgrade>
    {
        public override long Key => EntityTypes.PartyUpgrades;
    }
}
