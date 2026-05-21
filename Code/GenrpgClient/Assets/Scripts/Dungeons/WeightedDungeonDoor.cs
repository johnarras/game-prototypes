using OxDb.SharedCore.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class WeightedDungeonDoor : IWeightedItem
    {
        [field: SerializeField]
        public double Weight { get; set; }

        public DungeonDoor Door;
    }
}
