using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Assets.Entities
{
    public class BundleCacheData
    {
        public string name;
        public AssetBundle assetBundle;
        public DateTime LastUsed = DateTime.UtcNow;
        public int LoadingCount;
        public Dictionary<string, object> LoadedAssets = new Dictionary<string, object>();
        public List<object> Instances = new List<object>();
        public bool KeepLoaded = false;
        // Have a couple of ticks of not using a bundle before we try to delete it.
        public int DeleteTicks = 0;
        public List<BundleCacheData> ParentDependencies { get; set; } = new List<BundleCacheData>();
        public List<BundleCacheData> ChildDependencies { get; set; } = new List<BundleCacheData>();
    }

}
