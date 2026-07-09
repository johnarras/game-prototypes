using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.GameObjects;
using Assets.Scripts.MapTerrain;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.ProcGen.Services;
using OxDb.SharedGame.ProcGen.Settings.Textures;
using OxDb.SharedGame.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services
{
    public interface ICrawlerTerrainService : IInjectable
    {
        Awaitable DrawTerrain(CrawlerWorld world, PartyData party, CrawlerMapRoot mapRoot, CancellationToken token);
    }

    public class CrawlerTerrainService : ICrawlerTerrainService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IClientEntityService _clientEntityService = null;
        private IMapTerrainManager _mapTerrainManager = null;

        private INoiseService _noiseService = null;

        private ITerrainTextureManager _terrainTextureManager = null;

        const int WorldUnitsPerCell = 1;

        public async Awaitable DrawTerrain(CrawlerWorld world, PartyData party, CrawlerMapRoot mapRoot, CancellationToken token)
        {

            List<long> allZoneTypes = mapRoot.GetAllZoneTypes();

            if (mapRoot.MaterialBlocks.Count == 0 || mapRoot.MaterialBlocks.Any(x => !x.Value.IsReady()))
            {
                return;
            }
            int mapWidth = mapRoot.Map.Width;
            int mapHeight = mapRoot.Map.Height;

            int startMapMaxSize = Math.Max(mapWidth, mapHeight);

            int extraViewRadius = CrawlerDrawMapService.ViewRadius + 3;

            int heightCellsPerWorldBlock = mapRoot.XZBlockSize / WorldUnitsPerCell;

            int alphaCellsPerWorldBlock = heightCellsPerWorldBlock;

            int terrainOffset = (extraViewRadius) * heightCellsPerWorldBlock
                + heightCellsPerWorldBlock / 2; // Needed since objects are centered.
            int totalMapCellCount = startMapMaxSize + 2 * extraViewRadius;

            int startTerrainSize = totalMapCellCount * mapRoot.XZBlockSize / WorldUnitsPerCell;

            double log2 = Math.Log(startTerrainSize, 2);

            startTerrainSize = (int)Math.Pow(2, Math.Ceiling(log2));

            int heightMapSize = startTerrainSize + 1;

            mapRoot.Core.TerrainSize = heightMapSize;
            mapRoot.Core.WorldUnitsPerCell = WorldUnitsPerCell;
            mapRoot.TerrainParent = new GameObject() { name = "TerrainParent" };

            _clientEntityService.AddToParent(mapRoot.TerrainParent, mapRoot);
            mapRoot.TerrainParent.transform.localPosition = new Vector3(-terrainOffset, 0, -terrainOffset);
            await _mapTerrainManager.InitTerrainContainer(mapRoot, token);

            _clientEntityService.AddToParent(mapRoot.Core.Terrain.gameObject, mapRoot.TerrainParent);

            List<long> noiseIndexes = new List<long>();

            TextureTypeSettings textureSettings = _gameData.Get<TextureTypeSettings>(_gs.ch);

            ZoneTypeSettings zoneSettings = _gameData.Get<ZoneTypeSettings>(_gs.ch);

            List<string> detailTextureNames = new List<string>() { "Rock", "Dirt" };

            List<long> detailTextureTypeIds = new List<long>();

            foreach (string detailName in detailTextureNames)
            {
                TextureType detailTexture = textureSettings.GetData().FirstOrDefault(x => x.Name == detailName);

                if (detailTexture != null && !string.IsNullOrEmpty(detailTexture.Art))
                {
                    detailTextureTypeIds.Add(detailTexture.IdKey);
                }
            }

            ZoneType starsZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Name == "Stars");

            allZoneTypes = allZoneTypes.OrderBy(x => x).ToList();

            if (starsZoneType != null)
            {
                allZoneTypes.Insert(0, starsZoneType.IdKey);
            }

            await _terrainTextureManager.SetupTerrainContainerLayers(mapRoot, allZoneTypes, detailTextureTypeIds, token);

            while (!mapRoot.Core.IsReady())
            {
                await Awaitable.NextFrameAsync(token);
            }

            List<long> detailIgnoreAlphaIndexes = new List<long>();

            int alphamapSize = mapRoot.Core.TerrainData.alphamapHeight;

            long[,] zoneTypeIds = new long[alphamapSize, alphamapSize];

            long[,] seeds = new long[alphamapSize, alphamapSize];

            float[,] terrainHeights = new float[heightMapSize, heightMapSize];

            float[,,] textureAlphas1 = new float[alphamapSize, alphamapSize, mapRoot.Core.Layers.Count];
            float[,,] textureAlphas2 = new float[alphamapSize, alphamapSize, mapRoot.Core.Layers.Count];
            for (int x = 0; x < alphamapSize; x++)
            {
                for (int z = 0; z < alphamapSize; z++)
                {
                    textureAlphas1[x, z, 0] = 1;
                    textureAlphas2[x, z, 0] = 1;
                }
            }

            bool isLooping = mapRoot.Map.HasFlag(CrawlerMapFlags.IsLooping);
            for (int cx = 0; cx < totalMapCellCount; cx++)
            {
                if (!isLooping)
                {
                    if (cx < extraViewRadius || cx >= mapWidth + extraViewRadius)
                    {
                        continue;
                    }
                }

                int worldX = cx - extraViewRadius;
                while (worldX < 0)
                {
                    worldX += mapWidth;
                }

                worldX = worldX % mapWidth;

                for (int cz = 0; cz < totalMapCellCount; cz++)
                {
                    if (!isLooping)
                    {
                        if (cz < extraViewRadius || cz >= mapHeight + extraViewRadius)
                        {
                            continue;
                        }
                    }

                    int worldZ = cz - extraViewRadius;
                    while (worldZ < 0)
                    {
                        worldZ += mapHeight;
                    }

                    worldZ = worldZ % mapHeight;

                    long zoneTypeId = mapRoot.Map.Get(worldX, worldZ, CellIndex.Terrain);
                    for (int ix = 0; ix < alphaCellsPerWorldBlock; ix++)
                    {
                        int finalX = cx * alphaCellsPerWorldBlock + ix;
                        for (int iy = 0; iy < alphaCellsPerWorldBlock; iy++)
                        {
                            int finalY = cz * alphaCellsPerWorldBlock + iy;
                            zoneTypeIds[finalY, finalX] = zoneTypeId;

                            long seed = world.Seed * 31 + mapRoot.Map.IdKey * 59 +
                                worldX * 37 + worldZ * 23 + worldZ * ix + worldX * iy +
                                ix * 43 + iy * 61;

                            seeds[finalY, finalX] = seed;
                        }
                    }
                }
            }

            mapRoot.Core.SetLayers();

            for (int x = 0; x < alphamapSize; x++)
            {
                for (int z = 0; z < alphamapSize; z++)
                {

                    long zoneTypeId = zoneTypeIds[z, x];

                    if (zoneTypeId < 1)
                    {
                        textureAlphas1[z, x, 0] = 1;
                        continue;
                    }

                    IndexedTerrainLayer firstData = mapRoot.Core.Layers.FirstOrDefault(x => x.ZoneTypeId == zoneTypeId);

                    if (firstData != null)
                    {
                        textureAlphas1[z, x, 0] = 0;
                        textureAlphas1[z, x, firstData.Index] = 1;
                    }
                }
            }

            // Now adjust alpha edges.

            int shiftToSidePercent = 10;
            int shiftToMiddlePercent = 10;
            int delta = 0;
            for (int x = 1; x < alphamapSize - 1; x++)
            {
                for (int z = 0; z < alphamapSize; z++)
                {
                    if (zoneTypeIds[z, x] != zoneTypeIds[z, x + 1])
                    {
                        long seed = seeds[z, x];
                        if (delta == 0)
                        {
                            if ((seed * 131) % 100 < shiftToSidePercent)
                            {
                                if (seed % 2 == 0)
                                {
                                    delta = -1;
                                }
                                else
                                {
                                    delta = 1;
                                }
                            }
                        }
                        else if ((seed * 137) % 100 < shiftToMiddlePercent)
                        {
                            delta = 0;
                        }
                        if (delta == -1)
                        {
                            for (int l = 0; l < mapRoot.Core.Layers.Count; l++)
                            {
                                textureAlphas1[z, x, l] = textureAlphas1[z, x + 1, l];
                            }
                        }
                        else if (delta == 1)
                        {
                            for (int l = 0; l < mapRoot.Core.Layers.Count; l++)
                            {
                                textureAlphas1[z, x + 1, l] = textureAlphas1[z, x, l];
                            }
                        }
                    }
                }
            }


            for (int x = 1; x < alphamapSize - 1; x++)
            {
                for (int z = 1; z < alphamapSize - 1; z++)
                {
                    for (int l = 0; l < mapRoot.Core.Layers.Count; l++)
                    {
                        textureAlphas2[x, z, l] =
                            (textureAlphas1[x, z, l] * 4 +
                            textureAlphas1[x + 1, z, l] +
                            textureAlphas1[x - 1, z, l] +
                            textureAlphas1[x, z + 1, l] +
                            textureAlphas1[x, z - 1, l]) / 8;
                    }
                }
            }

            int detailTimes = 0;
            foreach (long detailTypeId in detailTextureTypeIds)
            {
                detailTimes++;

                IndexedTerrainLayer detailIndexData = mapRoot.Core.Layers.FirstOrDefault(x => x.TextureTypeId == detailTypeId);

                if (detailIndexData == null || !detailIndexData.IsReady())
                {
                    continue;
                }

                int detailIndex = detailIndexData.Index;
                IRandom rand = new MyRandom(world.Seed / 3 + mapRoot.Map.IdKey * 13 + detailTimes * 19);
                float freq = RandUtils.FloatRange(0.01f, 0.02f, rand) * alphamapSize;
                float amp = RandUtils.FloatRange(1.1f, 1.5f, rand);
                float pers = RandUtils.FloatRange(0.5f, 0.7f, rand);
                int octaves = 2;

                float[,] noiseVals = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), alphamapSize, alphamapSize);

                for (int x = 0; x < alphamapSize; x++)
                {
                    for (int z = 0; z < alphamapSize; z++)
                    {

                        int matchingIgnores = 0;
                        foreach (long alphaIndex in detailIgnoreAlphaIndexes)
                        {
                            if (textureAlphas2[x, z, alphaIndex] > 0)
                            {
                                matchingIgnores++;
                            }
                        }

                        if (matchingIgnores > 0)
                        {
                            continue;
                        }


                        float noiseVal = MathUtil.Clamp(0, noiseVals[x, z], 1);


                        for (int l = 0; l < mapRoot.Core.Layers.Count; l++)
                        {
                            textureAlphas2[x, z, l] *= (1 - noiseVal);
                            textureAlphas2[x, z, detailIndex] = noiseVal;
                        }
                    }
                }
            }

            mapRoot.Core.TerrainData.SetAlphamaps(0, 0, textureAlphas2);
            mapRoot.Core.Terrain.Flush();
        }
    }
}