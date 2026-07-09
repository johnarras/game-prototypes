
using Assets.Scripts.Assets.Constants;
using Assets.Scripts.MapTerrain;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.MapServer.Constants;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Pathfinding.Constants;
using OxDb.SharedGame.ProcGen.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine; // Needed

public interface ITerrainPatchLoader : IInitializable
{
    void LoadOneTerrainPatch(int gx, int gz, bool fastLoading, CancellationToken token);
}

public class TerrainPatchLoader : BaseZoneGenerator, ITerrainPatchLoader
{

    private IPlantAssetLoader _plantAssetLoader = null;
    private ITerrainTextureManager _terrainTextureManager = null;
    private IPlayerManager _playerManager = null;
    private ITextSerializer _textSerializer = null;

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
    }

    public void OnError(string txt)
    {
        _logService.Error("Zone load error: " + txt);

    }

    public void LoadOneTerrainPatch(int gx, int gz, bool fastLoading, CancellationToken token)
    {
        _token = token;
        _awaitableService.ForgetAwaitable(InnerLoadOneTerrainPatch(gx, gz, fastLoading, token));
    }

    private async Awaitable InnerLoadOneTerrainPatch(int gx, int gz, bool fastLoading, CancellationToken token)
    {

        try
        {
            if (gx < 0 || gz < 0 ||
                _mapProvider.GetMap() == null)
            {
                OnError("Missing basic data");
                return;
            }
            TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gz);

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
                    _terrainManager.SetTerrainPatchAtGridLocation(gx, gz, _mapProvider.GetMap(), patch);
                }
            }

            _terrainManager.IncrementPatchesAdded();
            await Awaitable.NextFrameAsync(cancellationToken: token);

            if (patch.Core.Terrain == null)
            {
                await _terrainManager.SetupOneTerrainPatch(gx, gz, token);
            }

            Terrain terr = patch.Core.Terrain;

            if (terr == null)
            {
                OnError("No patch terrain setup at " + patch.Core.GX + " " + patch.Core.GZ);
                return;
            }

            _terrainManager.SetTerrainPatchAtGridLocation(patch.Core.GX, patch.Core.GZ, _mapProvider.GetMap(), patch);

            int startX = patch.Core.GZ * (MapConstants.TerrainPatchSize - 1);
            int startY = patch.Core.GX * (MapConstants.TerrainPatchSize - 1);


            SetTerrainTextures setTextures = new SetTerrainTextures();

            // 1. Heights 2
            // 2. Objects 2
            // 3. Alphas 3
            // 4. Zone 1 
            // 5. SubZone 1
            // 6. OverrideZoneScale 1

            // 2 + 4 + 3 + 1 + 1 + 1 = 10;

            ushort shortHeight = 0;
            int index = 0;
            int xx = 0;
            int zz = 0;

            if (patch.heights == null)
            {
                patch.heights = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
            }

            // 1 Heights (2 bytes)
            try
            {
                for (xx = 0; xx < MapConstants.TerrainPatchSize; xx++)
                {
                    for (zz = 0; zz < MapConstants.TerrainPatchSize; zz++)
                    {
                        shortHeight = patch.DataBytes[index++];
                        shortHeight += (ushort)(patch.DataBytes[index++] << 8);
                        patch.heights[xx, zz] = 1.0f * shortHeight / MapConstants.HeightSaveMult;
                    }
                }
            }
            catch (Exception e)
            {
                string bytelen = (patch.DataBytes == null ? "NullBytes" : "Len: " + patch.DataBytes.Length);
                _logService.Exception(e, "LoadMap3: " + xx + " " + zz + " Len: " + bytelen + " Idx: " + index);
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
                    for (int z = 0; z < MapConstants.TerrainPatchSize - 1; z++)
                    {
                        patch.entityTypeIds[x, z] = patch.DataBytes[index++];
                        patch.entityIds[x, z] = patch.DataBytes[index++];
                    }
                }


                for (int x = 0; x < MapConstants.TerrainPatchSize - 1; x++)
                {
                    for (int z = 0; z < MapConstants.TerrainPatchSize - 1; z++)
                    {
                        if (patch.entityTypeIds[x, z] == EntityTypes.Plant)
                        {
                            int div = (MapConstants.MaxGrassValue + 1);

                            int entityId = patch.entityIds[x, z];

                            for (int i = 0; i < MapConstants.MaxGrass; i++)
                            {
                                patch.grassAmounts[z, x, i] = (ushort)(entityId % div);
                                entityId /= div;
                            }
                        }
                    }
                }

                // Push grass to edges

                for (int t = 0; t < MapConstants.TerrainPatchSize; t++)
                {
                    int horizOffset = (t * 13 + gx * 17 + gz * 29) % MapConstants.TerrainPatchSize;

                    uint horizOffsetEntityType = patch.entityTypeIds[horizOffset,
                        MapConstants.TerrainPatchSize - 1];

                    if (horizOffsetEntityType == EntityTypes.Plant)
                    {
                        patch.entityTypeIds[t, MapConstants.TerrainPatchSize - 1] = (byte)EntityTypes.Plant;
                        patch.entityIds[t, MapConstants.TerrainPatchSize - 1] = patch.entityIds[horizOffset, MapConstants.TerrainPatchSize - 1];
                    }

                    int vertOffset = (t * 23 + gx * 53 + gz * 71) % MapConstants.TerrainPatchSize;

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
                    for (int z = 0; z < MapConstants.TerrainPatchSize - 1; z++)
                    {
                        if (patch.entityTypeIds[x, z] == EntityTypes.Plant)
                        {
                            int div = (MapConstants.MaxGrassValue + 1);

                            int entityId = patch.entityIds[x, z];

                            for (int i = 0; i < MapConstants.MaxGrass; i++)
                            {
                                patch.grassAmounts[z, x, i] = (ushort)(entityId % div);
                                entityId /= div;
                            }

                            patch.entityTypeIds[x, z] = 0;
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
                for (int z = 0; z < MapConstants.TerrainPatchSize; z++)
                {
                    alphaTotal = 0;
                    for (int i = 0; i < TerrainTexChannels.Max - 1; i++)
                    {
                        try
                        {
                            patch.baseAlphas[x, z, i] = patch.DataBytes[index++] / alphaDiv;
                        }
                        catch (Exception e)
                        {
                            _logService.Exception(e, "LoadMap");
                            throw e;
                        }
                        alphaTotal += patch.baseAlphas[x, z, i];
                    }
                    if (alphaTotal < 1)
                    {
                        patch.baseAlphas[x, z, TerrainTexChannels.Max - 1] = 1 - alphaTotal;
                    }
                    else if (alphaTotal > 1)
                    {
                        for (int i = 0; i < TerrainTexChannels.Max; i++)
                        {
                            patch.baseAlphas[x, z, i] /= alphaTotal;
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
                for (int z = 0; z < MapConstants.TerrainPatchSize; z++)
                {
                    byte newMainZoneId = patch.DataBytes[index++];
                    patch.mainZoneIds[x, z] = newMainZoneId;
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
                for (int z = 0; z < MapConstants.TerrainPatchSize; z++)
                {
                    patch.subZoneIds[x, z] = patch.DataBytes[index++];
                    if (patch.subZoneIds[x, z] > 0 && !subZoneIds.Contains(patch.subZoneIds[x, z]))
                    {
                        subZoneIds.Add(patch.subZoneIds[x, z]);
                    }
                }
            }


            // 6 OverrideZonePercent (1 byte)
            for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
            {
                for (int z = 0; z < MapConstants.TerrainPatchSize; z++)
                {
                    patch.overrideZoneScales[x, z] = patch.DataBytes[index++];
                    if (patch.overrideZoneScales[x, z] <= _mapProvider.GetMap().OverrideZonePercent)
                    {
                        patch.subZoneIds[x, z] = (byte)_mapProvider.GetMap().OverrideZoneId;
                    }
                }
            }

            int coreLength = MapConstants.TerrainPatchSize * MapConstants.TerrainPatchSize * 10;

            int extraDataSize = patch.DataBytes.Length - coreLength;

            if (extraDataSize > 0)
            {

                ArraySegment<byte> jsonSegment = new ArraySegment<byte>(patch.DataBytes, coreLength, extraDataSize);

                // 2. Wrap it in a MemoryStream so readers can handle it stream-style
                // Setting the publiclyVisible parameter to false prevents internal copying
                using (MemoryStream stream = new MemoryStream(jsonSegment.Array, jsonSegment.Offset, jsonSegment.Count, false))
                {
                    // 3. Decode to string and Deserialize
                    // Note: If you are using JsonUtility, it requires a string.
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string jsonString = reader.ReadToEnd();

                        List<ExtendedWorldObjectData> deserializedData = _textSerializer.Deserialize<List<ExtendedWorldObjectData>>(jsonString);

                        patch.ExtendedObjects = deserializedData;

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

            List<long> allZoneIds = patch.FullZoneIdList.Concat(patch.MainZoneIdList).Distinct().ToList();

            await _terrainTextureManager.SetupTerrainContainerLayers(patch, allZoneIds, new List<long>(), token);

            if (!fastLoading)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
            else
            {
                await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);
            }

            while (!patch.Core.IsReady())
            {
                await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken: token);
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

            _plantAssetLoader.SetupOneMapGrass(patch.Core.GX, patch.Core.GZ, token);

            if (!fastLoading)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }

            _terrainManager.SetOneTerrainNeighbors(patch.Core.GX, patch.Core.GZ);

            _terrainManager.SetOneTerrainNeighbors(patch.Core.GX + 1, patch.Core.GZ);

            _terrainManager.SetOneTerrainNeighbors(patch.Core.GX - 1, patch.Core.GZ);

            _terrainManager.SetOneTerrainNeighbors(patch.Core.GX, patch.Core.GZ - 1);

            _terrainManager.SetOneTerrainNeighbors(patch.Core.GX, patch.Core.GZ + 1);

            if (!fastLoading)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
            await _terrainManager.AddPatchObjects(gx, gz, token);

            _terrainManager.RemoveLoadingPatches(gx, gz);

            if (false && _pathfindingService.GetPathfinding() != null)
            {
                for (int px = 0; px < MapConstants.TerrainPatchSize; px += PathfindingConstants.BlockSize)
                {
                    int worldx = gx * (MapConstants.TerrainPatchSize - 1) + px;
                    for (int pz = 0; pz < MapConstants.TerrainPatchSize; pz += PathfindingConstants.BlockSize)
                    {
                        int worldz = gz * (MapConstants.TerrainPatchSize - 1) + pz;

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
                            _clientEntityService.AddToParent(sphere, patch.Core.Terrain.gameObject);
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
        LoadOneTerrainPatch(patch.Core.GX, patch.Core.GZ, _playerManager.GetPlayerGameObject() == null, _token);
    }
}



