using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class DungeonDoorAssetBlock : BaseBehaviour, IWeightedItem
    {
        [field: SerializeField]
        public double Weight { get; set; }
        public List<WeightedDungeonAsset> DoorFrames = new List<WeightedDungeonAsset>();

        public List<WeightedDungeonDoor> Doors = new List<WeightedDungeonDoor>();

    }
}
