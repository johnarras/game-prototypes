using OxDb.SharedGame.Crawler.Maps.Constants;

namespace OxDb.SharedGame.Crawler.Maps.Entities
{
    public class WallTileImage
    {
        public int[] WallIds { get; set; } = new int[TileImageConstants.WallCount];
        public string Filename { get; set; } = "OOOO";

    }

}


