using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OxDb.SharedGame.Crawler.Maps.Constants
{

    public class MapDir
    {
        public int Index => (int)Dir;
        public EMapDirs Dir { get; set; }
        public EMapDirs OppDir { get; set; }
        public int DX { get; set; }
        public int DZ { get; set; }
    }

    public enum EMapDirs
    {
        North=0,
        East=1,
        South=2,
        West=3,
    }

    public static class MapDirUtils
    {
        private static readonly Dictionary<EMapDirs,MapDir> _dirs = new Dictionary<EMapDirs, MapDir>()
        {
            [EMapDirs.North] = new MapDir() { Dir = EMapDirs.North, OppDir = EMapDirs.South, DX = 0, DZ = 1 },
            [EMapDirs.East] = new MapDir() { Dir = EMapDirs.East, OppDir = EMapDirs.West, DX = 1, DZ = 0 },
            [EMapDirs.South] = new MapDir() { Dir = EMapDirs.South, OppDir = EMapDirs.North, DX = 0, DZ = -1 },
            [EMapDirs.West] = new MapDir() { Dir = EMapDirs.West, OppDir = EMapDirs.East, DX = -1, DZ = 0 },
        };

        public static Dictionary<EMapDirs,MapDir> GetDirs()
        {
            return _dirs;
        }

        public static MapDir GetDir(EMapDirs dir)
        {
            return _dirs[dir];
        }

        public static MapDir GetDirFromDeltas(int dx, int dz)
        {
            return _dirs.Values.FirstOrDefault(x=>x.DX == dx && x.DZ == dz);
        }
    }
}


