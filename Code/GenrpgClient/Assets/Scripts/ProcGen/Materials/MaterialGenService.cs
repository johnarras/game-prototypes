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
using System.Collections.Generic;
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
        public Color ForegroundColor;
        public Color BackgroundColor;
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

        Texture2D CreateAlphaNormalMapFromTexture(Texture2D diffuse, bool invertGrayscale, float strength = 1.0f);

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

                SetupFromArgs(state, args, prevState);

                Texture2D diffuseMap = await GenerateTexture(state);

                set.DiffuseTextures[materialIndex] = diffuseMap;
                set.ForegroundColor = state.ForegroundMain;
                set.BackgroundColor = state.BackgroundMain;

                float frontGrayscale = state.ForegroundMain.grayscale;
                float backGrayscale = state.BackgroundMain.grayscale;

                set.NormalTextures[materialIndex] = CreateAlphaNormalMapFromTexture(diffuseMap, frontGrayscale < backGrayscale);
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
                    Rand = new MyRandom(args.Seed + assetIndex + i),
                    MaterialIndex = assetIndex,
                    Settings = settings,
                };

                SetupFromArgs(state, args, prevState);

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

        /// <summary>
        /// We set the bumpmaps to create alphas (texture is opaque) so we use those alphas here to make a normal map.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="invertGrayscale"></param>
        /// <param name="strength"></param>
        /// <returns></returns>
        public Texture2D CreateAlphaNormalMapFromTexture(Texture2D source, bool invertGrayscale, float strength = 1.0f)
        {
            // Ensure the strength is clamped to a reasonable positive value
            strength = Mathf.Max(0.0f, strength);

            int width = source.width;
            int height = source.height;

            // Allocate the new texture
            Texture2D normalMap = new Texture2D(width, height, TextureFormat.RGBA32, true);
            normalMap.filterMode = FilterMode.Bilinear;
            Color[] sourcePixels = source.GetPixels();
            Color[] normalPixels = new Color[sourcePixels.Length];

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Sample neighboring pixels for Sobel-like filtering (wrapping edges)
                    float xLeft = sourcePixels[SampleIndex(x - 1, z, width, height)].a;
                    float xRight = sourcePixels[SampleIndex(x + 1, z, width, height)].a;
                    float zUp = sourcePixels[SampleIndex(x, z + 1, width, height)].a;
                    float zDown = sourcePixels[SampleIndex(x, z - 1, width, height)].a;

                    // Calculate horizontal and vertical gradients
                    float xGrad = (xLeft - xRight) * strength;
                    float zGrad = (zDown - zUp) * strength;

                    // Construct the surface normal vector
                    Vector3 normal = new Vector3(xGrad, zGrad, 1.0f).normalized;

                    // Map the Vector3 components from [-1, 1] to the RGB color space [0, 1]
                    Color pixelColor = new Color(
                        (normal.x * 0.5f) + 0.5f,
                        (normal.y * 0.5f) + 0.5f,
                        (normal.z * 0.5f) + 0.5f,
                        1.0f
                    );

                    normalPixels[z * width + x] = pixelColor;
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
        private int SampleIndex(int x, int z, int width, int height)
        {
            x = (x + width) % width;
            z = (z + height) % height;
            return z * width + x;
        }

        public void SetupFromArgs(MaterialGenState state, WallTextureGenArgs args, MaterialGenState prevState)
        {
            state.Width = state.Settings.TextureSize;
            state.Height = state.Settings.TextureSize;

            if (args.MapRoot != null)
            {
                state.Width *= args.MapRoot.XZBlockSize / args.MapRoot.YBlockSize;
            }


            WeightedMaterialGenType genType = RandUtils.GetRandomElement(args.MaterialsData.GenTypes, state.Rand);

            state.GenType = genType.WallGenType;

            if (state.GenType == EMaterialGenTypes.Default)
            {
                state.GenType = EMaterialGenTypes.Blocks;
            }

            if (state.MaterialIndex == DungeonMaterialIndexes.Wood)
            {
                state.GenType = EMaterialGenTypes.Wood;
            }

            if (state.Rand.NextDouble() < state.Settings.CornerPerturbAtAllChance)
            {
                state.CornerPerturbChance = RandUtils.FloatRange(state.Settings.MinCornerPerturbChance, state.Settings.MaxCornerPerturbChance, state.Rand);
                state.VerticalPerturbChance = RandUtils.FloatRange(state.Settings.MinVerticalPerturbChance, state.Settings.MaxVerticalPerturbChance, state.Rand);
            }

            state.RoundCornerMinSize = state.Width * state.Settings.RoundCornerMinSizePercent * RandUtils.DeltaScale(state.Settings.RoundCornerSizeDelta, state.Rand);
            state.RoundCornerMaxSize = state.Width * state.Settings.RoundCornerMaxSizePercent * RandUtils.DeltaScale(state.Settings.RoundCornerSizeDelta, state.Rand);
            state.MaxCornerPerturbScale = state.Settings.MaxCornerPerturbScale;

            state.MaxDistanceToCrevice = state.Width * RandUtils.FloatRange(state.Settings.MinDistanceToCrevicePercent, state.Settings.MaxDistanceToCrevicePercent, state.Rand);

            state.CurvedWallChance = RandUtils.FloatRange(state.Settings.CurvedWallMinChance, state.Settings.CurvedWallMaxChance, state.Rand);

            state.BlockRowCount = RandUtils.IntRange(state.Settings.MinBrickRows, state.Settings.MaxBrickRows, state.Rand);


            state.RoundCornerDistAmp = RandUtils.FloatRange(state.Settings.MinRoundCornerDistAmp, state.Settings.MaxRoundCornerDistAmp, state.Rand);
            state.RoundCornerDistFreq = RandUtils.FloatRange(state.Settings.MinRoundCornerDistFreq, state.Settings.MaxRoundCornerDistFreq, state.Rand);
            state.RoundCornerDistPers = RandUtils.FloatRange(state.Settings.MinRoundCornerDistPers, state.Settings.MaxRoundCornerDistPers, state.Rand);

            SetupColors(state, state.Settings, args, prevState);
        }

        private void SetupNoiseColors(MaterialGenState state, Color mainColor, List<ScaledColor> scaledColors, MaterialGenSettingsData settings, bool allowMultiple)
        {
            int noiseCount = RandUtils.IntRange(settings.MinColorNoiseCount, settings.MaxColorNoiseCount, state.Rand);

            if (!allowMultiple)
            {
                noiseCount = 1;
            }

            float noiseDelta = settings.MaxColorNoiseDelta;

            float grayscale = mainColor.grayscale;

            Color baseGrayColor = Color.white * grayscale;

            int grayscaleCount = 0;
            for (int i = 0; i < noiseCount; i++)
            {
                Color currColor = mainColor;

                if (state.Rand.NextDouble() < settings.ColorNoiseGrayscaleChance && grayscaleCount < settings.MaxGrayscaleColorNoise)
                {
                    currColor = baseGrayColor;
                    grayscaleCount++;
                }

                scaledColors.Add(new ScaledColor()
                {
                    Color = currColor * RandUtils.DeltaScale(noiseDelta, state.Rand),
                    EffectThreshold = RandUtils.FloatRange(settings.MinNoiseEffectThreshold, settings.MaxNoiseEffectThreshold, state.Rand)
                });
            }
        }

        private void SetupColors(MaterialGenState state, MaterialGenSettingsData settings, WallTextureGenArgs args, MaterialGenState prevState)
        {
            ColorSet Colors = RandUtils.GetRandomElement(args.MaterialsData.ColorSets, state.Rand);
            state.ForegroundMain = Colors.Foreground;
            state.BackgroundMain = Colors.Background;

            SetupNoiseColors(state, Colors.Foreground, state.ForegroundNoise, settings, true);

            SetupNoiseColors(state, Colors.Background, state.BackgroundNoise, settings, false);

        }
    }
}

