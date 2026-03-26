namespace Genrpg.Shared.Crawler.Maps.Constants
{
    public class MapWallBits
    {
        public const int EWallStart = 0; // 0
        public const int NWallStart = EWallStart + WallBitSize; // 90

        public const int WallBitSize = 3;

        public const int IsRoomBitOffset = NWallStart + WallBitSize;
    }

}


