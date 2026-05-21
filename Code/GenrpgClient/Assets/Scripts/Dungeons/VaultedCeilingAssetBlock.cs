using OxDb.SharedCore.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class VaultedCeilingAssetBlock : BaseBehaviour, IWeightedItem
    {
        [field: SerializeField]
        public double Weight { get; set; }
        public DungeonAsset LowCeiling;
        public DungeonAsset HighCeiling;
        public DungeonAsset OneCornerUp;
        public DungeonAsset OneEdgeUp;
        public DungeonAsset SaddlePoint;
        public DungeonAsset ThreeCornersUp;

        public bool IsValid()
        {
            return Weight > 0 &&
                LowCeiling != null &&
                HighCeiling != null &&
                OneCornerUp != null &&
                OneEdgeUp != null &&
                SaddlePoint != null &&
                ThreeCornersUp != null;
        }

    }
}
