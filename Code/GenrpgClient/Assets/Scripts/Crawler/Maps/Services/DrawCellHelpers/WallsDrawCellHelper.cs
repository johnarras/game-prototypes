using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Services.DrawEntityHelpers;
using OxDb.Client.Dungeons;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Zones.Settings;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.Services.DrawCellHelpers
{
    public class WallsDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override ECrawlerDrawCellOrder HelperKey => ECrawlerDrawCellOrder.Walls;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, CancellationToken token)
        {

          
            if (mapRoot.Map.IsOutdoorDungeon())
            {
                return;
            }

            int xzBlockSize = mapRoot.XZBlockSize;
            int yBlockSize = mapRoot.YBlockSize;
            GameObject go = (GameObject)cell.Content;

            MaterialBlock materialBlock = mapRoot.GetMaterialBlockAt(cell.MapX, cell.MapZ);

            if (materialBlock == null)
            {
                return;
            }

            bool IsTallCell = (mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;

            int dnx = (cell.MapX + 1) % mapRoot.Map.Width;
            int dnz = (cell.MapZ + 1) % mapRoot.Map.Height;

            bool eIsRoom = (mapRoot.Map.Get(dnx, cell.MapZ, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;
            bool nIsRoom = (mapRoot.Map.Get(cell.MapX, dnz, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;

            if (mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Terrain) != 0)
            {
                if (mapRoot.Map.HasFlag(CrawlerMapFlags.IsIndoorDungeon))
                {
                    float heightMult = (IsTallCell && mapRoot.VaultedCeilingAssets == null ? 2 : 1);
                    AddWallComponent(mapRoot, cell, materialBlock, DungeonAssetPosition.Ceiling, DungeonPrefabIndexes.Ceilings, go, new Vector3(0, yBlockSize * heightMult, 0), new Vector3(90, 0, 0));
                }
            }

            Vector3 nOffset = new Vector3(0, yBlockSize / 2, xzBlockSize / 2);
            Vector3 nRot = new Vector3(0, 0, 0);
            Vector3 eOffset = new Vector3(xzBlockSize / 2, yBlockSize / 2, 0);
            Vector3 eRot = new Vector3(0, 90, 0);

            int northBits = mapRoot.Map.NorthWall(cell.MapX, cell.MapZ);

            bool havePillar = false;
            bool IsTallBorder = false;

            float pillarAngle = 0;
            if (WallTypes.UsesTilemapWallArt(northBits))
            {
                AddWallComponent(mapRoot, cell, materialBlock, DungeonAssetPosition.NorthWall, DungeonPrefabIndexes.Walls, go, nOffset, nRot);
                havePillar = true;
                pillarAngle = 0;
            }
            else if (northBits == WallTypes.Door)
            {
                AddWallComponent(mapRoot, cell, materialBlock, DungeonAssetPosition.NorthWall, DungeonPrefabIndexes.Door, go, nOffset, nRot);
                havePillar = true;
                pillarAngle = 0;
            }

            else if (northBits == WallTypes.Barricade)
            {
                AddWallComponent(mapRoot, cell, materialBlock, DungeonAssetPosition.NorthWall, DungeonPrefabIndexes.Fences, go, nOffset, nRot);
            }
            if (mapRoot.VaultedCeilingAssets == null && IsTallCell != nIsRoom && mapRoot.Map.HasFlag(CrawlerMapFlags.IsIndoorDungeon))
            {
                AddWallComponent(mapRoot, cell, materialBlock, DungeonAssetPosition.NorthUpper, DungeonPrefabIndexes.Walls, go, nOffset + new Vector3(0, yBlockSize, 0), nRot);
                IsTallBorder = true;
            }


            int eastBits = mapRoot.Map.EastWall(cell.MapX, cell.MapZ);

            if (WallTypes.UsesTilemapWallArt(eastBits))
            {
                AddWallComponent(mapRoot, cell, materialBlock, DungeonAssetPosition.EastWall, DungeonPrefabIndexes.Walls, go, eOffset, eRot);
                havePillar = true;
                pillarAngle = 90;
            }
            else if (eastBits == WallTypes.Door)
            {
                AddWallComponent(mapRoot, cell, materialBlock, DungeonAssetPosition.EastWall, DungeonPrefabIndexes.Door, go, eOffset, eRot);
                havePillar = true;
                pillarAngle = 90;
            }
            else if (eastBits == WallTypes.Barricade)
            {
                AddWallComponent(mapRoot, cell, materialBlock, DungeonAssetPosition.EastWall, DungeonPrefabIndexes.Fences, go, eOffset, eRot);
            }

            if (mapRoot.VaultedCeilingAssets == null && IsTallCell != eIsRoom && mapRoot.Map.HasFlag(CrawlerMapFlags.IsIndoorDungeon))
            {
                AddWallComponent(mapRoot, cell, materialBlock, DungeonAssetPosition.EastUpper, DungeonPrefabIndexes.Walls, go, eOffset + new Vector3(0, yBlockSize, 0), eRot);
                IsTallBorder = true;
            }


            // Check next wall up or over.
            if (!havePillar)
            {
                if (cell.MapX == 0 || cell.MapZ == 0)
                {

                    havePillar = true;
                    pillarAngle = 180;
                }

                if (cell.MapX == mapRoot.Map.Width - 1 ||
                    cell.MapZ == mapRoot.Map.Height - 1)
                {
                    havePillar = true;
                    pillarAngle = 90;
                }

                int eastWall = mapRoot.Map.EastWall(cell.MapX, (cell.MapZ + 1) % mapRoot.Map.Height);
                if (WallTypes.HasPillar(eastWall))
                {
                    havePillar = true;
                    pillarAngle = -90;
                }
                else
                {
                    int northWall = mapRoot.Map.NorthWall((cell.MapX + 1) % mapRoot.Map.Width, cell.MapZ);
                    if (WallTypes.HasPillar(northWall))
                    {
                        havePillar = true;
                        pillarAngle = 90;
                    }
                }
            }

            if (havePillar && mapRoot.Map.CrawlerMapTypeId != CrawlerMapTypes.Outdoors)
            {
                Vector2 pillarRot = new Vector3(0, pillarAngle, 0);
                AddWallComponent(mapRoot, cell, materialBlock, DungeonAssetPosition.Pillar, DungeonPrefabIndexes.Pillars, go, new Vector3(xzBlockSize / 2, 0, xzBlockSize / 2), pillarRot);
                if (IsTallBorder)
                {
                    AddWallComponent(mapRoot, cell, materialBlock, DungeonAssetPosition.Pillar, DungeonPrefabIndexes.Pillars, go, new Vector3(xzBlockSize / 2, yBlockSize, xzBlockSize / 2), pillarRot);
                }
            }
            await Task.CompletedTask;
        }

        protected DungeonAsset GetFinalDoorAsset(IRandom rand, CrawlerMapRoot mapRoot, ClientMapCell cell, MaterialBlock block)
        {
            DungeonDoorAssetBlock doorAssetBlock = RandUtils.GetRandomElement(mapRoot.AssetBlock.Doors, rand);

            if (doorAssetBlock == null)
            {
                return null;
            }

            WeightedDungeonAsset doorFrame = RandUtils.GetRandomElement(doorAssetBlock.DoorFrames, rand);

            if (doorFrame == null || doorFrame.Asset == null)
            {
                return null;
            }

            DungeonAsset finalFrame = _clientEntityService.FullInstantiate(doorFrame.Asset);

            WeightedDungeonDoor weightedDoor = RandUtils.GetRandomElement(doorAssetBlock.Doors, rand);

            if (weightedDoor != null && weightedDoor.Door != null)
            {
                DungeonDoor doorAsset = _clientEntityService.FullInstantiate<DungeonDoor>(weightedDoor.Door);

                _clientEntityService.AddToParent(doorAsset, finalFrame);

                finalFrame.Door = doorAsset;

                List<MeshRenderer> targetList = finalFrame.StoneRenderers;
                if (rand.NextDouble() < doorAsset.WoodMaterialChance)
                {
                    targetList = finalFrame.WoodRenderers;
                }

                foreach (DungeonDoorPanel panel in doorAsset.Panels)
                {
                    foreach (MeshRenderer renderer in panel.Renderers)
                    {
                        targetList.Add(renderer);
                    }
                }
            }

            return finalFrame;
        }

        protected void AddWallComponent(CrawlerMapRoot mapRoot, ClientMapCell cell, MaterialBlock block, int assetPositionIndex, int dungeonAssetIndex, GameObject parent, Vector3 offset, Vector3 euler)
        {
            List<WeightedDungeonAsset> assetList = mapRoot.AssetBlock.GetAssetList(dungeonAssetIndex);


            DungeonAsset asset = null;

            if (assetList.Count > 0)
            {
                asset = assetList[0].Asset;
            }

            IRandom rand = new MyRandom(mapRoot.Map.ArtSeed + cell.MapX * 7079 + cell.MapZ * 2383 + (int)offset.x * 3361 + (int)offset.y * 709 + (int)offset.z * 4327);

            bool haveFinalAsset = false;
            if (dungeonAssetIndex == DungeonPrefabIndexes.Pillars)
            {
                asset = mapRoot.PillarAsset;
            }
            else if (dungeonAssetIndex == DungeonPrefabIndexes.Door)
            {
                asset = GetFinalDoorAsset(rand, mapRoot, cell, block);
                haveFinalAsset = true;
            }
            else if (dungeonAssetIndex != DungeonPrefabIndexes.Ceilings || mapRoot.VaultedCeilingAssets == null)
            {

                if (assetList.Count > 1)
                {
                    WeightedDungeonAsset weightedAsset = RandUtils.GetRandomElement(assetList, rand);

                    asset = weightedAsset.Asset;
                }
            }
            else
            {

                AssetWithAngle assetAngle = GetVaultedCeilingAsset(mapRoot, cell, assetList, cell.MapX, cell.MapZ);
                asset = assetAngle.Asset;
                euler = new Vector3(0, assetAngle.YAngle, 0);
            }

            if (asset == null)
            {
                _logService.Warning("Missing Dungeon asset for index: " + mapRoot.AssetBlockList.name + " -- " + dungeonAssetIndex + " at " + cell.MapX + " " + cell.MapZ);
                return;
            }

            DungeonAsset dungeonAsset = asset;
            if (!haveFinalAsset)
            {
                dungeonAsset = _clientEntityService.FullInstantiate(asset);
            }



            cell.AssetPositions[assetPositionIndex] = dungeonAsset;
            _clientEntityService.AddToParent(dungeonAsset, parent);


            dungeonAsset.transform.localPosition = offset;
            dungeonAsset.transform.eulerAngles = euler;

            List<MaterialOption> materialList = block.FinalMaterials.GetMaterials(dungeonAssetIndex);

            long weightHash = cell.MapX * 1951 + cell.MapZ * 443 + (int)offset.x * 197 + (int)offset.y * 2843 + (int)offset.z * 653;

            for (int materialIndex = 0; materialIndex < DungeonMaterialIndexes.Max; materialIndex++)
            {
                Material finalMat = block.GetRandomMaterial(materialIndex, weightHash + materialIndex * 131);

                foreach (MeshRenderer renderer in dungeonAsset.GetRenderersForMaterialIndex(materialIndex))
                {
                    renderer.sharedMaterial = finalMat;
                }
            }
        }


        class AssetWithAngle
        {
            public DungeonAsset Asset = null;
            public int YAngle = 0;
        }

        class CornerCountData
        {
            public bool[] ShortCorners = new bool[4];
            public int FirstTallCornerIndex = -1;
            public int FirstShortCornerIndex = -1;
            public int TallCornerCount = 0;
            public int ShortCornerCount = 0;
        }

        private AssetWithAngle GetVaultedCeilingAsset(CrawlerMapRoot mapRoot, ClientMapCell cell, List<WeightedDungeonAsset> assetList, int realMapX, int realMapZ)
        {
            AssetWithAngle angle = new AssetWithAngle()
            {

            };

            if (assetList.Count < 6)
            {
                angle.Asset = assetList[0].Asset;
            }

            DungeonAsset asset = assetList[0].Asset;


            CornerCountData countData = new CornerCountData();

            CheckCorner(mapRoot, countData, realMapX, realMapZ, 0);
            CheckCorner(mapRoot, countData, realMapX - 1, realMapZ, 1);
            CheckCorner(mapRoot, countData, realMapX - 1, realMapZ - 1, 2);
            CheckCorner(mapRoot, countData, realMapX, realMapZ - 1, 3);

            if (countData.TallCornerCount == 0)
            {
                angle.Asset = mapRoot.VaultedCeilingAssets.LowCeiling;
            }
            else if (countData.TallCornerCount == 4)
            {
                angle.Asset = mapRoot.VaultedCeilingAssets.HighCeiling;
            }
            else if (countData.TallCornerCount == 1)
            {
                angle.Asset = mapRoot.VaultedCeilingAssets.OneCornerUp;
                angle.YAngle = 90 * (5 - countData.FirstTallCornerIndex);


            }
            else if (countData.TallCornerCount == 3)
            {
                angle.Asset = mapRoot.VaultedCeilingAssets.ThreeCornersUp;
                angle.YAngle = 90 * (3 - countData.FirstShortCornerIndex);
            }
            else
            {
                if (countData.ShortCorners[0] && countData.ShortCorners[2] ||
                    countData.ShortCorners[1] && countData.ShortCorners[3])
                {
                    angle.Asset = mapRoot.VaultedCeilingAssets.SaddlePoint;

                    int offset = (countData.ShortCorners[0] && countData.ShortCorners[2] ? 2 : 0);

                    angle.YAngle = 90 * (countData.FirstTallCornerIndex + offset);
                }
                else
                {
                    int startIndex = 3;

                    for (int i = 0; i < 3; i++)
                    {
                        if (countData.ShortCorners[i] && countData.ShortCorners[i + 1])
                        {
                            startIndex = i;
                            break;
                        }
                    }

                    angle.Asset = mapRoot.VaultedCeilingAssets.OneEdgeUp;
                    angle.YAngle = 90 * (3 - startIndex);
                }
            }

            return angle;
        }

        private void CheckCorner(CrawlerMapRoot mapRoot, CornerCountData countData, int mapX, int mapZ, int cornerIndex)
        {
            if (mapRoot.HasWallInNECorner(mapX, mapZ))
            {
                countData.ShortCorners[cornerIndex] = true;
                countData.ShortCornerCount++;

                if (countData.FirstShortCornerIndex == -1)
                {
                    countData.FirstShortCornerIndex = cornerIndex;
                }
            }
            else
            {

                countData.TallCornerCount++;
                if (countData.FirstTallCornerIndex == -1)
                {
                    countData.FirstTallCornerIndex = cornerIndex;
                }
            }
        }
    }
}


