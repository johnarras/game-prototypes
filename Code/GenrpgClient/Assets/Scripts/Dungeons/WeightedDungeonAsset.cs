using Genrpg.Shared.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class WeightedDungeonAsset : IFloatWeightedItem
    {
        public WeightedDungeonAsset()
        {
            Weight = 1000;
        }

        [field: SerializeField]
        public float Weight { get; set; }
        public DungeonAsset Asset;
    }
}


