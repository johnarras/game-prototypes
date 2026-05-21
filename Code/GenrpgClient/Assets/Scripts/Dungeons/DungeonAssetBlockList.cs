using OxDb.SharedGame.Crawler.Maps.Constants;
using System.Collections.Generic;

namespace Assets.Scripts.Dungeons
{
    public class DungeonAssetBlockList : BaseBehaviour
    {
        public int BlockXZSize = CrawlerMapConstants.DefaultXZBlockSize;
        public int BlockYSize = CrawlerMapConstants.DefaultYBlockSize;

        public List<WeightedDungeonAssetBlock> Blocks = new List<WeightedDungeonAssetBlock>();

        public List<VaultedCeilingAssetBlock> VaultedCeilings = new List<VaultedCeilingAssetBlock>();

        public float VaultedCeilingChance;

        public void Clear()
        {
            foreach (WeightedDungeonAssetBlock block in Blocks)
            {
                block.Clear();
            }
        }
    }
}
