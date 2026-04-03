using System.Collections.Generic;

namespace Assets.Scripts.Dungeons
{
    public class DungeonAssetBlockList : BaseBehaviour
    {
        public List<WeightedDungeonAssetBlock> Blocks;

        public void Clear()
        {
            foreach (WeightedDungeonAssetBlock block in Blocks)
            {
                block.Clear();
            }
        }
    }
}
