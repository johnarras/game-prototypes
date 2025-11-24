using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.GuildDegrees.Settings
{
    [MessagePackObject]
    public class GuildDegree : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public long ExpRequired { get; set; }
        [Key(9)] public List<Reward> Rewards { get; set; } = new List<Reward>();


        public GuildDegree()
        {
        }
    }

    [MessagePackObject]
    public class GuildDegreeSettings : ParentSettings<GuildDegree>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class GuildDegreeSettingsDto : ParentSettingsDto<GuildDegreeSettings, GuildDegree> { }

    public class GuildDegreeSettingsLoader : ParentSettingsLoader<GuildDegreeSettings, GuildDegree> { }

    public class GuildDegreeSettingsMapper : ParentSettingsMapper<GuildDegreeSettings, GuildDegree, GuildDegreeSettingsDto> { }

}
