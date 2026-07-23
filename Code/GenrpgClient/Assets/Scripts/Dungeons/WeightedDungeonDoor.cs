using OxDb.SharedCore.Utils;
using System;
using UnityEngine;

namespace OxDb.Client.Dungeons
{
    [Serializable]
    public class WeightedDungeonDoor : IWeightedItem
    {
        [field: SerializeField]
        public double Weight { get; set; }

        public DungeonDoor Door;
    }
}
