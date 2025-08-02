using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Dungeons;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Buildings;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using System.Linq;

namespace Assets.Scripts.Crawler.Maps.GameObjects
{
    public class CrawlerMapRoot : BaseBehaviour
    {

        public string MapId { get; set; }

        private Dictionary<string, ClientMapCell> _worldCells { get; set; } = new Dictionary<string, ClientMapCell>();

        private Dictionary<string, List<ClientMapCell>> _mapCellCache { get; set; } = new Dictionary<string, List<ClientMapCell>>();

        private List<ClientMapCell> _allCells { get; set; } = new List<ClientMapCell>();
            
        public DungeonAssets DungeonAssets { get; set; }

        public DungeonMaterials DungeonMaterials { get; set; }

        public Material DoorMat { get; set; }

        public CityAssets CityAssets { get; set; }

        public ICrawlerMapTypeHelper MapTypeHelper { get; set; }

        public List<ClientMapCell> GetAllCells()
        {
            return _allCells;
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
