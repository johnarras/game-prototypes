using OxDb.Client.Buildings;
using OxDb.Client.Crawler.Maps.Services.Helpers;
using OxDb.Client.Dungeons;
using OxDb.Client.MapTerrain;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.GameObjects
{

    public class MaterialBlock
    {
        public long ZoneTypeId { get; set; }
        public FinalDungeonMaterials FinalMaterials { get; set; } = new FinalDungeonMaterials();
        public Color ForegroundColor = Color.white;
        public Color BackgroundColor = Color.black;

        public bool IsReady()
        {
            if (FinalMaterials == null || !FinalMaterials.IsReady())
            {
                return false;
            }
            return true;
        }

        public void Clear()
        {
        }

        public Material GetRandomMaterial(int dungeonAssetIndex, long seed)
        {
            List<MaterialOption> options = FinalMaterials.GetMaterials(dungeonAssetIndex);
            if (options.Count > 0)
            {
                return options[(int)seed % options.Count].Mat;
            }
            return null;
        }
    }


    public class CrawlerMapRoot : BaseBehaviour, ITerrainContainer
    {

        public ClientMapCell CellPrefab = null;
        public GameObject TerrainParent = null;
        public GameObject AssetRoot = null;
        public GameObject CellAnchor = null;

        public string MapId { get; set; }

        public int XZBlockSize { get; set; }

        public int YBlockSize { get; set; }

        public CoreTerrainData Core { get; set; } = new CoreTerrainData();

        private bool[,] _wallsInNECorner = null;

        public bool DidDrawEdgeProps { get; set; }

        public bool HasWallInNECorner(int x, int z)
        {

            if (_wallsInNECorner == null)
            {
                SetUpCornerWallData(Map);
            }

            while (x < 0)
            {
                x += Map.Width;
            }
            x = x % Map.Width;

            while (z < 0)
            {
                z += Map.Height;
            }
            z = z % Map.Height;

            return _wallsInNECorner[x, z];
        }

        private void SetUpCornerWallData(CrawlerMap map)
        {
            _wallsInNECorner = new bool[map.Width, map.Height];

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    byte northWall = map.NorthWall(x, z);

                    if (map.NorthWall(x, z) != WallTypes.None)
                    {
                        _wallsInNECorner[x, z] = true;
                        _wallsInNECorner[(x + map.Width - 1) % map.Width, z] = true;
                    }

                    if (map.EastWall(x, z) != WallTypes.None)
                    {
                        _wallsInNECorner[x, z] = true;
                        _wallsInNECorner[x, (z + map.Height - 1) % map.Height] = true;
                    }
                }
            }
        }


        private Dictionary<string, ClientMapCell> _worldCells { get; set; } = new Dictionary<string, ClientMapCell>();

        private Dictionary<string, List<ClientMapCell>> _mapCellCache { get; set; } = new Dictionary<string, List<ClientMapCell>>();

        private Dictionary<string, List<ClientMapCell>> _mapCellList { get; set; } = new Dictionary<string, List<ClientMapCell>>();

        private List<ClientMapCell> _allCells { get; set; } = new List<ClientMapCell>();

        public DungeonAssetBlockList AssetBlockList { get; set; }

        public WeightedDungeonAssetBlock AssetBlock { get; set; }

        public VaultedCeilingAssetBlock VaultedCeilingAssets { get; set; }

        public DungeonAsset PillarAsset { get; set; }

        public Dictionary<long, MaterialBlock> MaterialBlocks { get; set; } = new Dictionary<long, MaterialBlock>();

        public CityAssets CityAssets { get; set; }

        public List<MaterialOption> BuildingWallOptions { get; set; } = new List<MaterialOption>();

        public List<Texture2D> GeneratedTextures { get; set; } = new List<Texture2D>();

        public bool AssetsAreReady()
        {
            return AssetBlockList != null && AssetBlock != null && MaterialBlocks.Count > 0 &&
                !MaterialBlocks.Values.Any(x => !x.IsReady()) &&
                ((CityAssets != null && CityAssets.IsReady()) ||
                Map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon);
        }

        public ICrawlerMapTypeHelper MapTypeHelper { get; set; }

        private long[,] _extendedTerrain = null;

        private List<long> _dungeonZoneTypes = null;

        private List<long> _allZoneTypes = null;

        public List<ClientMapCell> GetAllCells()
        {
            return _allCells;
        }

        public MaterialBlock GetMaterialBlockAt(int x, int z)
        {
            long zoneTypeId = GetZoneTypeAt(x, z);

            if (MaterialBlocks.TryGetValue(zoneTypeId, out MaterialBlock assetBlock))
            {
                return assetBlock;
            }
            if (MaterialBlocks.Count > 0)
            {
                return MaterialBlocks.Values.First();
            }
            return null;
        }

        public List<long> GetAllZoneTypes()
        {
            SetupExtendedTerrain();
            return _allZoneTypes;
        }

        public List<long> GetAllTerrainZoneTypes()
        {
            SetupExtendedTerrain();
            return _dungeonZoneTypes;
        }

        public long GetZoneTypeAt(int x, int z)
        {
            SetupExtendedTerrain();
            return _extendedTerrain[x, z];
        }

        private void SetupExtendedTerrain()
        {
            if (_extendedTerrain == null)
            {
                _extendedTerrain = new long[Map.Width, Map.Height];
                _dungeonZoneTypes = new List<long>();
                _allZoneTypes = new List<long>();
                for (int x = 0; x < Map.Width; x++)
                {
                    for (int z = 0; z < Map.Height; z++)
                    {
                        long zoneTypeId = Map.Get(x, z, CellIndex.Terrain);
                        if (zoneTypeId == 0 && x > 0)
                        {
                            zoneTypeId = Map.Get(x - 1, z, CellIndex.Terrain);
                        }
                        if (zoneTypeId == 0 && z > 0)
                        {
                            zoneTypeId = Map.Get(x, z - 1, CellIndex.Terrain);
                        }
                        if (zoneTypeId == 0 && x < Map.Width - 1)
                        {
                            zoneTypeId = Map.Get(x + 1, z, CellIndex.Terrain);
                        }
                        if (zoneTypeId == 0 && z < Map.Height - 1)
                        {
                            zoneTypeId = Map.Get(x, z + 1, CellIndex.Terrain);
                        }
                        _extendedTerrain[x, z] = zoneTypeId;
                        if (zoneTypeId > 0 && !_dungeonZoneTypes.Contains(zoneTypeId))
                        {
                            _dungeonZoneTypes.Add(zoneTypeId);
                            _allZoneTypes.Add(zoneTypeId);
                        }
                    }

                    _dungeonZoneTypes = _dungeonZoneTypes.Distinct().ToList();
                }

                if (Map.ZoneTypeId > 0)
                {

                    if (!_allZoneTypes.Contains(Map.ZoneTypeId))
                    {
                        _allZoneTypes.Add(Map.ZoneTypeId);
                    }

                    if (Map.CrawlerMapTypeId != CrawlerMapTypes.Dungeon)
                    {
                        _dungeonZoneTypes = new List<long>() { Map.ZoneTypeId };

                    }
                }
            }
        }

        public FinalDungeonMaterials GetMaterialsAt(int x, int z)
        {
            return GetMaterialBlockAt(x, z)?.FinalMaterials ?? null;
        }
        public List<ClientMapCell> GetCellsAtMapPos(int mapX, int mapZ)
        {

            string mapKey = mapX + "." + mapZ;

            if (_mapCellList.TryGetValue(mapKey, out List<ClientMapCell> mapCells))
            {
                return mapCells;
            }
            return new List<ClientMapCell>();
        }

        public ClientMapCell GetCellAtWorldPos(int worldX, int worldZ, bool createIfNotExist, bool clampToMapCoordinates)
        {

            string worldKey = worldX + "." + worldZ;

            if (_worldCells.TryGetValue(worldKey, out ClientMapCell cell))
            {
                return cell;
            }

            if (!createIfNotExist)
            {
                return null;
            }

            int mapX = (worldX + Map.Width) % Map.Width;
            int mapZ = (worldZ + Map.Height) % Map.Height;

            if (!clampToMapCoordinates)
            {
                mapX = worldX;
                mapZ = worldZ;
            }

            string mapKey = mapX + "." + mapZ;

            if (_mapCellCache.TryGetValue(mapKey, out List<ClientMapCell> mapCells))
            {
                if (mapCells.Count > 0)
                {
                    cell = mapCells[0];
                    mapCells.RemoveAt(0);
                    InitCellPos(cell, mapX, mapZ, worldX, worldZ);
                    return cell;
                }
            }

            cell = _clientEntityService.FullInstantiate(CellPrefab);
            cell.name = "ClientMapCell" + mapX + "." + mapZ;
            _clientEntityService.AddToParent(cell, CellAnchor);
            if (!_mapCellList.ContainsKey(mapKey))
            {
                _mapCellList[mapKey] = new List<ClientMapCell>();
            }
            _mapCellList[mapKey].Add(cell);
            InitCellPos(cell, mapX, mapZ, worldX, worldZ);

            return cell;
        }
        private void InitCellPos(ClientMapCell cell, int mapX, int mapZ, int worldX, int worldZ)
        {
            cell.MapX = mapX;
            cell.MapZ = mapZ;
            cell.WorldX = worldX;
            cell.WorldZ = worldZ;
            string worldKey = worldX + "." + worldZ;
            string mapKey = mapX + "." + mapZ;
            _worldCells[worldKey] = cell;
            _allCells.Add(cell);
            _clientEntityService.SetActive(cell.Content, true);
        }

        public void ReturnCell(ClientMapCell cell)
        {
            if (cell.KeepActive)
            {
                return;
            }

            string mapKey = cell.MapX + "." + cell.MapZ;
            string worldKey = cell.WorldX + "." + cell.WorldZ;

            if (_worldCells.ContainsKey(worldKey))
            {
                _worldCells.Remove(worldKey);
            }

            if (!_mapCellCache.ContainsKey(mapKey))
            {
                _mapCellCache[mapKey] = new List<ClientMapCell>();

            }
            if (!_mapCellCache[mapKey].Contains(cell))
            {
                _mapCellCache[mapKey].Add(cell);
                cell.DidJustDraw = false;
                _clientEntityService.SetActive(cell.Content, false);
            }
            _allCells.Remove(cell);

        }

        public CrawlerMap Map { get; set; }

        public float DrawX { get; set; }
        public float DrawZ { get; set; }
        public float DrawY { get; set; }
        public float DrawRot { get; set; }

        public void SetupFromMap(CrawlerMap map)
        {
            Map = map;

            int dataSize = map.Width * map.Height;

            foreach (MapCellDetail detail in map.Details)
            {
                GetCellAtWorldPos(detail.X, detail.Z, true, true).Details.Add(detail);
            }
        }

        public void Clear()
        {
            foreach (ClientMapCell worldCell in _worldCells.Values)
            {
                worldCell.ClearFullCell();
            }

            foreach (List<ClientMapCell> mapCellList in _mapCellCache.Values)
            {
                foreach (ClientMapCell mapCell in mapCellList)
                {
                    mapCell.ClearFullCell();
                }
            }

            foreach (ClientMapCell mapCell in _allCells)
            {
                mapCell.ClearFullCell();
            }

            _worldCells.Clear();
            _mapCellCache.Clear();
            _allCells.Clear();

            foreach (Texture2D tex in GeneratedTextures)
            {
                _clientEntityService.Destroy(tex);
            }
            GeneratedTextures.Clear();

            foreach (MaterialBlock block in MaterialBlocks.Values)
            {
                block.Clear();
            }

            MaterialBlocks.Clear();

            foreach (MaterialOption opt in BuildingWallOptions)
            {
                opt.Clear();
            }
            BuildingWallOptions.Clear();

            _clientEntityService.Destroy(TerrainParent);

            PillarAsset = null;

            base.OnDestroy();
        }

        protected override void OnDestroy()
        {
            Clear();
            base.OnDestroy();
        }


        public void SetCellPropVisibility(int x, int z, float alpha)
        {
            if (Map.CrawlerMapTypeId != CrawlerMapTypes.Outdoors)
            {
                return;
            }

            List<ClientMapCell> cells = GetCellsAtMapPos(x, z);

            foreach (ClientMapCell cell in cells)
            {
                cell.SetPropAlphas(alpha);
            }
        }
    }
}


