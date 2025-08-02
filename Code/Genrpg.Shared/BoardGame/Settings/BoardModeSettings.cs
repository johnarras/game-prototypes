using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Vendors.WorldData;
using Genrpg.Shared.Achievements.Settings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.BoardGame.Prizes.Settings;

namespace Genrpg.Shared.BoardGame.Settings
{
    [MessagePackObject]
    public class BoardModeSettings : ParentSettings<BoardMode>
    {
        [Key(0)] public override string Id { get; set; }
    }


    [MessagePackObject]
    public class BoardModePrizeRule
    {
        [Key(0)] public long PathIndex { get; set; }

        [Key(1)] public bool SpawnOnce { get; set; }

        [Key(2)] public List<PrizeSpawn> PassPrizes { get; set; } = new List<PrizeSpawn>();
        [Key(3)] public List<PrizeSpawn> LandPrizes { get; set; } = new List<PrizeSpawn>();


        [Key(4)] public double PassPercent { get; set; }
        [Key(5)] public double LandPercent { get; set; }
    }

    [MessagePackObject]
    public class BoardMode : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

        [Key(8)] public int DiceCount { get; set; }
        [Key(9)] public bool FreeRolls { get; set; }
        [Key(10)] public bool GiveDefaultTileRewards { get; set; }

        [Key(11)] public List<BoardModePrizeRule> PrizeRules { get; set; } = new List<BoardModePrizeRule>();

    }

    public class BoardModeSettingsDto : ParentSettingsDto<BoardModeSettings, BoardMode> { }

    public class BoardModeSettingsLoader : ParentSettingsLoader<BoardModeSettings, BoardMode> { }

    public class BoardModeSettingsMapper : ParentSettingsMapper<BoardModeSettings, BoardMode, BoardModeSettingsDto> { }
}
