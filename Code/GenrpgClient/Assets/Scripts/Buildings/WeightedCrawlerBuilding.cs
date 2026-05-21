using OxDb.SharedCore.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.Buildings
{
    [Serializable]
    public class WeightedCrawlerBuilding : IWeightedItem
    {
        [field: SerializeField]
        public double Weight { get; set; }
        public CrawlerBuilding Building;
        public BuildingMats Mats;

        public bool IsReady()
        {
            return Building != null && Mats != null && Mats.IsReady();
        }
    }
}


