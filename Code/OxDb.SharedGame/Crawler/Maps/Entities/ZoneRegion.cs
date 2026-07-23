namespace OxDb.SharedGame.Crawler.Maps.Entities
{
    public class ZoneRegion
    {
        public long ZoneTypeId { get; set; }
        public string Name { get; set; }
        public int CenterX { get; set; }
        public int CenterZ { get; set; }
        public int Level { get; set; }
        public int RegionId { get; set; }
        public bool IsWaterRegion { get; set; }
    }
}


