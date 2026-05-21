using Assets.Scripts.ProcGen.Materials.Constants;
using OxDb.SharedCore.LineGen;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials.MaterialGenHelpers
{
    public class WoodMaterialGenHelper : BaseMaterialGenHelper
    {
        public override EMaterialGenTypes HelperKey => EMaterialGenTypes.Wood;

        public override async Awaitable<Texture2D> GenerateTexture(MaterialGenState state)
        {
            await Task.CompletedTask;

            Color oldForegroundMain = state.ForegroundMain;
            Color oldBackgroundMain = state.BackgroundMain;

            state.ForegroundMain = new Color(0.5f, 0.25f, 0, 1) * RandUtils.DeltaScale(0.2f, state.Rand);
            state.ForegroundNoise = new List<ScaledColor>();
            state.BackgroundMain = Color.black;
            state.BackgroundNoise = new List<ScaledColor>();
            Texture2D tex = CreateTexture(state.Width, state.Height);
            state.Block = new MaterialGenBlock(state.Width, state.Height, state.ForegroundMain, MaterialGenConstants.DefaultStartBrightness, MaterialGenConstants.DefaultStartBumpHeight);

            Color mainColor = state.ForegroundMain;
            // Now set the wood grain random colors.


            int minChangePixels = 0;
            int maxChangePixels = (state.Width + state.Height) / 4;

            int currColorPixelsLeft = RandUtils.IntRange(minChangePixels, maxChangePixels, state.Rand);

            float minLineDeltaScale = 0.1f;
            float maxLineDeltaScale = 0.4f;

            float regularColorChance = 0.70f;

            float yShiftOutChance = 0.1f;
            float yShiftBackChance = 0.5f;
            int deltaY = 0;
            int currY = 0;

            Color lineColor = mainColor;
            for (int y = 0; y < state.Block.Colors.GetLength(1); y++)
            {
                currY = y;
                deltaY = 0;
                for (int x = 0; x < state.Block.Colors.GetLength(0); x++)
                {
                    if (deltaY == 0)
                    {
                        if (state.Rand.NextDouble() < yShiftOutChance)
                        {
                            deltaY = state.Rand.NextDouble() < 0.5f ? -1 : 1;
                            currY = (y + deltaY + state.Block.Colors.GetLength(1)) % state.Block.Colors.GetLength(1);
                        }
                    }
                    else if (state.Rand.NextDouble() < yShiftBackChance)
                    {
                        deltaY = 0;
                        currY = y;
                    }

                    state.Block.Colors[x, currY] = lineColor;
                    currColorPixelsLeft--;
                    if (currColorPixelsLeft < 1)
                    {
                        float delta = RandUtils.FloatRange(minLineDeltaScale, maxLineDeltaScale, state.Rand);
                        lineColor = (mainColor * RandUtils.DeltaRange(delta, state.Rand));

                        if (state.Rand.NextDouble() < regularColorChance)
                        {
                            lineColor = mainColor;
                        }
                        currColorPixelsLeft = RandUtils.IntRange(minChangePixels, maxChangePixels, state.Rand);
                    }
                }
            }

            float colorDelta = RandUtils.FloatRange(0.1f, 0.4f, state.Rand);

            int noiseCount = 2 + state.Rand.Next() % 2;

            for (int i = 0; i < noiseCount; i++)
            {

                state.ForegroundNoise.Add(new ScaledColor()

                {
                    Color = (mainColor * RandUtils.DeltaScale(colorDelta, state.Rand)),
                    EffectThreshold = RandUtils.FloatRange(state.Settings.MinNoiseEffectThreshold, state.Settings.MaxNoiseEffectThreshold, state.Rand),
                }
                );

            }

            state.Settings.ColorNoiseOctaves = 3;

            int rowCount = RandUtils.IntRange(state.Settings.MinBrickRows * 3 / 2, state.Settings.MaxBrickRows * 3 / 2, state.Rand);

            float averageRowHeight = state.Height / rowCount;

            if (averageRowHeight < 4)
            {
                averageRowHeight = 4;
            }

            List<PointXZ> blockCenters = new List<PointXZ>();

            float heightDelta = 0.2f;

            List<int> rowYValues = new List<int>();

            int startRowYValue = RandUtils.IntRange(0, state.Height - 1, state.Rand);

            float currRowYValue = startRowYValue;

            float yValuesUsed = 0;

            rowYValues.Add(startRowYValue);
            while (true)
            {
                float yValueSkip = averageRowHeight * RandUtils.FloatRange(1, 1 + heightDelta, state.Rand);

                float maxSkip = (state.Height - yValuesUsed) / 2;

                if (yValueSkip > maxSkip)
                {
                    yValueSkip = maxSkip;
                }

                currRowYValue += yValueSkip;

                yValuesUsed += yValueSkip;

                int currYValueInt = (int)currRowYValue;

                rowYValues.Add(currYValueInt % state.Height);

                if (currRowYValue - startRowYValue > state.Height - averageRowHeight * 1.5f)
                {
                    break;
                }
            }

            List<CornerPoint> points = new List<CornerPoint>();


            LineGenParameters lineGenParams = new LineGenParameters()
            {
                WidthSize = state.Rand.Next(2, 5),
                WidthSizeChangeAmount = 0,
                WidthPosShiftChance = 0.00f,
                WidthPosShiftSize = 1,
                MaxWidthPosDrift = 0,
                LinePathNoiseScale = 0.0f,
                InitialNoPosShiftLength = 0,
                MaxWidthSize = 12,
                MinWidthSize = 2,
                Seed = state.Rand.Next(),
                WidthSizeChangeChance = 0.03f
            };



            state.CurvedWallChance = 0;

            foreach (int rowValue in rowYValues)
            {
                CornerPoint cp1 = new CornerPoint(0, rowValue);
                CornerPoint cp2 = new CornerPoint(state.Width - 1, rowValue);

                _materialGenUtilsService.ConnectLowerLeftToUpperRightPoint(state, cp1, cp2, 0, 2, 2);

            }

            _materialGenUtilsService.AddColorNoise(state);

            _materialGenUtilsService.ApplyRecessedColors(state);

            _materialGenUtilsService.SmoothColors(state);

            _materialGenUtilsService.ApplyBlockToTexture(state, state.Block, tex);

            state.ForegroundMain = oldForegroundMain;
            state.BackgroundMain = oldBackgroundMain;

            return tex;
        }
    }
}