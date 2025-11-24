using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.Settings
{

    [MessagePackObject]
    public class StoreBundle
    {
        [Key(0)] public long Index { get; set; }
        [Key(1)] public long ProductSkuId { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public string BundleId { get; set; }
        [Key(4)] public List<Reward> Rewards { get; set; } = new List<Reward>();
    }

    [MessagePackObject]
    public class StoreBundleSet : ChildSettings, IIdName, INameId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string NameId { get; set; }
        [Key(5)] public List<StoreBundle> Bundles { get; set; } = new List<StoreBundle>();

    }

    [MessagePackObject]
    public class StoreBundleSetSettings : ParentSettings<StoreBundleSet>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class StoreBundleSetLoader : ParentSettingsLoader<StoreBundleSetSettings, StoreBundleSet> { }
}
