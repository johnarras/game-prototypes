using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Rewards.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Rewards.Settings
{
    public class RewardSourceSettings : ParentConstantListSettings<RewardSourceType, RewardSources>
    {
        public override string Id { get; set; }
    }
    public class RewardSourceType : ChildSettings, IIndexedGameItem
    {
        public const int None = 0;
        public const int Doubloons = 1;

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string PluralName { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

    }
    public class RewardSourceSettingsDto : ParentSettingsDto<RewardSourceSettings, RewardSourceType>
    {
        public override List<RewardSourceType> Children { get; set; }
        public override RewardSourceSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class UnitCoinSettingsLoader : ParentSettingsLoader<RewardSourceSettings, RewardSourceType> { }

    public class RewardSourceSettingsMapper : ParentSettingsMapper<RewardSourceSettings, RewardSourceType, RewardSourceSettingsDto> { }
}


