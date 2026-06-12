using Assets.Scripts.Dungeons;
using Assets.Scripts.ProcGen.Materials.Constants;
using OxDb.SharedCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials
{

    public class ScaledColor
    {
        public Color Color;
        public float EffectThreshold;
    }
    public class MaterialGenState
    {
        public int Width;
        public int Height;
        public IRandom Rand;


        public EMaterialGenTypes GenType => _genType;
        protected EMaterialGenTypes _genType = EMaterialGenTypes.Blocks;
        public MaterialGenSettingsData Settings { get; set; }

        public Color ForegroundMain;
        public List<ScaledColor> ForegroundNoise = new List<ScaledColor>();

        public Color BackgroundMain;
        public List<ScaledColor> BackgroundNoise = new List<ScaledColor>();

        public int MaterialIndex = 0;

        public float CornerPerturbChance { get; set; }
        public float VerticalPerturbChance { get; set; }
        public float MaxCornerPerturbScale { get; set; }

        public float RoundCornerMinSize { get; set; }
        public float RoundCornerMaxSize { get; set; }

        public int BlockRowCount { get; set; }

        public float MaxDistanceToCrevice { get; set; }

        public float RoundCornerDistFreq { get; set; }
        public float RoundCornerDistAmp { get; set; }
        public float RoundCornerDistPers { get; set; }
        public bool UseLargeBlocks { get; set; }

        public float CurvedWallChance = 0.1f;

        public List<CornerPoint> CornerPoints { get; set; } = new List<CornerPoint>();

        public MaterialGenBlock Block { get; set; }


        public bool CopyColorsFromState(MaterialGenState state)
        {
            if (state == null)
            {
                return false;
            }
            ForegroundMain = state.ForegroundMain;
            BackgroundMain = state.BackgroundMain;
            ForegroundNoise = state.ForegroundNoise;
            BackgroundNoise = state.BackgroundNoise;
            return true;
        }


        public void SetupFromArgs(WallTextureGenArgs args, MaterialGenState prevState)
        {
            Width = Settings.TextureSize;
            Height = Settings.TextureSize;

            if (args.MapRoot != null)
            {
                Width *= args.MapRoot.XZBlockSize / args.MapRoot.YBlockSize;
            }


            WeightedMaterialGenType genType = RandUtils.GetRandomElement(args.MaterialsData.GenTypes, Rand);

            _genType = genType.WallGenType;

            if (_genType == EMaterialGenTypes.Default)
            {
                _genType = EMaterialGenTypes.Blocks;
            }

            if (MaterialIndex == DungeonMaterialIndexes.Wood)
            {
                _genType = EMaterialGenTypes.Wood;
            }

            if (Rand.NextDouble() < Settings.CornerPerturbAtAllChance)
            {
                CornerPerturbChance = RandUtils.FloatRange(Settings.MinCornerPerturbChance, Settings.MaxCornerPerturbChance, Rand);
                VerticalPerturbChance = RandUtils.FloatRange(Settings.MinVerticalPerturbChance, Settings.MaxVerticalPerturbChance, Rand);
            }

            RoundCornerMinSize = Width * Settings.RoundCornerMinSizePercent * RandUtils.DeltaScale(Settings.RoundCornerSizeDelta, Rand);
            RoundCornerMaxSize = Width * Settings.RoundCornerMaxSizePercent * RandUtils.DeltaScale(Settings.RoundCornerSizeDelta, Rand);
            MaxCornerPerturbScale = Settings.MaxCornerPerturbScale;

            MaxDistanceToCrevice = Width * RandUtils.FloatRange(Settings.MinDistanceToCrevicePercent, Settings.MaxDistanceToCrevicePercent, Rand);

            CurvedWallChance = RandUtils.FloatRange(Settings.CurvedWallMinChance, Settings.CurvedWallMaxChance, Rand);

            BlockRowCount = RandUtils.IntRange(Settings.MinBrickRows, Settings.MaxBrickRows, Rand);


            RoundCornerDistAmp = RandUtils.FloatRange(Settings.MinRoundCornerDistAmp, Settings.MaxRoundCornerDistAmp, Rand);
            RoundCornerDistFreq = RandUtils.FloatRange(Settings.MinRoundCornerDistFreq, Settings.MaxRoundCornerDistFreq, Rand);
            RoundCornerDistPers = RandUtils.FloatRange(Settings.MinRoundCornerDistPers, Settings.MaxRoundCornerDistPers, Rand);

            SetupColors(Settings, args, prevState);
        }

        private void SetupNoiseColors(Color mainColor, List<ScaledColor> scaledColors, MaterialGenSettingsData settings)
        {


            int noiseCount = Rand.Next(2, 3);

            float noiseDelta = 0.2f;

            for (int i = 0; i < noiseCount; i++)
            {
                scaledColors.Add(new ScaledColor()
                {
                    Color = mainColor * RandUtils.DeltaScale(noiseDelta, Rand),
                    EffectThreshold = RandUtils.FloatRange(settings.MinNoiseEffectThreshold, settings.MaxNoiseEffectThreshold, Rand)
                });
            }
        }

        private void SetupColors(MaterialGenSettingsData settings, WallTextureGenArgs args, MaterialGenState prevState)
        {
            if (CopyColorsFromState(prevState))
            {
                return;
            }

            ColorSet Colors = RandUtils.GetRandomElement(args.MaterialsData.ColorSets, Rand);
            ForegroundMain = Colors.Foreground;
            BackgroundMain = Colors.Background;

            SetupNoiseColors(Colors.Foreground, ForegroundNoise, settings);
            SetupNoiseColors(Colors.Background, BackgroundNoise, settings);

        }
    }
}
