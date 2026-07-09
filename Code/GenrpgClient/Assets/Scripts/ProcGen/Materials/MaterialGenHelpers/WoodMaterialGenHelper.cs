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
            Color oldBackgroundMain = state.ForegroundMain;

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

            float zShiftOutChance = 0.1f;
            float zShiftBackChance = 0.5f;
            int deltaZ = 0;
            int currZ = 0;

            Color lineColor = mainColor;
            for (int z = 0; z < state.Block.Colors.GetLength(1); z++)
            {
                currZ = z;
                deltaZ = 0;
                for (int x = 0; x < state.Block.Colors.GetLength(0); x++)
                {
                    if (deltaZ == 0)
                    {
                        if (state.Rand.NextDouble() < zShiftOutChance)
                        {
                            deltaZ = state.Rand.NextDouble() < 0.5f ? -1 : 1;
                            currZ = (z + deltaZ + state.Block.Colors.GetLength(1)) % state.Block.Colors.GetLength(1);
                        }
                    }
                    else if (state.Rand.NextDouble() < zShiftBackChance)
                    {
                        deltaZ = 0;
                        currZ = z;
                    }

                    state.Block.Colors[x, currZ] = lineColor;
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
            float colorDelta = RandUtils.FloatRange(0.2f, 0.5f, state.Rand);

            state.Settings.ColorNoiseOctaves = 3;

            int rowCount = RandUtils.IntRange(state.Settings.MinBrickRows * 3 / 2, state.Settings.MaxBrickRows * 3 / 2, state.Rand);

            float averageRowHeight = state.Height / rowCount;

            if (averageRowHeight < 4)
            {
                averageRowHeight = 4;
            }

            List<Point2I> blockCenters = new List<Point2I>();

            float heightDelta = 0.2f;

            List<int> rowZValues = new List<int>();

            int startRowZValue = RandUtils.IntRange(0, state.Height - 1, state.Rand);

            float currRowZValue = startRowZValue;

            float zValuesUsed = 0;

            rowZValues.Add(startRowZValue);
            while (true)
            {
                float zValueSkip = averageRowHeight * RandUtils.FloatRange(1, 1 + heightDelta, state.Rand);

                float maxSkip = (state.Height - zValuesUsed) / 2;

                if (zValueSkip > maxSkip)
                {
                    zValueSkip = maxSkip;
                }

                currRowZValue += zValueSkip;

                zValuesUsed += zValueSkip;

                int currentZValueInt = (int)currRowZValue;

                rowZValues.Add(currentZValueInt % state.Height);

                if (currRowZValue - startRowZValue > state.Height - averageRowHeight * 1.5f)
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

            foreach (int rowValue in rowZValues)
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
            state.ForegroundMain = oldBackgroundMain;

            return tex;
        }
    }
}