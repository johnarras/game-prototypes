using OxDb.SharedCore.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class WeightedDungeonAsset : IWeightedItem
    {
        public WeightedDungeonAsset()
        {
            Weight = 1000;
        }

        [field: SerializeField]
        public double Weight { get; set; }
        public DungeonAsset Asset;
    }
}


