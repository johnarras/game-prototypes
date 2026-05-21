using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;

using System.Collections.Generic;

namespace OxDb.SharedGame.RpgLevels.Settings
{
    public class RpgLevel : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public List<Reward> RewardList { get; set; }

        public long CurrExp { get; set; }
        public float MobCount { get; set; }
        public long MobExp { get; set; }
        public float QuestCount { get; set; }
        public long QuestExp { get; set; }
        public long KillMoney { get; set; }

        public int StatAmount { get; set; }
        public int MonsterStatScale { get; set; }

        public int AbilityPoints { get; set; }

        public string Art { get; set; }


        public RpgLevel()
        {
            RewardList = new List<Reward>();
        }
    }

    public class RpgLevelSettings : ParentSettings<RpgLevel>
    {
        public override string Id { get; set; }
        public int MaxLevel { get; set; }
    }

    public class RpgLevelSettingsDto : ParentSettingsDto<RpgLevelSettings, RpgLevel>
    {
        public override List<RpgLevel> Children { get; set; }
        public override RpgLevelSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class RpgLevelSettingsLoader : ParentSettingsLoader<RpgLevelSettings, RpgLevel> { }

    public class RpgLevelSettingsMapper : ParentSettingsMapper<RpgLevelSettings, RpgLevel, RpgLevelSettingsDto> { }

}


