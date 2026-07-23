using OxDb.Client.MapTerrain;
using UnityEngine;

namespace OxDb.Client.Trader.WorldMap.Entities
{

    public class TraderTerrainPatch : BaseBehaviour, ITerrainContainer
    {
        public int XPos { get; set; }
        public int ZPos { get; set; }

        public CoreTerrainData Core { get; set; } = new CoreTerrainData();

        public float[,] Heights { get; set; }

        public float[,,] Alphas { get; set; }

        public GameObject TerrainParent;
    }
}
