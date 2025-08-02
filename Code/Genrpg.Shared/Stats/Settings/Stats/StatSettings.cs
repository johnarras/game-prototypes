using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using System.Linq;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.Stats.Settings.Stats
{/// <summary>
 /// Stats have current core stats:
 /// Health/Mana/Might/Intellect/Willpower/Agility
 /// </summary>
    [MessagePackObject]
    public class StatType : ChildSettings, IIndexedGameItem
    {


        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Abbrev { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string AtlasPrefix { get; set; }
        [Key(7)] public string Icon { get; set; }
        [Key(8)] public string Art { get; set; }
        [Key(9)] public string ColorName { get; set; }
        [Key(10)] public string ColorCode { get; set; }

        [Key(11)] public int MaxPool { get; set; }
        [Key(12)] public int RegenSeconds { get; set; }
        [Key(13)] public int GenScalePct { get; set; }
        [Key(14)] public long BonusStatTypeId { get; set; }
        [Key(15)] public bool IsCrawlerStat { get; set; }

    }

    [MessagePackObject]
    public class StatSettings : ParentConstantListSettings<StatType, StatTypes>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int StatConstantUnitMultiple { get; set; }

        public List<StatType> GetPowerStats()
        {
            return _data.Where(x => x.IdKey <= StatConstants.MaxMutableStatTypeId &&
            x.IdKey > 0 && x.IdKey != StatTypes.Health).ToList();
        }
    }

    public class StatSettingsDto : ParentSettingsDto<StatSettings, StatType> { }
    public class StatSettingsLoader : ParentSettingsLoader<StatSettings, StatType> { }

    public class StatSettingsMapper : ParentSettingsMapper<StatSettings, StatType, StatSettingsDto> { }


    public class StatTypeHelper : BaseEntityHelper<StatSettings, StatType>
    {
        public override long Key => EntityTypes.Stat;
    }
    public class StatBonusTypeHelper : BaseEntityHelper<StatSettings, StatType>
    {
        public override long Key => EntityTypes.StatBonus;
    }
    public class StatPctHelper : BaseEntityHelper<StatSettings, StatType>
    {
        public override long Key => EntityTypes.StatPct;
    }
}
