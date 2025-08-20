using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers;
using Assets.Scripts.Dungeons;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{
    public class WallsDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override int Order => 100;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token)
        {
            int xzBlockSize = CrawlerMapConstants.XZBlockSize;
            int yBlockSize = CrawlerMapConstants.YBlockSize;
            GameObject go = (GameObject)cell.Content;

            AssetBlock assetBlock = mapRoot.GetAssetBlockAt(cell.MapX, cell.MapZ);

            if (assetBlock == null)
            {
                return;
            }

            bool isRoom = (mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;

            int dnx = (cell.MapX + 1) % mapRoot.Map.Width;
            int dnz = (cell.MapZ + 1) % mapRoot.Map.Height;

            bool eIsRoom = (mapRoot.Map.Get(dnx, cell.MapZ, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;
            bool nIsRoom = (mapRoot.Map.Get(cell.MapX, dnz, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;


            if (mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Terrain) != 0)
            {

                if (mapRoot.Map.HasFlag(CrawlerMapFlags.IsIndoors))
                {
                    AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.Ceiling, DungeonAssetIndex.Ceilings, go, new Vector3(0, yBlockSize * (isRoom ? 2 : 1), 0), new Vector3(90, 0, 0), realCellX, realCellZ);
                }

                if (mapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon)
                {
                    AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.Floor, DungeonAssetIndex.Floors, go, new Vector3(0, 0, 0), new Vector3(90, 0, 0), realCellX, realCellZ);
                }
            }

            Vector3 nOffset = new Vector3(0, xzBlockSize / 2, xzBlockSize / 2);
            Vector3 nRot = new Vector3(0, 0, 0);

            int northBits = mapRoot.Map.NorthWall(cell.MapX, cell.MapZ);

            bool havePillar = false;
            bool IsTallBorder = false;

            if (northBits == WallTypes.Wall || northBits == WallTypes.Secret)
            {
                AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.NorthWall, DungeonAssetIndex.Walls, go, nOffset, nRot, realCellX, realCellZ);
                havePillar = true;
            }
            else if (northBits == WallTypes.Door)
            {
                AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.NorthWall, DungeonAssetIndex.Doors, go, nOffset, nRot, realCellX, realCellZ);
                havePillar = true;
            }
            else if (northBits == WallTypes.Barricade)
            {
                AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.NorthWall, DungeonAssetIndex.Fences, go, nOffset, nRot, realCellX, realCellZ);
            }
            if (isRoom != nIsRoom && mapRoot.Map.HasFlag(CrawlerMapFlags.IsIndoors))
            {
                AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.NorthUpper, DungeonAssetIndex.Walls, go, nOffset + new Vector3(0, yBlockSize, 0), nRot, realCellX, realCellZ);
                IsTallBorder = true;
            }

            Vector3 eOffset = new Vector3(xzBlockSize / 2, yBlockSize / 2, 0);
            Vector3 eRot = new Vector3(0, 90, 0);

            int eastBits = mapRoot.Map.EastWall(cell.MapX, cell.MapZ);

            if (eastBits == WallTypes.Wall || eastBits == WallTypes.Secret)
            {
                AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.EastWall, DungeonAssetIndex.Walls, go, eOffset, eRot, realCellX, realCellZ);
                havePillar = true;
            }
            else if (eastBits == WallTypes.Door)
            {
                AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.EastWall, DungeonAssetIndex.Doors, go, eOffset, eRot, realCellX, realCellZ);
                havePillar = true;
            }
            else if (eastBits == WallTypes.Barricade)
            {
                AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.EastWall, DungeonAssetIndex.Fences, go, eOffset, eRot, realCellX, realCellZ);
            }

            if (isRoom != eIsRoom && mapRoot.Map.HasFlag(CrawlerMapFlags.IsIndoors))
            {
                AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.EastUpper, DungeonAssetIndex.Walls, go, eOffset + new Vector3(0, yBlockSize, 0), eRot, realCellX, realCellZ);
                IsTallBorder = true;
            }


            // Check next wall up or over.
            if (!havePillar)
            {
                if (realCellX == 0 || realCellZ == 0 ||
                    realCellX == mapRoot.Map.Width - 1 ||
                    realCellZ == mapRoot.Map.Height - 1)
                {
                    if (!mapRoot.Map.HasFlag(CrawlerMapFlags.IsLooping))
                    {
                        havePillar = true;
                    }
                }

                int eastWall = mapRoot.Map.EastWall(realCellX, (realCellZ + 1) % mapRoot.Map.Height);
                if (eastWall == WallTypes.Wall || eastWall == WallTypes.Door || eastWall == WallTypes.Secret)
                {
                    havePillar = true;
                }
                else
                {
                    int northWall = mapRoot.Map.NorthWall((realCellX + 1) % mapRoot.Map.Width, realCellZ);
                    if (northWall == WallTypes.Wall || northWall == WallTypes.Door || northWall == WallTypes.Secret)
                    {
                        havePillar = true;
                    }
                }
            }

            if (havePillar && mapRoot.Map.CrawlerMapTypeId != CrawlerMapTypes.Outdoors)
            {
                AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.Pillar, DungeonAssetIndex.Pillars, go, new Vector3(xzBlockSize / 2, 0, xzBlockSize / 2), Vector3.zero, -1, -1);
                if (IsTallBorder)
                {
                    AddWallComponent(mapRoot, cell, assetBlock, DungeonAssetPosition.Pillar, DungeonAssetIndex.Pillars, go, new Vector3(xzBlockSize / 2, yBlockSize, xzBlockSize / 2), Vector3.zero, -1, -1);
                }
            }
            await Task.CompletedTask;
        }

        protected void AddWallComponent(CrawlerMapRoot mapRoot, ClientMapCell cell, AssetBlock block, int assetPositionIndex, int dungeonAssetIndex, GameObject parent, Vector3 offset, Vector3 euler, int realCellX, int realCellZ)
        {
            List<WeightedDungeonAsset> assetList = block.DungeonAssets.GetAssetList(dungeonAssetIndex);

            bool isDoor = dungeonAssetIndex == DungeonAssetIndex.Doors;

            if (isDoor)
            {
                dungeonAssetIndex = DungeonAssetIndex.Walls;
            }

            DungeonAsset asset = assetList[0].Asset;

            long assetWeightSum = assetList.Sum(x => x.Weight);

            if (assetWeightSum > 0)
            {
                if (realCellZ < 0 && realCellZ < 0)
                {
                    asset = assetList[(int)(mapRoot.Map.ArtSeed % assetList.Count)].Asset;
                }

                long weightHash = realCellX * 7079 + realCellZ * 2383 + (int)offset.x * 3361 + (int)offset.y * 709 + (int)offset.z * 4327;

                long chosenWeight = weightHash % assetWeightSum;

                foreach (WeightedDungeonAsset wgo in assetList)
                {
                    chosenWeight -= wgo.Weight;

                    if (chosenWeight <= 0)
                    {
                        asset = wgo.Asset;
                        break;
                    }
                }
            }

            DungeonAsset dungeonAsset = _clientEntityService.FullInstantiate(asset);
            cell.AssetPositions[assetPositionIndex] = dungeonAsset;
            _clientEntityService.AddToParent(dungeonAsset, parent);
            dungeonAsset.transform.localPosition = offset;
            dungeonAsset.transform.eulerAngles = euler;


            List<WeightedMaterial> materialList = block.DungeonMaterials.GetMaterials(dungeonAssetIndex);

            Material finalMat = materialList.Count > 0 ? materialList[0].Mat : null;

            long matWeightSum = materialList.Sum(x => x.Weight);

            if (matWeightSum > 0)
            {
                long weightHash = realCellX * 1951 + realCellZ * 443 + (int)offset.x * 197 + (int)offset.y * 2843 + (int)offset.z * 653;

                long chosenWeight = weightHash % matWeightSum;

                foreach (WeightedMaterial weightedMat in materialList)
                {
                    chosenWeight -= weightedMat.Weight;

                    if (chosenWeight <= 0)
                    {
                        finalMat = weightedMat.Mat;
                        break;
                    }
                }
            }

            if (finalMat != null)
            {
                foreach (Renderer rend in dungeonAsset.Renderers)
                {
                    rend.sharedMaterial = finalMat;
                }

                if (isDoor)
                {
                    foreach (Renderer rend in dungeonAsset.DoorRenderers)
                    {
                        rend.sharedMaterial = block.DoorMat;
                    }
                }
            }
            else
            {
                _clientEntityService.SetActive(dungeonAsset, false);
            }
        }

    }
}
