using System.Collections.Generic;

namespace Assets.Scripts.Dungeons
{
    public class DungeonMaterials : BaseBehaviour
    {
        // Must be in same order as the DungeonAssets asset lists.
        public List<WeightedMaterial> WallMat;
        public List<WeightedMaterial> DoorMat;
        public List<WeightedMaterial> CeilingMat;
        public List<WeightedMaterial> FloorMat;
        public List<WeightedMaterial> PillarMat;

        public List<WeightedMaterial> GetMaterials(int assetIndex)
        {
            if (assetIndex == DungeonAssetIndex.Walls)
            {
                return WallMat;
            }
            else if (assetIndex == DungeonAssetIndex.Doors)
            {
                return DoorMat;
            }
            else if (assetIndex == DungeonAssetIndex.Floors)
            {
                return FloorMat;
            }
            else if (assetIndex == DungeonAssetIndex.Ceilings)
            {
                return CeilingMat;
            }
            else if (assetIndex == DungeonAssetIndex.Pillars)
            {
                return PillarMat;
            }
            else if (assetIndex == DungeonAssetIndex.Fences)
            {
                return WallMat;
            }
            return WallMat;
        }

        private void Clear()
        {
            for (int a = 0; a < DungeonAssetIndex.Max; a++)
            {
                GetMaterials(a).Clear();
            }


        }

        protected override void OnDestroy()
        {
            Clear();
            base.OnDestroy();
        }

    }
}
