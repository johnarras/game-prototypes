using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Dungeons;
using Assets.Scripts.ProcGen.Materials.Constants;
using Assets.Scripts.ProcGen.Materials.MaterialGenHelpers;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using System;
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

        public CrawlerMapRoot MapRoot { get; set; }

    }

    public interface IMaterialGenService : IInjectable
    {
        Task<GeneratedWallLooseTextureSet> GenerateTextures(WallTextureGenArgs args);

        Awaitable<Texture2D[]> GenerateMultipleLooseTexturesForOneMaterialIndex(WallTextureGenArgs args, int materialIndex, int repeatTimes);
    }
    public class MaterialGenService : IMaterialGenService
    {
        private IClientAppService _appService = null;
        private ILogService _logService = null;

        private SetupDictionaryContainer<EMaterialGenTypes, IMaterialGenHelper> _materialGenHelpers = new SetupDictionaryContainer<EMaterialGenTypes, IMaterialGenHelper>();

        public async Task<GeneratedWallLooseTextureSet> GenerateTextures(WallTextureGenArgs args)
        {
            GeneratedWallLooseTextureSet set = new GeneratedWallLooseTextureSet();

            if (!_appService.IsPlaying)
            {
                return set;
            }

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
