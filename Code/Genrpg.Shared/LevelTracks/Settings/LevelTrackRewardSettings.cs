using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using MessagePack;

namespace Genrpg.Shared.LevelTracks.Settings
{
    [MessagePackObject]
    public class LevelTrackReward : ChildSettings, IIndexedGameItem, IReward
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public long Exp { get; set; }
        [Key(9)] public long EntityTypeId { get; set; }
        [Key(10)] public long EntityId { get; set; }
        [Key(11)] public long Quantity { get; set; }
        [Key(12)] public long QualityTypeId { get; set; }
        [Key(13)] public long Level { get; set; }
        [Key(14)] public Item ExtraData { get; set; }
    }

    [MessagePackObject]
    public class LevelTrackRewardSettings : ParentSettings<LevelTrackReward>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class LevelTrackRewardSettingsDto : ParentSettingsDto<LevelTrackRewardSettings, LevelTrackReward> { }

    public class LevelTrackRewardSettingsLoader : ParentSettingsLoader<LevelTrackRewardSettings, LevelTrackReward> { }

    public class LevelTrackRewardSettingsMapper : ParentSettingsMapper<LevelTrackRewardSettings, LevelTrackReward, LevelTrackRewardSettingsDto> { }

}
