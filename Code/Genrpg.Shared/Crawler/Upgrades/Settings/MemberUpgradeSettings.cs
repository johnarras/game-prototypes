using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Stats.Settings.Stats;
using System.Linq;

namespace Genrpg.Shared.Crawler.Upgrades.Settings
{
    [MessagePackObject]
    public class MemberUpgradeSettings : ParentSettings<MemberUpgrade>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int LevelsPerPoint { get; set; }
        [Key(2)] public int MaxTier { get; set; }


        public MemberUpgrade Get(long entityTypeId, long entityId)
        {
            return _data.FirstOrDefault(x=>x.EntityTypeId == entityTypeId && x.EntityId == entityId);   
        }
    }

    [MessagePackObject]
    public class MemberUpgrade : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public double BonusPerTier { get; set; }
        [Key(9)] public long EntityTypeId { get; set; }
        [Key(10)] public long EntityId { get; set; }


    }


    public class MemberUpgradeSettingsDto : ParentSettingsDto<MemberUpgradeSettings, MemberUpgrade> { }
    public class MemberUpgradeSettingsLoader : ParentSettingsLoader<MemberUpgradeSettings, MemberUpgrade> { }

    public class MemberUpgradeSettingsMapper : ParentSettingsMapper<MemberUpgradeSettings, MemberUpgrade, MemberUpgradeSettingsDto> { }


    public class MemberUpgradeHelper : BaseEntityHelper<MemberUpgradeSettings, MemberUpgrade>
    {
        public override long Key => EntityTypes.MemberUpgrades;
    }
}
