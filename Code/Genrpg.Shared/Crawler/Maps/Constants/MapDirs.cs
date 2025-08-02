using MessagePack;
using Genrpg.Shared.Characters.PlayerData;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace Genrpg.Shared.Crawler.Maps.Constants
{

    [MessagePackObject]
    public class MapDir
    {
        [Key(0)] public int Dir { get; set; }
        [Key(1)] public int OppDir { get; set; }
        [Key(2)] public int DX { get; set; }
        [Key(3)] public int DZ { get; set; }
    }

    public static class MapDirs
    {
        public const int North = 0;
        public const int East = 1;
        public const int South = 2;
        public const int West = 3;


        private static readonly MapDir[] _dirs = new MapDir[]
        {
            new MapDir() { Dir = North, OppDir = South, DX = 0, DZ = 1 },
            new MapDir() { Dir = East, OppDir = West, DX = 1, DZ = 0 },
            new MapDir() { Dir = South, OppDir = North, DX = 0, DZ = -1 },
            new MapDir() { Dir = West, OppDir = East, DX = -1, DZ = 0 },
        };

        public static MapDir[] GetDirs()
        {
            return _dirs;
        }
    }
}
