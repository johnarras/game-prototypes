using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Stats.Constants;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Stats.Settings.Stats
{/// <summary>
 /// Stats have current core stats:
 /// Health/Mana/Might/Intellect/Willpower/Agility
 /// </summary>
    public class StatType : ChildSettings, IIndexedGameItem
    {


        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Abbrev { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }

        public int MaxPool { get; set; }
        public int RegenSeconds { get; set; }
        public int GenScalePct { get; set; }
        public long BonusStatTypeId { get; set; }
        public bool IsCrawlerStat { get; set; }

    }

    public class StatSettings : ParentConstantListSettings<StatType, StatTypes>
    {
        public override string Id { get; set; }
        public int StatConstantUnitMultiple { get; set; }

        public List<StatType> GetPowerStats()
        {
            return _data.Where(x => x.IdKey <= StatConstants.MaxMutableStatTypeId &&
            x.IdKey > 0 && x.IdKey != StatTypes.Health).ToList();
        }
    }

    public class StatSettingsDto : ParentSettingsDto<StatSettings, StatType>
    {
        public override List<StatType> Children { get; set; }
        public override StatSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class StatSettingsLoader : ParentSettingsLoader<StatSettings, StatType> { }

    public class StatSettingsMapper : ParentSettingsMapper<StatSettings, StatType, StatSettingsDto> { }


    public class StatTypeHelper : BaseEntityHelper<StatSettings, StatType>
    {
        public override long HelperKey => EntityTypes.Stat;
    }
    public class StatBonusTypeHelper : BaseEntityHelper<StatSettings, StatType>
    {
        public override long HelperKey => EntityTypes.StatBonus;
    }
    public class StatPctHelper : BaseEntityHelper<StatSettings, StatType>
    {
        public override long HelperKey => EntityTypes.StatPct;
    }
}


