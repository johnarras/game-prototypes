namespace Genrpg.Shared.Crawler.Maps.Entities
{

    public class FullWallTileImage
    {

        public int Index { get; set; }
        public long RotAngle { get; set; } = 0;
        public string Filename { get; set; }
        public WallTileImage RefImage { get; set; }
    }

}


