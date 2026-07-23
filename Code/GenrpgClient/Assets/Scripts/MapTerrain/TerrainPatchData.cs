
using OxDb.SharedGame.MapServer.Entities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // Needed
namespace OxDb.Client.MapTerrain
{

    public class CoreTerrainData
    {

        public Terrain Terrain { get; set; }

        public TerrainData TerrainData { get; set; }

        public GameObject AssetRoot { get; set; }

        public List<IndexedTerrainLayer> Layers { get; set; } = new List<IndexedTerrainLayer>();

        public int TerrainSize { get; set; }

        public float WorldUnitsPerCell { get; set; } = 1;

        public int GX { get; set; }
        public int GZ { get; set; }


        public bool IsValid()
        {
            return Terrain != null && TerrainData != null;
        }

        public bool IsReady()
        {
            return Layers.Count > 0 && !Layers.Any(x => !x.IsReady());
        }

        public void AddNewTextureIndex(long textureTypeId, long zoneTypeId)
        {
            if (textureTypeId < 1 || Layers.Any(x => x.TextureTypeId == textureTypeId))
            {
                return;
            }

            IndexedTerrainLayer indexData = new IndexedTerrainLayer()
            {
                TextureTypeId = textureTypeId,
                Index = Layers.Count,
                ZoneTypeId = zoneTypeId,
                Core = this
            };

            Layers.Add(indexData);

        }


        public void SetLayers()
        {
            if (!IsValid() || !IsReady())
            {
                return;
            }

            TerrainLayer[] layers = Layers.OrderBy(x => x.Index).Select(x => x.TerrainLayer).ToArray();

            TerrainData.terrainLayers = layers;
        }

    }

    public interface ITerrainContainer
    {
        CoreTerrainData Core { get; set; }
    }

    public class IndexedTerrainLayer
    {
        public int Index { get; set; }
        public long ZoneTypeId { get; set; }
        public long TextureTypeId { get; set; }

        public TerrainLayer TerrainLayer { get; set; }

        public CoreTerrainData Core { get; set; }

        public bool IsReady() { return TerrainLayer != null; }

    }

    public class TerrainPatchData : ITerrainContainer
    {
        public string Id { get; set; }
        public string MapId { get; set; }

        public int MapVersion { get; set; }

        public CoreTerrainData Core { get; set; } = new CoreTerrainData();
        // X grid in map

        public List<long> FullZoneIdList { get; set; } = new List<long>();

        public List<long> MainZoneIdList { get; set; } = new List<long>();

        public byte[] DataBytes;

        public float[,] heights { get; set; }

        public float[,,] baseAlphas { get; set; }

        public byte[,] entityTypeIds { get; set; }

        public byte[,] entityIds { get; set; }

        public ushort[,,] grassAmounts { get; set; }

        public byte[,] subZoneIds { get; set; } = new byte[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        public byte[,] mainZoneIds { get; set; } = new byte[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        public byte[,] overrideZoneScales { get; set; } = new byte[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        public bool HaveSetAlphamaps = false;

        public List<ExtendedWorldObjectData> ExtendedObjects { get; set; } = new List<ExtendedWorldObjectData>();

        public ExtendedWorldObjectData GetObjAtPos(PatchLoadData loadData, int x, int z)
        {
            return ExtendedObjects.FirstOrDefault(
                e => e.Z - loadData.StartX == x && e.X - loadData.StartZ == z);
        }

        public string GetFilePath()
        {
            string path = MapUtils.GetMapFolder(MapId, MapVersion) + "TerrainX" + Core.GX.ToString("000") + "Z" + Core.GZ.ToString("000");
            return path;
        }

    }
}


