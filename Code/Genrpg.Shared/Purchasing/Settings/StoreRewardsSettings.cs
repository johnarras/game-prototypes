using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.Settings
{
    [MessagePackObject]
    public class StoreRewardsSettings : ParentSettings<StoreRewards>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class StoreRewardsSettingsDto : ParentSettingsDto<StoreRewardsSettings, StoreRewards> { }
    public class StoreRewardsSettingsLoader : ParentSettingsLoader<StoreRewardsSettings, StoreRewards>
    {
    }

    public class StoreRewardsSettingsMapper : ParentSettingsMapper<StoreRewardsSettings, StoreRewards, StoreRewardsSettingsDto>
    {
        public override bool SendToClient() { return false; }
    }

    [MessagePackObject]
    public class StoreRewards : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public List<Reward> Rewards { get; set; } = new List<Reward>();
    }
}
