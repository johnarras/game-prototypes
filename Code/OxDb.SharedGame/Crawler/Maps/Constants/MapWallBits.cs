namespace OxDb.SharedGame.Crawler.Maps.Constants
{
    public class MapWallBits
    {
        public const int EWallStart = 0;
        public const int NWallStart = EWallStart + WallBitSize;

        public const int WallBitSize = 3;

        public const int IsRoomBitOffset = NWallStart + WallBitSize;

        public const int WallBitMask = ((1 << WallBitSize) - 1);

        public const int NWallBitMask = WallBitMask << NWallStart;

        public const int EWallBitMask = WallBitMask << EWallStart;
    }
}


