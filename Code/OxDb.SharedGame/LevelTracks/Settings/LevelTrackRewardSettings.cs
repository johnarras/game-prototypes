using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.LevelTracks.Settings
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
        public long StartCaravanMemberId { get; set; }
        public long StartSkinTypeId { get; set; }
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


