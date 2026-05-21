using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Crawler.Upgrades.Settings
{
    public class MemberUpgradeSettings : ParentSettings<MemberUpgrade>
    {
        public override string Id { get; set; }
        public int LevelsPerPoint { get; set; }
        public int MaxTier { get; set; }


        public MemberUpgrade Get(long entityTypeId, long entityId)
        {
            return _data.FirstOrDefault(x => x.EntityTypeId == entityTypeId && x.EntityId == entityId);
        }
    }

    public class MemberUpgrade : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public double BonusPerTier { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }


    }


    public class MemberUpgradeSettingsDto : ParentSettingsDto<MemberUpgradeSettings, MemberUpgrade>
    {
        public override List<MemberUpgrade> Children { get; set; }
        public override MemberUpgradeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class MemberUpgradeSettingsLoader : ParentSettingsLoader<MemberUpgradeSettings, MemberUpgrade> { }

    public class MemberUpgradeSettingsMapper : ParentSettingsMapper<MemberUpgradeSettings, MemberUpgrade, MemberUpgradeSettingsDto> { }


    public class MemberUpgradeHelper : BaseEntityHelper<MemberUpgradeSettings, MemberUpgrade>
    {
        public override long HelperKey => EntityTypes.MemberUpgrades;
    }
}


