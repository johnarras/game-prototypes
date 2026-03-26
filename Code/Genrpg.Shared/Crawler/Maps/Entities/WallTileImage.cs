using Genrpg.Shared.Crawler.Maps.Constants;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    public class WallTileImage
    {
        public int[] WallIds { get; set; } = new int[TileImageConstants.WallCount];
        public string Filename { get; set; } = "OOOO";

    }

}


