using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;

using System.Collections.Generic;

namespace OxDb.SharedGame.Purchasing.Settings
{

    public class StoreBundle
    {
        public long Index { get; set; }
        public long ProductSkuId { get; set; }
        public string Name { get; set; }
        public string BundleId { get; set; }
        public List<Reward> Rewards { get; set; } = new List<Reward>();
    }

    public class StoreBundleSet : ChildSettings, IIdName, INameId
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string NameId { get; set; }
        public List<StoreBundle> Bundles { get; set; } = new List<StoreBundle>();

    }

    public class StoreBundleSetSettings : ParentSettings<StoreBundleSet>
    {
        public override string Id { get; set; }
    }

    public class StoreBundleSetLoader : ParentSettingsLoader<StoreBundleSetSettings, StoreBundleSet> { }
}


