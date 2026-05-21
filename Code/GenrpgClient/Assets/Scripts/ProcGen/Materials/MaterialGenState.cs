using Assets.Scripts.Dungeons;
using Assets.Scripts.ProcGen.Materials.Constants;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<Color> AllForegroundColors = new List<Color>();
        public List<Color> AllBackgroundColors = new List<Color>();
        public List<Color> AllAccentColors = new List<Color>();
        public Color ForegroundMain = Color.gray;
        public List<ScaledColor> ForegroundNoise = new List<ScaledColor>();

        public Color BackgroundMain = Color.black;
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
            ForegroundNoise = state.ForegroundNoise;
            BackgroundMain = state.BackgroundMain;
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


            List<WeightedMaterialGenType> weightedTypes = args.MaterialsData.GenTypes;

            if (weightedTypes != null)
            {
                double weightSum = weightedTypes.Sum(x => x.Weight);

                double weightChosen = Rand.NextDouble() * weightSum;

                foreach (WeightedMaterialGenType genType in weightedTypes)
                {
                    weightChosen -= genType.Weight;

                    if (weightChosen <= 0)
                    {
                        _genType = genType.WallGenType;
                        break;
                    }
                }
            }

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

        private void SetupColors(MaterialGenSettingsData settings, WallTextureGenArgs args, MaterialGenState prevState)
        {
            if (CopyColorsFromState(prevState))
            {
                return;
            }

            AllForegroundColors = args.MaterialsData.ForegroundColors;
            AllBackgroundColors = args.MaterialsData.BackgroundColors;
            AllAccentColors = args.MaterialsData.AccentColors;

            if (AllForegroundColors.Count > 0)
            {
                List<Color> listCopy = new List<Color>(args.MaterialsData.ForegroundColors);

                int index = Rand.Next() % listCopy.Count;

                ForegroundMain = listCopy[index];

                listCopy.RemoveAt(index);

                int noiseCount = Rand.Next(2, 3);


                float noiseStep = 0.2f;
                float noiseDelta = 0.1f;
                for (int i = 0; i < noiseCount && listCopy.Count > 0; i++)
                {
                    ForegroundNoise.Add(new ScaledColor() { Color = ForegroundMain * RandUtils.DeltaScale(noiseDelta, Rand) });
                }

                for (int i = -2; i <= 2; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    float offset = i * noiseStep + RandUtils.DeltaScale(noiseDelta, Rand);

                    Color nextColor = ForegroundMain * (1 + offset);

                    if (Rand.NextDouble() > 0.7f)
                    {
                        nextColor = listCopy[Rand.Next() % listCopy.Count];
                    }


                    ForegroundNoise.Add(new ScaledColor()
                    {
                        Color = nextColor,
                        EffectThreshold = RandUtils.FloatRange(settings.MinNoiseEffectThreshold, settings.MaxNoiseEffectThreshold, Rand)
                    });
                }
            }
            if (AllBackgroundColors.Count > 0)
            {
                List<Color> potentialBgs = new List<Color>(args.MaterialsData.BackgroundColors);

                if (potentialBgs.Count > 0)
                {
                    float minDistanceToMain = 0.75f;
                    List<Color> finalList = new List<Color>();

                    foreach (Color color in potentialBgs)
                    {
                        Color diffColor = color - ForegroundMain;

                        float size = Math.Abs(diffColor.r) + Math.Abs(diffColor.g) + Math.Abs(diffColor.b);

                        if (size > minDistanceToMain)
                        {
                            finalList.Add(color);
                        }
                    }

                    if (finalList.Count < 1)
                    {
                        finalList = potentialBgs;
                    }

                    int index = Rand.Next() % finalList.Count;

                    BackgroundMain = finalList[index];

                    int bgAccentCount = Rand.Next(1, 2);

                    List<Color> accentCopy = new List<Color>(AllAccentColors);

                    for (int a = 0; a < bgAccentCount && accentCopy.Count > 0; a++)
                    {
                        int aindex = Rand.Next() % accentCopy.Count;

                        BackgroundNoise.Add(new ScaledColor()
                        {
                            Color = accentCopy[aindex],
                            EffectThreshold = RandUtils.FloatRange(settings.MinNoiseEffectThreshold, settings.MaxNoiseEffectThreshold, Rand)
                        });

                        accentCopy.RemoveAt(aindex);
                    }
                }
            }
        }
    }
}
