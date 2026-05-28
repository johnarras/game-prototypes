using Assets.Scripts.Buildings;
using Assets.Scripts.Crawler.Maps.GameObjects;

namespace Assets.Scripts.Crawler.Maps.Loading
{
    public class CrawlerObjectLoadData
    {
        public ClientMapCell Cell { get; set; }
        public object Data { get; set; }
        public long Angle { get; set; }
        public CrawlerMapRoot MapRoot { get; set; }
        public long Seed { get; set; }
        public BuildingMats Mats { get; set; }
        public string PrefabName { get; set; }
        public string AssetCategoryNameOverride { get; set; }
    }

}


