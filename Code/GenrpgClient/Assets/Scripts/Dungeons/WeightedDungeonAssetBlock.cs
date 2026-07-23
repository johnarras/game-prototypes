using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace OxDb.Client.Dungeons
{
    [Serializable]
    public class WeightedDungeonAssetBlock : IWeightedItem
    {
        [field: SerializeField]
        public double Weight { get; set; }

        // Must be in same order as DungeonMaterials Material order
        public List<WeightedDungeonAsset> Walls = new List<WeightedDungeonAsset>();
        public List<WeightedDungeonAsset> Floors = new List<WeightedDungeonAsset>();
        public List<WeightedDungeonAsset> Ceilings = new List<WeightedDungeonAsset>();
        public List<WeightedDungeonAsset> Pillars = new List<WeightedDungeonAsset>();
        public List<WeightedDungeonAsset> Fences = new List<WeightedDungeonAsset>();


        public List<DungeonDoorAssetBlock> Doors = new List<DungeonDoorAssetBlock>();


        public List<WeightedDungeonAsset> GetAssetList(int assetIndex)
        {
            if (assetIndex == DungeonPrefabIndexes.Walls)
            {
                return Walls;
            }
            else if (assetIndex == DungeonPrefabIndexes.Floors)
            {
                return Floors;
            }
            else if (assetIndex == DungeonPrefabIndexes.Ceilings)
            {
                return Ceilings;
            }
            else if (assetIndex == DungeonPrefabIndexes.Pillars)
            {
                return Pillars;
            }
            else if (assetIndex == DungeonPrefabIndexes.Fences)
            {
                return Fences;
            }
            return Walls;
        }


        private void DestroyAssetList(List<WeightedDungeonAsset> list)
        {
            foreach (WeightedDungeonAsset asset in list)
            {
                if (asset.Asset != null)
                {
                    asset.Asset.Clear();
                }
            }
            list.Clear();
        }


        public void Clear()
        {
            for (int i = 0; i < DungeonPrefabIndexes.Max; i++)
            {
                DestroyAssetList(GetAssetList(i));
            }
        }
    }
}


