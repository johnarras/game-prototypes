using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Dungeons;
using Assets.Scripts.ProcGen.Materials.Constants;
using Assets.Scripts.ProcGen.Materials.MaterialGenHelpers;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials
{

    /// <summary>
    /// The word Loose is here so you know that these things are not tracked anywhere else, and you need to do that
    /// if you use this system to avoid memory leaks.
    /// </summary>
    public class GeneratedWallLooseTextureSet
    {
        public Texture2D[] Textures { get; set; } = new Texture2D[DungeonMaterialIndexes.Max];
    }

    public class WallTextureGenArgs
    {
        public long Seed { get; set; }

        public MaterialGenData MaterialsData { get; set; }

        public long ZoneTypeId { get; set; }

    }

    public interface IMaterialGenService : IInjectable
    {
        Task<GeneratedWallLooseTextureSet> GenerateTextures(WallTextureGenArgs args);

        Awaitable GenerateRandomMaterialsInCrawler(CancellationToken token);

        Awaitable<Texture2D[]> GenerateMultipleLooseTexturesForOneMaterialIndex(WallTextureGenArgs args, int materialIndex, int repeatTimes);
    }
    public class MaterialGenService : IMaterialGenService
    {
        private IClientAppService _appService = null;
        private ILogService _logService = null;
        private ICrawlerMapService _mapService = null;
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IAssetService _assetService = null;
        private ICrawlerTerrainService _crawlerTerrainService = null;

        private SetupDictionaryContainer<EMaterialGenTypes, IMaterialGenHelper> _materialGenHelpers = new SetupDictionaryContainer<EMaterialGenTypes, IMaterialGenHelper>();

        public async Awaitable GenerateRandomMaterialsInCrawler(CancellationToken token)
        {
            WallTextureGenArgs args = new WallTextureGenArgs()
            {
                Seed = new MyRandom(DateTime.UtcNow.Ticks).Next(),
            };

            CrawlerMapRoot mapRoot = _mapService.GetMapRoot();

            CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);
            CrawlerMapType mtype = mapSettings.Get(CrawlerMapTypes.Dungeon);

            List<long> dungeonZoneTypes = new List<long>();

            foreach (CrawlerMapGenType genType in mtype.GenTypes)
            {
                dungeonZoneTypes.AddRange(genType.WeightedZones.Select(x => x.ZoneTypeId));
            }

            dungeonZoneTypes = dungeonZoneTypes.Distinct().ToList();
            long zoneTypeId = dungeonZoneTypes[(int)(args.Seed % dungeonZoneTypes.Count)];

            ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);

            string artName = zoneType.Art;

            args.MaterialsData = await _assetService.LoadAssetAsync<MaterialGenData>(AssetCategoryNames.Dungeons,
                artName + CrawlerMapService.MaterialGenDataFilenameSuffix, mapRoot, token);

            args.ZoneTypeId = zoneTypeId;

            List<NameValue> indexNames = ConstantUtils.GetNumericConstants(typeof(DungeonMaterialIndexes));

            GeneratedWallLooseTextureSet set = await GenerateTextures(args);

            for (int t = 0; t < set.Textures.Length; t++)
            {
                if (set.Textures[t] == null)
                {
                    continue;
                }

                NameValue indexName = indexNames.FirstOrDefault(x => x.IdKey == t);

                string texName = "Tex-" + t + "-";

                if (indexName != null)
                {
                    texName = indexName.Name;
                }

                foreach (MaterialBlock mblock in mapRoot.MaterialBlocks.Values)
                {
                    FinalDungeonMaterials dungeonMat = mblock.FinalMaterials;

                    List<MaterialOption> options = dungeonMat.GetMaterials(t);

                    foreach (MaterialOption opt in options)
                    {
                        if (opt.Mat != null)
                        {
                            opt.Mat.mainTexture = set.Textures[t];
                        }
                    }
                }

            }
            if (mapRoot.GroundTerrain != null && mapRoot.GroundTerrainData != null)
            {
                Texture2D floor = set.Textures[DungeonMaterialIndexes.Floors];
                foreach (CrawlerTerrainIndexData indexData in mapRoot.TerrainTextureIndexes)
                {
                    indexData.Diffuse = floor;
                }
                _crawlerTerrainService.UpdateTerrainLayersFromTextures(mapRoot);
            }
        }

        public async Task<GeneratedWallLooseTextureSet> GenerateTextures(WallTextureGenArgs args)
        {
            GeneratedWallLooseTextureSet set = new GeneratedWallLooseTextureSet();

            if (!_appService.IsPlaying)
            {
                return set;
            }


            int repeatTimes = 1;


            MaterialGenState prevState = null;
            for (int materialIndex = 0; materialIndex < DungeonMaterialIndexes.Max; materialIndex++)
            {
                MaterialGenSettingsData settings = ScriptableObjectUtils.LoadDefault<MaterialGenSettingsData>();

                MaterialGenState state = new MaterialGenState()
                {
                    Rand = new MyRandom(args.Seed + materialIndex),
                    MaterialIndex = materialIndex,
                    Settings = settings,
                };

                state.SetupFromArgs(args, prevState);

                set.Textures[materialIndex] = await GenerateTexture(state);

                prevState = state;
            }
            return set;
        }

        public async Awaitable<Texture2D[]> GenerateMultipleLooseTexturesForOneMaterialIndex(WallTextureGenArgs args, int assetIndex, int repeatTimes)
        {
            Texture2D[] retval = new Texture2D[repeatTimes];

            MaterialGenState prevState = null;
            for (int i = 0; i < repeatTimes; i++)
            {
                MaterialGenSettingsData settings = ScriptableObjectUtils.LoadDefault<MaterialGenSettingsData>();

                MaterialGenState state = new MaterialGenState()
                {
                    Rand = new MyRandom(args.Seed + assetIndex + repeatTimes),
                    MaterialIndex = assetIndex,
                    Settings = settings,
                };

                state.SetupFromArgs(args, prevState);

                retval[i] = await GenerateTexture(state);

                prevState = state;
            }
            return retval;
        }


        private async Awaitable<Texture2D> GenerateTexture(MaterialGenState state)
        {
            EMaterialGenTypes wallGenType = state.GenType;

            if (state.MaterialIndex == DungeonMaterialIndexes.Wood)
            {
                wallGenType = EMaterialGenTypes.Wood;
            }
            else if (state.MaterialIndex == DungeonMaterialIndexes.Floors)
            {
                wallGenType = EMaterialGenTypes.FlatPlane;
            }
            else
            {
                // Need better logic here for the given asset index.
                if (wallGenType == EMaterialGenTypes.Default)
                {
                    wallGenType = EMaterialGenTypes.Blocks;
                }
            }

            try
            {
                if (_materialGenHelpers.TryGetValue(wallGenType, out IMaterialGenHelper helper))
                {
                    Texture2D tex = await helper.GenerateTexture(state);
                    if (tex != null)
                    {
                        tex.name = "Texture" + state.MaterialIndex;
                    }
                    return tex;
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "WallGenTexture");
            }
            return null;
        }
    }
}
