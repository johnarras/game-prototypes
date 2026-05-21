using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Assets.Textures;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.GameObjects;
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
    public class CrawlerTerrainIndexData
    {
        public long ZoneTypeId { get; set; }
        public TextureList Textures { get; set; }

        public TextureType TextureType { get; set; }

        public Texture2D Diffuse { get; set; }
        public Texture2D Normal { get; set; }

        public int AlphaIndex { get; set; }

        public bool IsReady { get; set; }
    }

    public interface ICrawlerTerrainService : IInjectable
    {
        Awaitable DrawTerrain(CrawlerWorld world, PartyData party, CrawlerMapRoot mapRoot, CancellationToken token);

        void UpdateTerrainLayersFromTextures(CrawlerMapRoot mapRoot);
    }

    public class CrawlerTerrainService : ICrawlerTerrainService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IAssetService _assetService = null;
        private IClientEntityService _clientEntityService = null;
        private IZoneGenService _zoneGenService = null;
        private IMapTerrainManager _mapTerrainManager = null;

        private INoiseService _noiseService = null;

        public async Awaitable DrawTerrain(CrawlerWorld world, PartyData party, CrawlerMapRoot mapRoot, CancellationToken token)
        {

            List<long> allZoneTypes = mapRoot.GetAllZoneTypes();


            if (mapRoot.MaterialBlocks.Count == 0 || mapRoot.MaterialBlocks.Any(x => !x.Value.IsReady()))
            {
                return;
            }

            _clientEntityService.Destroy(mapRoot.TerrainObject);
            mapRoot.TerrainObject = (GameObject)(await _assetService.LoadAssetAsync(AssetCategoryNames.Prefabs, "TerrainMaterialPlaceholder", null, token));
            Terrain terr = mapRoot.TerrainObject.GetComponent<Terrain>();
            mapRoot.GroundTerrain = terr;
            mapRoot.TerrainObject.name = "CrawlerMapTerrain";
            _clientEntityService.AddToParent(mapRoot.TerrainObject, mapRoot);
            mapRoot.TerrainObject.transform.localPosition = Vector3.zero;
            TerrainData tdata = GameObject.Instantiate<TerrainData>(terr.terrainData);
            terr.terrainData = tdata;
            mapRoot.GroundTerrainData = tdata;
            tdata.detailPrototypes = new DetailPrototype[0];
            tdata.treePrototypes = new TreePrototype[0];

            int extraViewRadius = CrawlerDrawMapService.ViewRadius + 3;

            int heightCellsPerWorldBlock = mapRoot.XZBlockSize;

            int alphaCellsPerWorldBlock = heightCellsPerWorldBlock * 2;

            int terrainOffset = (extraViewRadius) * heightCellsPerWorldBlock
                + heightCellsPerWorldBlock / 2; // Needed since objects are centered.

            mapRoot.TerrainObject.transform.localPosition = new Vector3(-terrainOffset, 0, -terrainOffset);

            int mapWidth = mapRoot.Map.Width;
            int mapHeight = mapRoot.Map.Height;

            int startMapMaxSize = Math.Max(mapWidth, mapHeight);

            int totalMapCellCount = startMapMaxSize + 2 * extraViewRadius;

            int startTerrainSize = totalMapCellCount * mapRoot.XZBlockSize;

            double log2 = Math.Log(startTerrainSize, 2);

            startTerrainSize = (int)Math.Pow(2, Math.Ceiling(log2));

            int heightMapSize = startTerrainSize + 1;

            int alphamapSize = startTerrainSize * 2;

            List<long> noiseIndexes = new List<long>();

            TextureTypeSettings textureSettings = _gameData.Get<TextureTypeSettings>(_gs.ch);

            ZoneTypeSettings zoneSettings = _gameData.Get<ZoneTypeSettings>(_gs.ch);

            List<string> ignoreDetailZoneTypeNames = new List<string>() { "Water", "Stars" };
            List<string> detailTextureNames = new List<string>() { "Rock", "Dirt" };

            List<TextureType> detailTextureTypes = new List<TextureType>();

            List<long> ignoreZoneTypeIds = new List<long>();

            foreach (string ignoreName in ignoreDetailZoneTypeNames)
            {
                ZoneType ztype = zoneSettings.GetData().FirstOrDefault(x => x.Name == ignoreName);
                if (ztype != null)
                {
                    ignoreZoneTypeIds.Add(ztype.IdKey);
                }
            }

            foreach (string detailName in detailTextureNames)
            {
                TextureType detailTexture = textureSettings.GetData().FirstOrDefault(x => x.Name == detailName);

                if (detailTexture != null && !string.IsNullOrEmpty(detailTexture.Art))
                {
                    detailTextureTypes.Add(detailTexture);
                }
            }

            _zoneGenService.InitTerrainSettings(terr, heightMapSize);

            List<CrawlerTerrainIndexData> finalIndexes = new List<CrawlerTerrainIndexData>();

            ZoneType starsZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Name == "Stars");

            allZoneTypes = allZoneTypes.OrderBy(x => x).ToList();

            if (starsZoneType != null)
            {
                allZoneTypes.Insert(0, starsZoneType.IdKey);
            }

            foreach (long zoneTypeId in allZoneTypes)
            {
                ZoneType ztype = zoneSettings.Get(zoneTypeId);
                if (ztype == null)
                {
                    continue;
                }
                long textureTypeId = ztype.BaseTextureTypeId;

                TextureType ttype = textureSettings.Get(textureTypeId);

                CrawlerTerrainIndexData newIndexData = new CrawlerTerrainIndexData()
                {
                    ZoneTypeId = ztype.IdKey,
                    TextureType = ttype,
                };

                finalIndexes.Add(newIndexData);
                _assetService.LoadAssetInto(mapRoot.TerrainObject, AssetCategoryNames.TerrainTex, ttype.Art, OnDownloadTexture, token, newIndexData);

            }


            List<CrawlerTerrainIndexData> detailIndexDataList = new List<CrawlerTerrainIndexData>();
            foreach (TextureType ttype in detailTextureTypes)
            {
                CrawlerTerrainIndexData detailIndexData = new CrawlerTerrainIndexData()
                {
                    ZoneTypeId = -1,
                    TextureType = ttype,
                };
                finalIndexes.Add(detailIndexData);
                detailIndexDataList.Add(detailIndexData);
                _assetService.LoadAssetInto(mapRoot.TerrainObject, AssetCategoryNames.TerrainTex, ttype.Art, OnDownloadTexture, token, detailIndexData);
            }

            while (finalIndexes.Any(x => !x.IsReady))
            {
                await Awaitable.NextFrameAsync(token);
            }

            for (int i = 0; i < finalIndexes.Count; i++)
            {
                finalIndexes[i].AlphaIndex = i;
            }

            List<long> detailIgnoreAlphaIndexes = new List<long>();

            foreach (long zoneTypeId in ignoreZoneTypeIds)
            {
                CrawlerTerrainIndexData ignoreIndexData = finalIndexes.FirstOrDefault(x => x.ZoneTypeId == zoneTypeId);

                if (ignoreIndexData != null)
                {
                    detailIgnoreAlphaIndexes.Add(ignoreIndexData.AlphaIndex);
                }

            }

            long[,] zoneTypeIds = new long[alphamapSize, alphamapSize];

            long[,] seeds = new long[alphamapSize, alphamapSize];

            float[,] terrainHeights = new float[heightMapSize, heightMapSize];

            float[,,] textureAlphas1 = new float[alphamapSize, alphamapSize, finalIndexes.Count];
            float[,,] textureAlphas2 = new float[alphamapSize, alphamapSize, finalIndexes.Count];
            for (int x = 0; x < alphamapSize; x++)
            {
                for (int y = 0; y < alphamapSize; y++)
                {
                    textureAlphas1[x, y, 0] = 1;
                    textureAlphas2[x, y, 0] = 1;
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

                for (int cy = 0; cy < totalMapCellCount; cy++)
                {
                    if (!isLooping)
                    {
                        if (cy < extraViewRadius || cy >= mapHeight + extraViewRadius)
                        {
                            continue;
                        }
                    }

                    int worldY = cy - extraViewRadius;
                    while (worldY < 0)
                    {
                        worldY += mapHeight;
                    }

                    worldY = worldY % mapHeight;

                    long zoneTypeId = mapRoot.Map.Get(worldX, worldY, CellIndex.Terrain);
                    for (int ix = 0; ix < alphaCellsPerWorldBlock; ix++)
                    {
                        int finalX = cx * alphaCellsPerWorldBlock + ix;
                        for (int iy = 0; iy < alphaCellsPerWorldBlock; iy++)
                        {
                            int finalY = cy * alphaCellsPerWorldBlock + iy;
                            zoneTypeIds[finalY, finalX] = zoneTypeId;

                            long seed = world.Seed * 31 + mapRoot.Map.IdKey * 59 +
                                worldX * 37 + worldY * 23 + worldY * ix + worldX * iy +
                                ix * 43 + iy * 61;

                            seeds[finalY, finalX] = seed;
                        }
                    }
                }
            }

            mapRoot.TerrainTextureIndexes = finalIndexes;


            UpdateTerrainLayersFromTextures(mapRoot);


            for (int x = 0; x < alphamapSize; x++)
            {
                for (int y = 0; y < alphamapSize; y++)
                {

                    long zoneTypeId = zoneTypeIds[y, x];

                    if (zoneTypeId < 1)
                    {
                        textureAlphas1[y, x, 0] = 1;
                        continue;
                    }

                    CrawlerTerrainIndexData firstData = finalIndexes.FirstOrDefault(x => x.ZoneTypeId == zoneTypeId);

                    if (firstData != null)
                    {
                        textureAlphas1[y, x, 0] = 0;
                        textureAlphas1[y, x, firstData.AlphaIndex] = 1;
                    }
                }
            }

            // Now adjust alpha edges.

            int shiftToSidePercent = 10;
            int shiftToMiddlePercent = 10;
            int delta = 0;
            for (int x = 1; x < alphamapSize - 1; x++)
            {
                for (int y = 0; y < alphamapSize; y++)
                {
                    if (zoneTypeIds[y, x] != zoneTypeIds[y, x + 1])
                    {
                        long seed = seeds[y, x];
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
                            for (int l = 0; l < mapRoot.TerrainTextureIndexes.Count; l++)
                            {
                                textureAlphas1[y, x, l] = textureAlphas1[y, x + 1, l];
                            }
                        }
                        else if (delta == 1)
                        {
                            for (int l = 0; l < mapRoot.TerrainTextureIndexes.Count; l++)
                            {
                                textureAlphas1[y, x + 1, l] = textureAlphas1[y, x, l];
                            }
                        }
                    }
                }
            }


            for (int x = 1; x < alphamapSize - 1; x++)
            {
                for (int y = 1; y < alphamapSize - 1; y++)
                {
                    for (int l = 0; l < mapRoot.TerrainTextureIndexes.Count; l++)
                    {
                        textureAlphas2[x, y, l] =
                            (textureAlphas1[x, y, l] * 4 +
                            textureAlphas1[x + 1, y, l] +
                            textureAlphas1[x - 1, y, l] +
                            textureAlphas1[x, y + 1, l] +
                            textureAlphas1[x, y - 1, l]) / 8;
                    }
                }
            }

            int detailTimes = 0;
            foreach (CrawlerTerrainIndexData detailIndexData in detailIndexDataList)
            {
                detailTimes++;
                if (detailIndexData.Diffuse == null)
                {
                    continue;
                }

                int detailIndex = detailIndexData.AlphaIndex;
                IRandom rand = new MyRandom(world.Seed / 3 + mapRoot.Map.IdKey * 13 + detailTimes * 19);
                float freq = RandUtils.FloatRange(0.01f, 0.02f, rand) * alphamapSize;
                float amp = RandUtils.FloatRange(1.1f, 1.5f, rand);
                float pers = RandUtils.FloatRange(0.5f, 0.7f, rand);
                int octaves = 2;

                float[,] noiseVals = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), alphamapSize, alphamapSize);

                for (int x = 0; x < alphamapSize; x++)
                {
                    for (int y = 0; y < alphamapSize; y++)
                    {

                        int matchingIgnores = 0;
                        foreach (long alphaIndex in detailIgnoreAlphaIndexes)
                        {
                            if (textureAlphas2[x, y, alphaIndex] > 0)
                            {
                                matchingIgnores++;
                            }
                        }

                        if (matchingIgnores > 0)
                        {
                            continue;
                        }


                        float noiseVal = MathUtil.Clamp(0, noiseVals[x, y], 1);


                        for (int l = 0; l < mapRoot.TerrainTextureIndexes.Count; l++)
                        {
                            textureAlphas2[x, y, l] *= (1 - noiseVal);
                            textureAlphas2[x, y, detailIndex] = noiseVal;
                        }
                    }
                }
            }

            tdata.heightmapResolution = heightMapSize;
            tdata.alphamapResolution = alphamapSize;
            tdata.SetHeights(0, 0, terrainHeights);

            float maxHeight = 100;
            tdata.size = new Vector3(heightMapSize - 1, maxHeight, heightMapSize - 1);

            tdata.SetAlphamaps(0, 0, textureAlphas2);
            terr.Flush();
        }

        public void UpdateTerrainLayersFromTextures(CrawlerMapRoot mapRoot)
        {
            if (mapRoot.GroundTerrain == null || mapRoot.GroundTerrainData == null)
            {
                return;
            }
            int heightCellsPerWorldBlock = mapRoot.XZBlockSize;
            TerrainLayer[] layers = new TerrainLayer[mapRoot.TerrainTextureIndexes.Count];

            for (int l = 0; l < mapRoot.TerrainTextureIndexes.Count; l++)
            {

                TerrainLayer layer = _mapTerrainManager.CreateTerrainLayer(mapRoot.TerrainTextureIndexes[l].Diffuse, mapRoot.TerrainTextureIndexes[l].Normal);

                layer.tileSize = new Vector2(heightCellsPerWorldBlock, heightCellsPerWorldBlock);
                layers[l] = layer;
            }
            mapRoot.GroundTerrainData.terrainLayers = layers;
            mapRoot.GroundTerrain.Flush();
        }


        private void OnDownloadTexture(GameObject go, CrawlerTerrainIndexData index, CancellationToken token)
        {

            TextureList tl = go.GetComponent<TextureList>();
            Texture2D diffuse = null;
            Texture2D normal = null;
            if (tl != null && tl.Textures != null)
            {
                if (tl.Textures.Count > 0)
                {
                    diffuse = tl.Textures[0];

                    if (tl.Textures.Count > 1)
                    {
                        normal = tl.Textures[1];
                    }
                }
            }
            SetIndexTextures(index, diffuse, normal);
        }

        private void SetIndexTextures(CrawlerTerrainIndexData index, Texture2D diffuse, Texture2D normal)
        {

            index.Diffuse = diffuse;
            index.Normal = normal;

            index.IsReady = true;
        }
    }
}