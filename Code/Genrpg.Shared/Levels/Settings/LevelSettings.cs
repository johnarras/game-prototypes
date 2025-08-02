using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.Shared.Levels.Settings
{
    [MessagePackObject]
    public class LevelInfo : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public List<Reward> RewardList { get; set; }

        [Key(8)] public long CurrExp { get; set; }
        [Key(9)] public float MobCount { get; set; }
        [Key(10)] public long MobExp { get; set; }
        [Key(11)] public float QuestCount { get; set; }
        [Key(12)] public long QuestExp { get; set; }
        [Key(13)] public long KillMoney { get; set; }

        [Key(14)] public int StatAmount { get; set; }
        [Key(15)] public int MonsterStatScale { get; set; }

        [Key(16)] public int AbilityPoints { get; set; }

        [Key(17)] public string Art { get; set; }


        public LevelInfo()
        {
            RewardList = new List<Reward>();
        }
    }

    [MessagePackObject]
    public class LevelSettings : ParentSettings<LevelInfo>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int MaxLevel { get; set; }
    }

    public class LevelSettingsDto : ParentSettingsDto<LevelSettings, LevelInfo> { }

    public class LevelSettingsLoader : ParentSettingsLoader<LevelSettings, LevelInfo> { }

    public class ItemSettingsMapper : ParentSettingsMapper<LevelSettings, LevelInfo, LevelSettingsDto> { }

}
