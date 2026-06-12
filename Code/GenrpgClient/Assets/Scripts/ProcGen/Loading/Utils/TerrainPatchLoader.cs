
using Assets.Scripts.Assets.Constants;
using Assets.Scripts.MapTerrain;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.MapServer.Constants;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Pathfinding.Constants;
using OxDb.SharedGame.ProcGen.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine; // Needed

public interface ITerrainPatchLoader : IInitializable
{
    void LoadOneTerrainPatch(int gx, int gy, bool fastLoading, CancellationToken token);
}

public class TerrainPatchLoader : BaseZoneGenerator, ITerrainPatchLoader
{

    private IPlantAssetLoader _plantAssetLoader = null;
    private ITerrainTextureManager _terrainTextureManager = null;
    private IPlayerManager _playerManager = null;

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
    }

    public void OnError(string txt)
    {
        _logService.Error("Zone load error: " + txt);

    }

    public void LoadOneTerrainPatch(int gx, int gy, bool fastLoading, CancellationToken token)
    {
        _token = token;
        _awaitableService.ForgetAwaitable(InnerLoadOneTerrainPatch(gx, gy, fastLoading, token));
    }

    private async Awaitable InnerLoadOneTerrainPatch(int gx, int gy, bool fastLoading, CancellationToken token)
    {

        try
        {
            if (gx < 0 || gy < 0 ||
                _mapProvider.GetMap() == null)
            {
                OnError("Missing basic data");
                return;
            }
            TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gy);

            if (patch == null)
            {
                return;
            }

            if (patch.DataBytes == null)
            {
                string filePath = patch.GetFilePath();

                patch.DataBytes = await _clientRepoService.LoadBytes(filePath);

                await Awaitable.NextFrameAsync(cancellationToken: token);

                if (patch.DataBytes == null || patch.DataBytes.Length < 1)
                {

                    DownloadFileData ddata = new DownloadFileData()
                    {
                        Handler = OnDownloadTerrainBytes,
                        Data = patch,
                        Category = EDataCategories.Worlds,
                    };
                    _fileDownloadService.DownloadFile(filePath, ddata, token);
                    return;
                }
                else
                {
                    _terrainManager.SetTerrainPatchAtGridLocation(gx, gy, _mapProvider.GetMap(), patch);
                }
            }

            _terrainManager.IncrementPatchesAdded();
            await Awaitable.NextFrameAsync(cancellationToken: token);

            if (patch.terrain == null)
            {
                await _terrainManager.SetupOneTerrainPatch(gx, gy, token);
            }

            Terrain terr = patch.terrain as Terrain;

            if (terr == null)
            {
                OnError("No patch terrain setup at " + patch.X + " " + patch.Y);
                return;
            }

            _terrainManager.SetTerrainPatchAtGridLocation(patch.X, patch.Y, _mapProvider.GetMap(), patch);

            int startX = patch.Y * (MapConstants.TerrainPatchSize - 1);
            int startY = patch.X * (MapConstants.TerrainPatchSize - 1);


            SetTerrainTextures setTextures = new SetTerrainTextures();

            // 1. Heights 2
            // 2. Objects 4
            // 3. Alphas 3
            // 4. Zone 1 
            // 5. SubZone 1
            // 6. OverrideZoneScale 1

            // 2 + 4 + 3 + 1 + 1 + 1 = 12;

            ushort shortHeight = 0;
            int index = 0;
            int xx = 0;
            int yy = 0;

            if (patch.heights == null)
            {
                patch.heights = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
            }

            // 1 Heights (2 bytes)
            try
            {
                for (xx = 0; xx < MapConstants.TerrainPatchSize; xx++)
                {
                    for (yy = 0; yy < MapConstants.TerrainPatchSize; yy++)
                    {
                        shortHeight = patch.DataBytes[index++];
                        shortHeight += (ushort)(patch.DataBytes[index++] << 8);
                        patch.heights[xx, yy] = 1.0f * shortHeight / MapConstants.HeightSaveMult;
                    }
                }
            }
            catch (Exception e)
            {
                string bytelen = (patch.DataBytes == null ? "NullBytes" : "Len: " + patch.DataBytes.Length);
                _logService.Exception(e, "LoadMap3: " + xx + " " + yy + " Len: " + bytelen + " Idx: " + index);
            }

            if (!fastLoading)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
            try
            {
                if (patch.grassAmounts == null)
                {
                    patch.grassAmounts = new ushort[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, MapConstants.MaxGrass];
                }

                if (patch.entityIds == null)
                {

                }

                if (patch.entityTypeIds == null)
                {
                    patch.entityTypeIds = new byte[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
                }

                if (patch.entityIds == null)
                {
                    patch.entityIds = new byte[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
                }

                // 2 Objects (2 bytes)
                for (int x = 0; x < MapConstants.TerrainPatchSize - 1; x++)
                {
                    for (int y = 0; y < MapConstants.TerrainPatchSize - 1; y++)
                    {
                        patch.entityTypeIds[x, y] = patch.DataBytes[index++];
                        patch.entityIds[x, y] = patch.DataBytes[index++];
                    }
                }


                for (int x = 0; x < MapConstants.TerrainPatchSize - 1; x++)
                {
                    for (int y = 0; y < MapConstants.TerrainPatchSize - 1; y++)
                    {
                        if (patch.entityTypeIds[x, y] == EntityTypes.Plant)
                        {
                            int div = (MapConstants.MaxGrassValue + 1);

                            int entityId = patch.entityIds[x, y];

                            for (int i = 0; i < MapConstants.MaxGrass; i++)
                            {
                                patch.grassAmounts[y, x, i] = (ushort)(entityId % div);
                                entityId /= div;
                            }
                        }
                    }
                }

                // Push grass to edges

                for (int t = 0; t < MapConstants.TerrainPatchSize; t++)
                {
                    int horizOffset = (t * 13 + gx * 17 + gy * 29) % MapConstants.TerrainPatchSize;

                    uint horizOffsetEntityType = patch.entityTypeIds[horizOffset,
                        MapConstants.TerrainPatchSize - 1];

                    if (horizOffsetEntityType == EntityTypes.Plant)
                    {
                        patch.entityTypeIds[t, MapConstants.TerrainPatchSize - 1] = (byte)EntityTypes.Plant;
                        patch.entityIds[t, MapConstants.TerrainPatchSize - 1] = patch.entityIds[horizOffset, MapConstants.TerrainPatchSize - 1];
                    }

                    int vertOffset = (t * 23 + gx * 53 + gy * 71) % MapConstants.TerrainPatchSize;

                    uint vertOffsetEntityType = patch.entityTypeIds[
                        MapConstants.TerrainPatchSize - 1, vertOffset];

                    if (vertOffsetEntityType == EntityTypes.Plant)
                    {
                        patch.entityTypeIds[MapConstants.TerrainPatchSize - 1, t] = (byte)EntityTypes.Plant;
                        patch.entityIds[MapConstants.TerrainPatchSize - 1, t] = patch.entityIds[MapConstants.TerrainPatchSize - 1, horizOffset];
                    }
                }

                for (int x = 0; x < MapConstants.TerrainPatchSize - 1; x++)
                {
                    for (int y = 0; y < MapConstants.TerrainPatchSize - 1; y++)
                    {
                        if (patch.entityTypeIds[x, y] == EntityTypes.Plant)
                        {
                            int div = (MapConstants.MaxGrassValue + 1);

                            int entityId = patch.entityIds[x, y];

                            for (int i = 0; i < MapConstants.MaxGrass; i++)
                            {
                                patch.grassAmounts[y, x, i] = (ushort)(entityId % div);
                                entityId /= div;
                            }

                            patch.entityTypeIds[x, y] = 0;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                _logService.Exception(e, "LoadMap2");
            }

            if (!fastLoading)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }

            if (patch.baseAlphas == null)
            {
                patch.baseAlphas = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, TerrainTexChannels.Max];
            }

            // 3 Alphas (3 bytes) 
            float alphaTotal = 0;
            float alphaDiv = MapConstants.AlphaSaveMult * 1.0f;
            for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
            {
                for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
                {
                    alphaTotal = 0;
                    for (int i = 0; i < TerrainTexChannels.Max - 1; i++)
                    {
                        try
                        {
                            patch.baseAlphas[x, y, i] = patch.DataBytes[index++] / alphaDiv;
                        }
                        catch (Exception e)
                        {
                            _logService.Exception(e, "LoadMap");
                            throw e;
                        }
                        alphaTotal += patch.baseAlphas[x, y, i];
                    }
                    if (alphaTotal < 1)
                    {
                        patch.baseAlphas[x, y, TerrainTexChannels.Max - 1] = 1 - alphaTotal;
                    }
                    else if (alphaTotal > 1)
                    {
                        for (int i = 0; i < TerrainTexChannels.Max; i++)
                        {
                            patch.baseAlphas[x, y, i] /= alphaTotal;
                        }
                    }
                }
            }

            // 4 ZoneId (1 byte)
            List<long> subZoneIds = new List<long>();
            List<long> mainZoneIds = new List<long>();
            if (_mapProvider.GetMap().OverrideZoneId > 0)
            {
                mainZoneIds.Add((int)_mapProvider.GetMap().OverrideZoneId);
                subZoneIds.Add((int)_mapProvider.GetMap().OverrideZoneId);
            }

            List<IdVal> mainZoneQuantities = new List<IdVal>();

            for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
            {
                for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
                {
                    byte newMainZoneId = patch.DataBytes[index++];
                    patch.mainZoneIds[x, y] = newMainZoneId;
                    if (newMainZoneId >= SharedMapConstants.MapZoneStartId)
                    {
                        if (!subZoneIds.Contains(newMainZoneId))
                        {
                            subZoneIds.Add(newMainZoneId);
                        }
                        if (!mainZoneIds.Contains(newMainZoneId))
                        {
                            mainZoneIds.Add(newMainZoneId);
                        }

                        IdVal zoneQuantity = mainZoneQuantities.FirstOrDefault(x => x.Id == newMainZoneId);

                        if (zoneQuantity == null)
                        {
                            zoneQuantity = new IdVal() { Id = newMainZoneId };
                            mainZoneQuantities.Add(zoneQuantity);
                        }

                        zoneQuantity.Val++;
                    }
                }
            }

            mainZoneIds = mainZoneQuantities.OrderByDescending(x => x.Val).Select(x => x.Id).ToList();

            if (!fastLoading)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }

            // 5 subzoneId (1 byte)
            for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
            {
                for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
                {
                    patch.subZoneIds[x, y] = patch.DataBytes[index++];
                    if (patch.subZoneIds[x, y] > 0 && !subZoneIds.Contains(patch.subZoneIds[x, y]))
                    {
                        subZoneIds.Add(patch.subZoneIds[x, y]);
                    }
                }
            }


            // 6 OverrideZonePercent (1 byte)
            for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
            {
                for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
                {
                    patch.overrideZoneScales[x, y] = patch.DataBytes[index++];
                    if (patch.overrideZoneScales[x, y] <= _mapProvider.GetMap().OverrideZonePercent)
                    {
                        patch.subZoneIds[x, y] = (byte)_mapProvider.GetMap().OverrideZoneId;
                    }
                }
            }


            patch.FullZoneIdList = new List<long>();
            patch.MainZoneIdList = new List<long>();
            foreach (int zid in subZoneIds)
            {
                patch.FullZoneIdList.Add(zid);
            }

            foreach (int zid in mainZoneIds)
            {
                patch.MainZoneIdList.Add(zid);
            }

            await _terrainTextureManager.SetOneTerrainPatchLayers(patch, token);

            if (!fastLoading)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
            else
            {
                await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);
            }

            await _zoneGenService.SetOnePatchAlphamaps(patch, token);

            if (!fastLoading)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
            _zoneGenService.SetOnePatchHeightmaps(patch, null, patch.heights);


            if (!fastLoading)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }

            _plantAssetLoader.SetupOneMapGrass(patch.X, patch.Y, token);

            if (!fastLoading)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }

            _terrainManager.SetOneTerrainNeighbors(patch.X, patch.Y);

            _terrainManager.SetOneTerrainNeighbors(patch.X + 1, patch.Y);

            _terrainManager.SetOneTerrainNeighbors(patch.X - 1, patch.Y);

            _terrainManager.SetOneTerrainNeighbors(patch.X, patch.Y - 1);

            _terrainManager.SetOneTerrainNeighbors(patch.X, patch.Y + 1);

            if (!fastLoading)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
            await _terrainManager.AddPatchObjects(gx, gy, token);

            _terrainManager.RemoveLoadingPatches(gx, gy);

            if (false && _pathfindingService.GetPathfinding() != null)
            {
                for (int px = 0; px < MapConstants.TerrainPatchSize; px += PathfindingConstants.BlockSize)
                {
                    int worldx = gx * (MapConstants.TerrainPatchSize - 1) + px;
                    for (int pz = 0; pz < MapConstants.TerrainPatchSize; pz += PathfindingConstants.BlockSize)
                    {
                        int worldz = gy * (MapConstants.TerrainPatchSize - 1) + pz;

                        int finalx = worldx / PathfindingConstants.BlockSize;
                        int finalz = worldz / PathfindingConstants.BlockSize;

                        if (finalx < 0 || finalx >= _pathfindingService.GetPathfinding().GetLength(0) ||
                            finalz < 0 || finalz >= _pathfindingService.GetPathfinding().GetLength(1))
                        {
                            continue;
                        }
                        if (false && _pathfindingService.GetPathfinding()[finalx, finalz])
                        {
                            float height = _terrainManager.SampleHeight(worldx, worldz);
                            GameObject sphere = (GameObject)(await _assetService.LoadAssetAsync(AssetCategoryNames.Prefabs, "TestSphere", null, token));
                            sphere.name = "TestSphere_" + worldx + "_" + worldz;
                            _clientEntityService.AddToParent(sphere, patch.terrain.gameObject);
                            sphere.transform.position = new Vector3(worldx, height, worldz);
                        }
                    }
                }
            }
        }
        catch (Exception ebig)
        {
            _logService.Exception(ebig, "LoadOneTerrainPatch");
        }
    }

    private void OnDownloadTerrainBytes(object obj, object data, CancellationToken token)
    {
        TerrainPatchData patch = data as TerrainPatchData;

        if (patch == null)
        {
            return;
        }
        if (obj == null)
        {
            return;
        }

        byte[] bytes = obj as byte[];

        if (bytes == null || bytes.Length < 10)
        {
            string txt = "No bytes";
            if (bytes != null)
            {
                txt = System.Text.Encoding.UTF8.GetString(bytes);
            }
            _logService.Error("Failed to download Bytes");
            return;
        }

        string filePath = patch.GetFilePath();
        _awaitableService.ForgetAwaitable(_clientRepoService.SaveBytes(filePath, bytes));
        patch.DataBytes = bytes;
        LoadOneTerrainPatch(patch.X, patch.Y, _playerManager.GetPlayerGameObject() == null, _token);
    }
}



