using OxDb.Client.Buildings;
using OxDb.Client.Crawler.Maps.GameObjects;

namespace OxDb.Client.Crawler.Maps.Loading
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
        public float XOffset { get; set; }
        public float ZOffset { get; set; }
        public float Scale { get; set; } = 1.0f;
    }

}


