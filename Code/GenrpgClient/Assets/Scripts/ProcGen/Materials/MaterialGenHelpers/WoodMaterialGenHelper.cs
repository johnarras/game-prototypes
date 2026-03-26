using Assets.Scripts.ProcGen.Materials.Constants;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.ProcGen.Settings.LineGen;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

namespace Assets.Scripts.ProcGen.Materials.MaterialGenHelpers
{
    public class WoodMaterialGenHelper : BaseMaterialGenHelper
    {
        public override EMaterialGenTypes HelperKey => EMaterialGenTypes.Wood;

        public override async Awaitable<Texture2D> GenerateTexture(MaterialGenState state)
        {
            state.ForegroundMain = new Color(0.5f, 0.25f, 0, 1) * RandUtils.DeltaScale(0.2f, state.Rand);
            state.ForegroundNoise = new List<ScaledColor>();
            state.BackgroundMain = Color.black;
            state.BackgroundNoise = new List<ScaledColor>();
            Texture2D tex = new Texture2D(state.Size, state.Size, TextureFormat.RGBAFloat, false);
            state.Block = new MaterialGenBlock(state.Size, state.ForegroundMain, MaterialGenConstants.DefaultStartBrightness, MaterialGenConstants.DefaultStartBumpHeight);


            float colorDelta = RandUtils.FloatRange(0.5f, 1.0f, state.Rand);

            int noiseCount = 2 + state.Rand.Next() % 2;

            Color mainColor = state.ForegroundMain;
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

            float averageRowHeight = state.Size / rowCount;

            if (averageRowHeight < 4)
            {
                averageRowHeight = 4;
            }

            List<PointXZ> blockCenters = new List<PointXZ>();

            float heightDelta = 0.2f;

            List<int> rowYValues = new List<int>();

            int startRowYValue = RandUtils.IntRange(0, state.Size - 1, state.Rand);

            float currRowYValue = startRowYValue;

            float yValuesUsed = 0;

            rowYValues.Add(startRowYValue);
            while (true)
            {
                float yValueSkip = averageRowHeight * RandUtils.FloatRange(1, 1 + heightDelta, state.Rand);

                float maxSkip = (state.Size - yValuesUsed) / 2;

                if (yValueSkip > maxSkip)
                {
                    yValueSkip = maxSkip;
                }

                currRowYValue += yValueSkip;

                yValuesUsed += yValueSkip;

                int currYValueInt = (int)currRowYValue;

                rowYValues.Add(currYValueInt % state.Size);

                if (currRowYValue - startRowYValue > state.Size - averageRowHeight * 1.5f)
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
                CornerPoint cp2 = new CornerPoint(state.Size - 1, rowValue);

                _materialGenUtilsService.ConnectLowerLeftToUpperRightPoint(state, cp1, cp2, 0, 2, 2);

            }

            _materialGenUtilsService.AddColorNoise(state);

            _materialGenUtilsService.ApplyRecessedColors(state);

            _materialGenUtilsService.SmoothColors(state);

            _materialGenUtilsService.ApplyBlockToTexture(state, state.Block, tex);

            return tex;
        }
    }
}