using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using MessagePack;

namespace Genrpg.Shared.NewPlayers.Settings
{
    [MessagePackObject]
    public class NewPlayerBonusSettings : ParentSettings<NewPlayerBonus>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class NewPlayerBonus : ChildSettings, IIndexedGameItem, IReward
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public long EntityTypeId { get; set; }
        [Key(9)] public long EntityId { get; set; }
        [Key(10)] public long Quantity { get; set; }
        [Key(11)] public long QualityTypeId { get; set; }
        [Key(12)] public long Level { get; set; }
        [Key(13)] public Item ExtraData { get; set; }
    }

    public class NewPlayerBonusSettingsLoader : ParentSettingsLoader<NewPlayerBonusSettings, NewPlayerBonus>
    {

    }
}
