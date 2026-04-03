using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.LevelTracks.Settings
{
    public class LevelTrackReward : ChildSettings, IIndexedGameItem, IEffect
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long Quantity { get; set; }
        public long Level { get; set; }
    }

    public class LevelTrackRewardSettings : ParentSettings<LevelTrackReward>
    {
        public override string Id { get; set; }
        public long StartCityId { get; set; }
    }

    public class LevelTrackRewardSettingsDto : ParentSettingsDto<LevelTrackRewardSettings, LevelTrackReward>
    {
        public override List<LevelTrackReward> Children { get; set; }
        public override LevelTrackRewardSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class LevelTrackRewardSettingsLoader : ParentSettingsLoader<LevelTrackRewardSettings, LevelTrackReward> { }

    public class LevelTrackRewardSettingsMapper : ParentSettingsMapper<LevelTrackRewardSettings, LevelTrackReward, LevelTrackRewardSettingsDto> { }

}


