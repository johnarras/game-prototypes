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
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.BoardGame.Prizes.Settings
{

    [MessagePackObject]
    public class PrizeSpawn : IWeightedItem
    {
        [Key(0)] public long BoardPrizeId { get; set; }
        [Key(1)] public double Weight { get; set; }
        [Key(2)] public string Name { get; set; }
    }


    [MessagePackObject]
    public class BoardPrizeSettings : ParentSettings<BoardPrize>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class BoardPrize : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public List<SpawnItem> LootTable { get; set; } = new List<SpawnItem>(); // What it actually spawns
    }

    public class BoardPrizeSettingsDto : ParentSettingsDto<BoardPrizeSettings, BoardPrize> { }

    public class BoardPrizeSettingsLoader : ParentSettingsLoader<BoardPrizeSettings, BoardPrize> { }

    public class BoardPrizeSettingsMapper : ParentSettingsMapper<BoardPrizeSettings, BoardPrize, BoardPrizeSettingsDto> { }


    public class BoardPrizeHelper : BaseEntityHelper<BoardPrizeSettings, BoardPrize>
    {
        public override long Key => EntityTypes.BoardPrize;
    }
}
