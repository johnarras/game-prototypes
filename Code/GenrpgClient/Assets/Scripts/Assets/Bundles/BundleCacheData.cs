using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Assets.Entities
{
    public class BundleCacheData
    {
        public string Name;
        public AssetBundle assetBundle;
        public DateTime LastUsed = DateTime.UtcNow;
        public int LoadingCount;
        public Dictionary<string, GameObject> LoadedAssets = new Dictionary<string, GameObject>();
        public List<GameObject> Instances = new List<GameObject>();
        // Have a couple of ticks of not using a bundle before we try to delete it.
        public int DeleteTicks = 0;
        public List<BundleCacheData> ParentDependencies { get; set; } = new List<BundleCacheData>();
        public List<BundleCacheData> ChildDependencies { get; set; } = new List<BundleCacheData>();
    }

}


