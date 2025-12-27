using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.Shared.NewPlayers.Settings
{
    public class NewPlayerBonusSettings : ParentSettings<NewPlayerBonus>
    {
        public override string Id { get; set; }
        public long StartCityId { get; set; }
    }

    public class NewPlayerBonus : ChildSettings, IIndexedGameItem, IReward
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
        public long QualityTypeId { get; set; }
        public long Level { get; set; }
        public Item ExtraData { get; set; }
    }

    public class NewPlayerBonusSettingsLoader : ParentSettingsLoader<NewPlayerBonusSettings, NewPlayerBonus>
    {

    }
}


