using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.Settings
{

    [MessagePackObject]
    public class StoreBundle
    {
        [Key(0)] public long Index { get; set; }
        [Key(1)] public long StoreRewardsId { get; set; }
        [Key(2)] public long ProductSkuId { get; set; }
        [Key(3)] public string Name { get; set; }
        [Key(4)] public string BundleId { get; set; }
    }

    [MessagePackObject]
    public class StoreBundleSet : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public List<StoreBundle> Bundles { get; set; } = new List<StoreBundle>();

    }

    [MessagePackObject]
    public class StoreBundleSetSettings : ParentSettings<StoreBundleSet>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class StoreBundleSetLoader : ParentSettingsLoader<StoreBundleSetSettings, StoreBundleSet> { }
}
