using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Constants
{
    public class BoardGameConstants
    {
        public const int FirstTileIndex = 0;
        public const int MinPlayMult = 1;
        public const int StartPathIndex = 1;

        public const int GoldTilesBetweenEachSpecialTile = 3;


        public const string NewBoardModeTempFilename = "NewBoard";

        public const int MapWidth = 20;
        public const int MapHeight = 20;

        public const int BaseRewardMult = 1;
    }
}
