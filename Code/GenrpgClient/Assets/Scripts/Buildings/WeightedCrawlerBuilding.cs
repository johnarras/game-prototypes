using Genrpg.Shared.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.Buildings
{
    [Serializable]
    public class WeightedCrawlerBuilding : IFloatWeightedItem
    {
        [field: SerializeField]
        public float Weight { get; set; }
        public CrawlerBuilding Building;
        public BuildingMats Mats;
    }
}


