using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Maps.Constants
{
    public class CrawlerMapFlags
    {
        public const int IsLooping = (1 << 0);
        public const int ShowFullRiddleText = (1 << 1);
        public const int IsIndoors = (1 << 2);
        public const int NextLevelIsDown = (1 << 3);
    }
}
