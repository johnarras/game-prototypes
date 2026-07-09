namespace Assets.Scripts.Crawler.MapGen.RoomGen.Entities
{
    public class EdgeRowArgs
    {
        public int SX { get; set; }
        public int SZ { get; set; }
        public int DX { get; set; }
        public int DZ { get; set; }
        public int Length { get; set; }
        public bool RoomAtEnd { get; set; }
    }
}
