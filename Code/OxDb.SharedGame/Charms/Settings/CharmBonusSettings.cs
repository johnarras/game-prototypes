using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Charms.Settings
{
    public class CharmBonus : ChildSettings, IIdName
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string NameId { get; set; }
        public string Desc { get; set; }
        public string Icon { get; set; }

        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }

        public long CharmUseId { get; set; }

        public bool CheckBitValue { get; set; }
        public long CheckBitCount { get; set; }
        public long CheckBitsMatchTarget { get; set; }

        public long CheckBitStartIndex { get; set; }
        public long CheckBitSkip { get; set; }

        public long BonusQuantityStart { get; set; }
        public long BonusQuantitySkip { get; set; }

        public long QuantityBitsCount { get; set; }
        public long QuantityMod { get; set; }

        public long QuantityBitSkip { get; set; }
        public long QuantityStartBit { get; set; }
        public long QuantityBonusType { get; set; }

    }
    public class CharmBonusSettings : ParentSettings<CharmBonus>
    {
        public override string Id { get; set; }
    }

    public class CharmBonusSettingsDto : ParentSettingsDto<CharmBonusSettings, CharmBonus>
    {
        public override List<CharmBonus> Children { get; set; }
        public override CharmBonusSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class CharmBonusSettingsLoader : ParentSettingsLoader<CharmBonusSettings, CharmBonus> { }

    public class CharmBonusSettingsMapper : ParentSettingsMapper<CharmBonusSettings, CharmBonus, CharmBonusSettingsDto> { }
}


