using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Spawns.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.Travel.Settings
{
    public class TravelRewardSettings : ParentSettings<TravelReward>
    {
        public override string Id { get; set; }
    }

    public class TravelReward : ChildSettings, ISpawnItem, IIdName
    {

        public override string Id { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public override string ParentId { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long MinQuantity { get; set; }
        public long MaxQuantity { get; set; }
        public double Weight { get; set; }
        public int GroupId { get; set; }
        public long MinLevel { get; set; }
    }

    public class TravelRewardSettingsDto : ParentSettingsDto<TravelRewardSettings, TravelReward>
    {
        public override List<TravelReward> Children { get; set; }
        public override TravelRewardSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TravelRewardSettingsLoader : ParentSettingsLoader<TravelRewardSettings, TravelReward> { }

    public class TravelRewardSettingsMapper : ParentSettingsMapper<TravelRewardSettings, TravelReward, TravelRewardSettingsDto> { }

}


