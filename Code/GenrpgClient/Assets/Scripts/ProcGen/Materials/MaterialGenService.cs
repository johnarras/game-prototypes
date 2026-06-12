using Assets.Scripts.Assets.Materials;
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
        public Texture2D[] DiffuseTextures { get; set; } = new Texture2D[DungeonMaterialIndexes.Max];
        public Texture2D[] NormalTextures { get; set; } = new Texture2D[DungeonMaterialIndexes.Max];
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

        Texture2D CreateGrayscaleNormalMapFromDiffuseTexture(Texture2D diffuse, bool invertGrayscale, float strength = 1.0f);

        void SetNormalMap(Material mat, Texture2D normalMap);
    }
    public class MaterialGenService : IMaterialGenService
    {
        private IClientAppService _appService = null;
        private ILogService _logService = null;

        private SetupDictionaryContainer<EMaterialGenTypes, IMaterialGenHelper> _materialGenHelpers = new SetupDictionaryContainer<EMaterialGenTypes, IMaterialGenHelper>();

        public void SetNormalMap(Material mat, Texture2D normalMap)
        {

            MaterialGenSettingsData settings = ScriptableObjectUtils.LoadDefault<MaterialGenSettingsData>();


            mat.SetFloat(MaterialUtils.BumpScalePropertyName, settings.BumpScale);
            mat.SetFloat(MaterialUtils.SmoothnessPropertyName, settings.SmoothnessScale);
            mat.SetColor(MaterialUtils.SpecularColorPropertyName, settings.SpecularColor);
            mat.SetTexture(MaterialUtils.NormalMapPropertyName, normalMap);


            mat.EnableKeyword(MaterialUtils.EnableNormalMapKeyword);
        }

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

                Texture2D diffuseMap = await GenerateTexture(state);

                set.DiffuseTextures[materialIndex] = diffuseMap;

                float frontGrayscale = state.ForegroundMain.grayscale;
                float backGrayscale = state.BackgroundMain.grayscale;

                set.NormalTextures[materialIndex] = CreateGrayscaleNormalMapFromDiffuseTexture(diffuseMap, frontGrayscale < backGrayscale);
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

        public Texture2D CreateGrayscaleNormalMapFromDiffuseTexture(Texture2D source, bool invertGrayscale, float strength = 1.0f)
        {
            // Ensure the strength is clamped to a reasonable positive value
            strength = Mathf.Max(0.0f, strength);

            int width = source.width;
            int height = source.height;

            // Allocate the new texture
            Texture2D normalMap = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color[] sourcePixels = source.GetPixels();
            Color[] normalPixels = new Color[sourcePixels.Length];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Sample neighboring pixels for Sobel-like filtering (wrapping edges)
                    float xLeft = sourcePixels[SampleIndex(x - 1, y, width, height)].grayscale;
                    float xRight = sourcePixels[SampleIndex(x + 1, y, width, height)].grayscale;
                    float yUp = sourcePixels[SampleIndex(x, y + 1, width, height)].grayscale;
                    float yDown = sourcePixels[SampleIndex(x, y - 1, width, height)].grayscale;

                    // Calculate horizontal and vertical gradients
                    float xGrad = (xLeft - xRight) * strength;
                    float yGrad = (yDown - yUp) * strength;

                    // Construct the surface normal vector
                    Vector3 normal = new Vector3(xGrad, yGrad, 1.0f).normalized;

                    // Map the Vector3 components from [-1, 1] to the RGB color space [0, 1]
                    Color pixelColor = new Color(
                        (normal.x * 0.5f) + 0.5f,
                        (normal.y * 0.5f) + 0.5f,
                        (normal.z * 0.5f) + 0.5f,
                        1.0f
                    );

                    normalPixels[y * width + x] = pixelColor;
                }
            }

            // Apply the pixel data to the new texture
            normalMap.SetPixels(normalPixels);
            normalMap.Apply();

            return normalMap;
        }

        /// <summary>
        /// Helper to sample pixel arrays with edge wrapping (tiling)
        /// </summary>
        private int SampleIndex(int x, int y, int width, int height)
        {
            x = (x + width) % width;
            y = (y + height) % height;
            return y * width + x;
        }
    }
}

