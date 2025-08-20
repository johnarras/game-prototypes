using Assets.Scripts.Buildings;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Assets.Scripts.Dungeons;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.GameObjects
{

    public class AssetBlock
    {
        public long ZoneTypeId { get; set; }
        public DungeonAssets DungeonAssets { get; set; }
        public DungeonMaterials DungeonMaterials { get; set; }
        public Material DoorMat { get; set; }

        public bool IsReady()
        {
            return DungeonAssets != null && DungeonMaterials != null && DoorMat != null;
        }
    }

    public class CrawlerMapRoot : BaseBehaviour
    {
        public string MapId { get; set; }

        private Dictionary<string, ClientMapCell> _worldCells { get; set; } = new Dictionary<string, ClientMapCell>();

        private Dictionary<string, List<ClientMapCell>> _mapCellCache { get; set; } = new Dictionary<string, List<ClientMapCell>>();

        private List<ClientMapCell> _allCells { get; set; } = new List<ClientMapCell>();

        public Dictionary<long, AssetBlock> AssetBlocks { get; set; } = new Dictionary<long, AssetBlock>();

        public CityAssets CityAssets { get; set; }

        public ICrawlerMapTypeHelper MapTypeHelper { get; set; }

        private long[,] _extendedTerrain = null;

        private List<long> _zoneTypes = null;

        public List<ClientMapCell> GetAllCells()
        {
            return _allCells;
        }

        public AssetBlock GetAssetBlockAt(int x, int z)
        {
            long zoneTypeId = GetZoneTypeAt(x, z);

            if (AssetBlocks.TryGetValue(zoneTypeId, out AssetBlock assetBlock))
            {
                return assetBlock;
            }
            if (AssetBlocks.Count > 0)
            {
                return AssetBlocks.Values.First();
            }
            return null;
        }

        public List<long> GetAllZoneTypes()
        {
            SetupExtendedTerrain();
            return _zoneTypes;
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
                _zoneTypes = new List<long>();
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
                        if (zoneTypeId > 0 && !_zoneTypes.Contains(zoneTypeId))
                        {
                            _zoneTypes.Add(zoneTypeId);
                        }
                    }
                }

                if (Map.CrawlerMapTypeId != CrawlerMapTypes.Dungeon)
                {
                    _zoneTypes = new List<long>() { Map.ZoneTypeId };
                }
            }
        }

        public DungeonMaterials GetMaterialsAt(int x, int z)
        {
            return GetAssetBlockAt(x, z)?.DungeonMaterials ?? null;
        }

        public DungeonAssets GetAssetsAt(int x, int z)
        {
            return GetAssetBlockAt(x, z)?.DungeonAssets ?? null;
        }

        public Material GetDoorMatAt(int x, int z)
        {
            return GetAssetBlockAt(x, z)?.DoorMat ?? null;
        }

        public ClientMapCell GetCellAtWorldPos(int worldX, int worldZ, bool createIfNotExist)
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

            GameObject go = new GameObject() { name = "MapCell-" + mapKey };
            _clientEntityService.AddToParent(go, gameObject);
            cell = go.AddComponent<ClientMapCell>();
            cell.Content = go;
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
                GetCellAtWorldPos(detail.X, detail.Z, true).Details.Add(detail);
            }
        }
    }
}
