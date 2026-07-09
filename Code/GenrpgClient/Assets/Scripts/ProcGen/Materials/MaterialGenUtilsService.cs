using Assets.Scripts.Assets.Utils;
using Assets.Scripts.ProcGen.Materials.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.LineGen;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials
{
    public interface IMaterialGenUtilsService : IInjectable
    {
        bool TryVerticalPerturb(MaterialGenState state, List<CornerPoint> corners, CornerPoint thisCorner, double maxPerturb);
        void RoundEdgesNearCrevices(MaterialGenState state);
        List<LineCell> ConnectLowerLeftToUpperRightPoint(MaterialGenState state, CornerPoint lowerLeft, CornerPoint upperRight, float bumpHeight, int minWidth, int maxWidth);

        void RoundCreviceCorner(MaterialGenState state, List<CornerPoint> corners, CornerPoint currentCorner, int lowAnglePoint, int highAnglePoint);

        void AddColorNoise(MaterialGenState state);
        void ApplyBlockToTexture(MaterialGenState state, MaterialGenBlock block, Texture2D tex);

        void ApplyRecessedColors(MaterialGenState state);

        void RandomizeBumpHeights(MaterialGenState state);

        void SmoothColors(MaterialGenState state);

        bool ChangeBrightnessInForegroundRegion(MaterialGenState state, TextureBlock block);

        bool MakeBlockVeryRound(MaterialGenState state, TextureBlock block);

        bool RemoveBlockFromWall(MaterialGenState state, TextureBlock block);


        void AddCracksToFrontRegions(MaterialGenState state);

        void SetupColorBlockRegion(MaterialGenState state, TextureBlock block);

    }
    public class MaterialGenUtilsService : IMaterialGenUtilsService
    {
        private ILineGenService _lineGenService = null;
        private INoiseService _noiseService = null;

        public bool TryVerticalPerturb(MaterialGenState state, List<CornerPoint> corners, CornerPoint thisCorner, double maxPerturb)
        {

            List<CornerPoint> allCornersInvolved = new List<CornerPoint>();

            allCornersInvolved.Add(thisCorner);

            CornerPoint leftCorner = corners.FirstOrDefault(x => x.Index == thisCorner.LeftIndex);

            if (leftCorner == null)
            {
                return false;
            }

            CornerPoint rightCorner = corners.FirstOrDefault(x => x.Index == thisCorner.RightIndex);

            if (rightCorner == null)
            {
                return false;
            }

            CornerPoint secondLeftCorner = corners.FirstOrDefault(x => x.Index == leftCorner.LeftIndex);

            if (secondLeftCorner == null)
            {
                return false;
            }

            CornerPoint secondRightCorner = corners.FirstOrDefault(x => x.Index == rightCorner.RightIndex);

            if (secondRightCorner == null)
            {
                return false;
            }

            allCornersInvolved.Add(secondLeftCorner);
            allCornersInvolved.Add(secondRightCorner);

            allCornersInvolved.Add(leftCorner);
            allCornersInvolved.Add(rightCorner);

            CornerPoint centerUpCorner = corners.FirstOrDefault(x => x.Index == thisCorner.UpIndex);

            CornerPoint centerDownCorner = corners.FirstOrDefault(x => x.Index == thisCorner.DownIndex);


            if (centerUpCorner != null && centerDownCorner != null)
            {
                return false;
            }

            CornerPoint leftOppCorner = null;
            CornerPoint rightOppCorner = null;

            // I have an up corner, so L and r need to have down corners.
            if (centerUpCorner != null)
            {
                allCornersInvolved.Add(centerUpCorner);

                leftOppCorner = corners.FirstOrDefault(x => x.Index == leftCorner.DownIndex);
                rightOppCorner = corners.FirstOrDefault(x => x.Index == rightCorner.DownIndex);
            }
            else if (centerDownCorner != null)
            {
                allCornersInvolved.Add(centerDownCorner);

                leftOppCorner = corners.FirstOrDefault(x => x.Index == leftCorner.UpIndex);
                rightOppCorner = corners.FirstOrDefault(x => x.Index == rightCorner.UpIndex);
            }

            if (leftOppCorner == null || rightOppCorner == null)
            {
                return false;
            }

            allCornersInvolved.Add(leftOppCorner);
            allCornersInvolved.Add(rightOppCorner);

            if (allCornersInvolved.Any(x => x.WasPerturbed))
            {
                return false;
            }


            float dz = RandUtils.FloatRange(maxPerturb / 2, maxPerturb, state.Rand);
            if (centerDownCorner != null)
            {
                dz = -dz;
            }


            CornerPoint newLeftCorner = new CornerPoint(leftCorner.X, leftCorner.Z)
            {
                Index = corners.Max(x => x.Index) + 1,
                LeftIndex = leftCorner.LeftIndex,
                ReplacesIndex = leftCorner.Index,
                IsLeftReplace = true
            };
            corners.Add(newLeftCorner);
            CornerPoint newRightCorner = new CornerPoint(rightCorner.X, rightCorner.Z)
            {
                Index = corners.Max(x => x.Index) + 1,
                RightIndex = rightCorner.RightIndex,
                ReplacesIndex = rightCorner.Index,
                IsLeftReplace = false
            };
            corners.Add(newRightCorner);

            allCornersInvolved.Add(newLeftCorner);
            allCornersInvolved.Add(newRightCorner);
            secondRightCorner.LeftIndex = newRightCorner.Index;
            secondLeftCorner.RightIndex = newLeftCorner.Index;
            rightCorner.RightIndex = 0;
            leftCorner.LeftIndex = 0;

            // Move the things up
            if (centerUpCorner != null)
            {
                newLeftCorner.IsUpReplace = true;
                newLeftCorner.UpIndex = leftCorner.Index;
                newLeftCorner.DownIndex = leftOppCorner.Index;
                leftCorner.DownIndex = newLeftCorner.Index;
                leftCorner.UpIndex = 0;
                leftOppCorner.UpIndex = newLeftCorner.Index;

                newRightCorner.IsUpReplace = true;
                newRightCorner.UpIndex = rightCorner.Index;
                newRightCorner.DownIndex = rightOppCorner.Index;
                rightCorner.DownIndex = newRightCorner.Index;
                rightCorner.UpIndex = 0;
                rightOppCorner.UpIndex = newRightCorner.Index;
            }
            else if (centerDownCorner != null)
            {
                newLeftCorner.IsUpReplace = false;
                newLeftCorner.DownIndex = leftCorner.Index;
                newLeftCorner.UpIndex = leftOppCorner.Index;
                leftCorner.UpIndex = newLeftCorner.Index;
                leftCorner.DownIndex = 0;
                leftOppCorner.DownIndex = newLeftCorner.Index;

                newRightCorner.IsUpReplace = false;
                newRightCorner.DownIndex = rightCorner.Index;
                newRightCorner.UpIndex = rightOppCorner.Index;
                rightCorner.UpIndex = newRightCorner.Index;
                rightCorner.DownIndex = 0;
                rightOppCorner.DownIndex = newRightCorner.Index;
            }

            thisCorner.WasPerturbed = true;
            leftCorner.WasPerturbed = true;
            rightCorner.WasPerturbed = true;

            thisCorner.Z += (int)dz;
            leftCorner.Z += (int)dz;
            rightCorner.Z += (int)dz;

            return true;
        }

        public void RoundEdgesNearCrevices(MaterialGenState state)
        {
            if (state.MaxDistanceToCrevice < 2 || state.Rand.NextDouble() < state.Settings.NoCreviceSmoothingChance)
            {
                return;
            }
            float[,] distances = new float[state.Width, state.Height];

            float pers = state.RoundCornerDistPers;
            float amp = state.RoundCornerDistAmp;
            float freq = state.RoundCornerDistFreq;
            int octaves = 2;

            float[,] noiseOutputs = _noiseService.Generate(pers, freq, amp, octaves, state.Rand.Next(), state.Width, state.Height);

            for (int x = 0; x < state.Width; x++)
            {
                for (int z = 0; z < state.Height; z++)
                {
                    distances[x, z] = (float)(state.MaxDistanceToCrevice);
                }
            }

            for (int x = 0; x < state.Width; x++)
            {
                for (int z = 0; z < state.Height; z++)
                {

                    int currDistToCrevice = (int)Math.Ceiling((state.MaxDistanceToCrevice * (1 + noiseOutputs[x, z])));
                    if (state.Block.BumpHeights[x, z] == MaterialGenConstants.RecessedBumpHeight)
                    {
                        distances[x, z] = 0;
                    }
                    else
                    {
                        continue;
                    }

                    for (int xx = 1; xx < currDistToCrevice; xx++)
                    {
                        int cx = (x + xx).SafeMod(state.Width);

                        if (state.Block.BumpHeights[cx, z] == MaterialGenConstants.RecessedBumpHeight)
                        {
                            break;
                        }

                        if (distances[cx, z] > xx)
                        {
                            distances[cx, z] = xx;
                        }
                    }

                    for (int xx = 1; xx < currDistToCrevice; xx++)
                    {
                        int cx = (x - xx).SafeMod(state.Width);

                        if (state.Block.BumpHeights[cx, z] == MaterialGenConstants.RecessedBumpHeight)
                        {
                            break;
                        }

                        if (distances[cx, z] > xx)
                        {
                            distances[cx, z] = xx;
                        }
                    }

                    for (int zz = 1; zz < currDistToCrevice; zz++)
                    {
                        int cz = (z + zz).SafeMod(state.Height);

                        if (state.Block.BumpHeights[x, cz] == MaterialGenConstants.RecessedBumpHeight)
                        {
                            break;
                        }

                        if (distances[x, cz] > zz)
                        {
                            distances[x, cz] = zz;
                        }
                    }

                    for (int zz = 1; zz < currDistToCrevice; zz++)
                    {
                        int cz = (z - zz).SafeMod(state.Height);

                        if (state.Block.BumpHeights[x, cz] == MaterialGenConstants.RecessedBumpHeight)
                        {
                            break;
                        }

                        if (distances[x, cz] > zz)
                        {
                            distances[x, cz] = zz;
                        }
                    }
                }
            }

            for (int x = 0; x < state.Width; x++)
            {
                for (int z = 0; z < state.Height; z++)
                {
                    if (distances[x, z] > 0)
                    {

                        float startDist = distances[x, z] / (state.MaxDistanceToCrevice);

                        startDist = (float)Math.Pow(startDist, 0.1f);
                        float initialHeight = startDist * (1 + noiseOutputs[x, z]);
                        state.Block.BumpHeights[x, z] *= initialHeight;
                    }
                    else
                    {
                        state.Block.BumpHeights[x, z] = 0;
                    }
                }
            }
        }



        public List<LineCell> ConnectLowerLeftToUpperRightPoint(MaterialGenState state, CornerPoint lowerLeft, CornerPoint upperRight, float bumpHeight, int minWidth, int maxWidth)
        {
            List<LineCell> retval = new List<LineCell>();
            if (lowerLeft == null || upperRight == null)
            {
                return retval;
            }
            int llx = lowerLeft.X;
            int llz = lowerLeft.Z;
            int urx = upperRight.X;
            int urz = upperRight.Z;

            int dx = Math.Abs(urx - llx);
            int dz = Math.Abs(urz - llz);

            int origDx = Math.Abs(lowerLeft.OrigX - upperRight.OrigX);
            int origDz = Math.Abs(lowerLeft.OrigZ - upperRight.OrigZ);

            // Vertical, fix it if the original looped.
            if (lowerLeft.OrigX == upperRight.OrigX && lowerLeft.OrigZ > upperRight.OrigZ)
            {
                urz += state.Height;
            }

            // Horizontal
            if (lowerLeft.OrigZ == upperRight.OrigZ && lowerLeft.OrigX > upperRight.OrigX)
            {
                urx += state.Width;
            }

            LineGenParameters lineGenParams = new LineGenParameters()
            {
                WidthSize = state.Rand.Next(minWidth, maxWidth),
                WidthSizeChangeAmount = 0,
                WidthPosShiftChance = 0.00f,
                WidthPosShiftSize = 1,
                MaxWidthPosDrift = 0,
                LinePathNoiseScale = 0.0f,
                InitialNoPosShiftLength = 0,
                MaxWidthSize = maxWidth,
                MinWidthSize = minWidth,
                Seed = state.Rand.Next(),
                WidthSizeChangeChance = 0.03f
            };

            int newdx = llx - urx;
            int newdz = llz - urz;

            float newDist = Mathf.Sqrt((newdx * newdx + newdz * newdz));

            if (state.Rand.NextDouble() < state.CurvedWallChance)
            {
                lineGenParams.LinePathNoiseScale = 1.0f;
            }

            retval = _lineGenService.GetBressenhamLine(new Point2I(llx, llz), new Point2I(urx, urz), lineGenParams);

            foreach (LineCell pt in retval)
            {
                int px = ((int)pt.X).SafeMod(state.Width);
                int pz = ((int)pt.Z).SafeMod(state.Height);

                state.Block.BumpHeights[px, pz] = bumpHeight;
            }

            return retval;
        }

        public void AddColorNoise(MaterialGenState state)
        {
            AddColorNoiseToLayer(state, true);
        }

        class FullScaledColor
        {
            public ScaledColor ScaledColor;
            public float[,] Noise;
            public float NoiseBumpScale;
        }

        private void AddColorNoiseToLayer(MaterialGenState state, bool isFront)
        {

            List<ScaledColor> accentColors = new List<ScaledColor>();
            if (isFront)
            {
                accentColors = state.ForegroundNoise;
            }
            else
            {
                accentColors = state.BackgroundNoise;
            }

            List<FullScaledColor> fullScaledColors = new List<FullScaledColor>();

            foreach (ScaledColor accentColor in accentColors)
            {
                FullScaledColor full = new FullScaledColor() { ScaledColor = accentColor };
                fullScaledColors.Add(full);

                float pers = RandUtils.FloatRange(state.Settings.MinColorNoisePers, state.Settings.MaxColorNoisePers, state.Rand);
                float amp = RandUtils.FloatRange(state.Settings.MinColorNoiseAmp, state.Settings.MaxColorNoiseAmp, state.Rand);

                float freq = RandUtils.FloatRange(state.Settings.MinColorNoiseFreq, state.Settings.MaxColorNoiseFreq, state.Rand);

                int octaves = state.Settings.ColorNoiseOctaves;
                full.Noise = _noiseService.Generate(pers, freq, amp, octaves, state.Rand.Next(), state.Width, state.Height);
                full.NoiseBumpScale = RandUtils.FloatRange(0, state.Settings.MaxColorNoiseBumpScale, state.Rand);

            }


            for (int w = 0; w < state.Width; w++)
            {
                for (int h = 0; h < state.Height; h++)
                {
                    if (state.Block.BumpHeights[w, h] <= MaterialGenConstants.MaxRecessedBumpHeight == isFront)
                    {
                        continue;
                    }

                    for (int s = 0; s < fullScaledColors.Count; s++)
                    {
                        FullScaledColor full = fullScaledColors[s];

                        float noiseValue = full.Noise[w, h];

                        noiseValue *= (1 + RandUtils.FloatRange(-state.Settings.ColorPerPixelNoiseDelta, state.Settings.ColorPerPixelNoiseDelta, state.Rand));

                        if (Math.Abs(noiseValue) < full.ScaledColor.EffectThreshold)
                        {
                            continue;
                        }


                        state.Block.Colors[w, h] += noiseValue * full.ScaledColor.Color;
                        state.Block.BumpHeights[w, h] += noiseValue * full.NoiseBumpScale;
                    }
                }
            }
        }

        private int GetClampedValueNear(int startValue, int otherValue, int maxDistance, int shiftValue)
        {
            int dist = Math.Abs(startValue - otherValue);

            if (dist >= maxDistance)
            {
                if (otherValue < startValue)
                {
                    otherValue += shiftValue;
                }
                else
                {
                    otherValue -= shiftValue;
                }
            }
            return otherValue;
        }

        public void RoundCreviceCorner(MaterialGenState state, List<CornerPoint> corners, CornerPoint currentCorner, int lowAnglePoint, int highAnglePoint)
        {

            CornerPoint low = corners.FirstOrDefault(x => x.Index == lowAnglePoint);

            CornerPoint high = corners.FirstOrDefault(x => x.Index == highAnglePoint);

            if (low == null || high == null)
            {
                return;
            }

            double maxDistToAnotherCorner = 1000000;

            foreach (CornerPoint c2 in corners)
            {
                if (c2.Index == currentCorner.Index)
                {
                    continue;
                }
                float dx = (c2.X - currentCorner.X);
                float dz = (c2.Z - currentCorner.Z);

                double otherDist = Math.Sqrt(dx * dx + dz * dz);

                if (otherDist < maxDistToAnotherCorner)
                {
                    maxDistToAnotherCorner = otherDist;
                }
            }

            int shiftDistance = state.Width;
            int maxDistance = state.Width / 3;

            int cx = currentCorner.X;
            int cz = currentCorner.Z;
            int lx = GetClampedValueNear(currentCorner.X, low.X, maxDistance, shiftDistance);
            int lz = GetClampedValueNear(currentCorner.Z, low.Z, maxDistance, shiftDistance);

            float ldx = lx - cx;
            float ldz = lz - cz;

            int hx = GetClampedValueNear(currentCorner.X, high.X, maxDistance, shiftDistance);
            int hz = GetClampedValueNear(currentCorner.Z, high.Z, maxDistance, shiftDistance);

            float hdx = hx - cx;
            float hdz = hz - cz;


            float lowRadians = Mathf.Atan2(ldz, ldx);
            float highRadians = Mathf.Atan2(hdz, hdx);

            if (lowRadians > highRadians)
            {
                highRadians += 2 * Mathf.PI;
            }

            float midRadians = (lowRadians + highRadians) / 2;

            float radiansDiff = highRadians - lowRadians;

            float angleDiff = radiansDiff * 180 / Mathf.PI;

            float angleScale = 90 / angleDiff;

            float creviceSizeScale = angleScale;

            if (creviceSizeScale > 1)
            {
                creviceSizeScale *= creviceSizeScale;
            }

            float distScale = angleScale;

            float midSin = Mathf.Sin(midRadians);
            float midCos = Mathf.Cos(midRadians);

            float dist = RandUtils.FloatRange(state.RoundCornerMinSize, state.RoundCornerMaxSize, state.Rand);

            if (dist > maxDistToAnotherCorner)
            {
                dist = (float)maxDistToAnotherCorner;
            }

            dist *= distScale;

            int mx = (int)(cx + midCos * dist);
            int mz = (int)(cz + midSin * dist);

            int intDist = (int)Math.Ceiling(dist * 1.0f);

            float tdx = mx - cx;
            float tdz = mz - cz;

            float tdist = MathF.Sqrt(tdx * tdx + tdz * tdz);

            tdist /= 2;

            for (int xMain = cx - intDist; xMain <= cx + intDist; xMain++)
            {
                int cdx = xMain - cx;
                int cmx = xMain - mx;
                for (int zMain = cz - intDist; zMain <= cz + intDist; zMain++)
                {
                    int cdz = zMain - cz;
                    int cmz = zMain - mz;

                    float cdist = Mathf.Sqrt(cdx * cdx + cdz * cdz);
                    float mdist = Mathf.Sqrt(cmx * cmx + cmz * cmz);

                    float mdistRatio = mdist / tdist;

                    if (mdist < tdist)
                    {
                        continue;
                    }

                    float mRatio = mdist / tdist;

                    if (mdist * mRatio < cdist / creviceSizeScale)
                    {
                        continue;
                    }

                    float newRadians = Mathf.Atan2(cdz, cdx);

                    if (newRadians < lowRadians)
                    {
                        newRadians += Mathf.PI * 2;
                    }
                    if (newRadians > highRadians)
                    {
                        newRadians -= Mathf.PI * 2;
                    }

                    if (newRadians < lowRadians || newRadians > highRadians)
                    {
                        continue;
                    }

                    int nx = xMain.SafeMod(state.Width);

                    int nz = zMain.SafeMod(state.Height);

                    state.Block.BumpHeights[nx, nz] = MaterialGenConstants.RecessedBumpHeight;
                }
            }
        }

        public void ApplyBlockToTexture(MaterialGenState state, MaterialGenBlock block, Texture2D tex)
        {
            for (int w = 0; w < state.Width; w++)
            {
                for (int h = 0; h < state.Height; h++)
                {
                    Color c = block.Colors[w, h];
                    c *= block.Brightness[w, h];

                    if (block.GrayScaleScaling[w, h] != 0)
                    {
                        c = ColorUtils.ShiftSaturation(c, block.GrayScaleScaling[w, h]);
                    }
                    float a = block.BumpHeights[w, h];
                    tex.SetPixel(w, h, new Color(c.r, c.g, c.b, a));
                }
            }
            tex.Apply();
        }

        public void ApplyRecessedColors(MaterialGenState state)
        {
            for (int w = 0; w < state.Width; w++)
            {
                for (int h = 0; h < state.Height; h++)
                {
                    if (state.Block.BumpHeights[w, h] > MaterialGenConstants.MaxRecessedBumpHeight)
                    {
                        continue;
                    }

                    float bumpHeight = state.Block.BumpHeights[w, h];
                    Color curr = state.Block.Colors[w, h];
                    state.Block.Colors[w, h] = (curr * bumpHeight) + (1 - bumpHeight) * state.BackgroundMain;
                }
            }

            AddColorNoiseToLayer(state, false);
        }

        public void CreateBlockGrid(MaterialGenState state)
        {
            for (int i = 0; i < state.Block.Blocks.Count; i++)
            {

            }
        }

        public void SmoothColors(MaterialGenState state)
        {
            int width = state.Width;
            int height = state.Height;
            float[,] tempAlphas = new float[width, height];
            Color[,] tempColors = new Color[width, height];
            float finalDivisor = 4; // 1 + 0.5*4 + 0.25*4 for middle, udlr, 4 corner
            for (int xMain = 0; xMain < width; xMain++)
            {
                for (int zMain = 0; zMain < height; zMain++)
                {
                    float alphaSum = 0;

                    Color colorSum = Color.black;

                    finalDivisor = 0;

                    int radius = 1;

                    for (int xx = xMain - radius; xx <= xMain + radius; xx++)
                    {
                        int cx = (xx + width) % width;
                        int dx = Math.Abs(xx - xMain);
                        for (int zz = zMain - radius; zz <= zMain + radius; zz++)
                        {
                            int cz = (zz + height) % height;

                            int dz = Math.Abs(zz - zMain);

                            // Final divisor depends on how this is calculated.
                            float currDivisor = (dx + 1) * (dz + 1);
                            currDivisor = 1;
                            finalDivisor += currDivisor;

                            colorSum += state.Block.Colors[cx, cz] / currDivisor;
                            alphaSum += state.Block.BumpHeights[cx, cz] / currDivisor;
                        }
                    }

                    tempAlphas[xMain, zMain] = alphaSum / finalDivisor;
                    tempColors[xMain, zMain] = colorSum / finalDivisor;
                }
            }
            state.Block.BumpHeights = tempAlphas;
            state.Block.Colors = tempColors;
        }

        public bool ChangeBrightnessInForegroundRegion(MaterialGenState state, TextureBlock block)
        {
            bool[,] didChangeBrightness = new bool[state.Width, state.Height];

            if (state.Rand.NextDouble() > state.Settings.ChangeBlockColorChance)
            {
                return false;
            }

            Color currColor = state.Block.Colors[block.CX, block.CZ];

            float currBrightnessDelta = RandUtils.DeltaRange(state.Settings.MaxBrightnessDelta, state.Rand);
            float brightnessScale = 1 + currBrightnessDelta;

            float bumpDelta = currBrightnessDelta * RandUtils.DeltaRange(state.Settings.MaxBrightnessBumpScale, state.Rand);

            float grayScaleScaling = 0;

            if (state.Rand.NextDouble() < state.Settings.MakeGrayscaleColorChangeChance)
            {
                grayScaleScaling = RandUtils.FloatRange(state.Settings.MinGrayscaleShift, state.Settings.MaxGrayscaleShift, state.Rand);
            }

            for (int x = 0; x < state.Width; x++)
            {
                for (int z = 0; z < state.Height; z++)
                {
                    if (state.Block.BlockIndexes[x, z] != block.Index)
                    {
                        continue;
                    }
                    state.Block.Brightness[x, z] *= brightnessScale;
                    state.Block.AddFrontBumpHeight(x, z, bumpDelta);
                    state.Block.GrayScaleScaling[x, z] += grayScaleScaling;
                }
            }

            return true;
        }

        public void SetupColorBlockRegion(MaterialGenState state, TextureBlock block)
        {

            int pointsAdded = 0;
            Queue<Point2I> queue = new Queue<Point2I>();

            queue.Enqueue(new Point2I(block.CX, block.CZ));

            while (queue.TryDequeue(out Point2I pt))
            {
                int x = MathUtil.ModClamp(pt.X, state.Width);
                int z = MathUtil.ModClamp(pt.Z, state.Height);
                if (CanSetBlockIndex(state, pt.X, pt.Z))
                {
                    state.Block.BlockIndexes[x, z] = block.Index;
                    state.Block.DidCheckBlockIndex[x, z] = true;
                    pointsAdded++;
                }

                TryEnqueueNewBlockPoint(state, queue, x - 1, z);
                TryEnqueueNewBlockPoint(state, queue, x + 1, z);
                TryEnqueueNewBlockPoint(state, queue, x, z - 1);
                TryEnqueueNewBlockPoint(state, queue, x, z + 1);

            }
        }

        private bool CanSetBlockIndex(MaterialGenState state, int x, int z)
        {

            x = MathUtil.ModClamp(x, state.Width);
            z = MathUtil.ModClamp(z, state.Height);

            if (state.Block.BlockIndexes[x, z] != 0
                || state.Block.BumpHeights[x, z] <= MaterialGenConstants.RecessedBumpHeight)
            {
                return false;
            }
            return true;
        }

        private void TryEnqueueNewBlockPoint(MaterialGenState state, Queue<Point2I> queue, int x, int z)
        {
            x = MathUtil.ModClamp(x, state.Width);
            z = MathUtil.ModClamp(z, state.Height);

            if (!CanSetBlockIndex(state, x, z) ||
                state.Block.DidCheckBlockIndex[x, z])
            {
                return;
            }

            queue.Enqueue(new Point2I(x, z));
            state.Block.BlockIndexes[x, z] = 0;
            state.Block.DidCheckBlockIndex[x, z] = true;

        }

        public bool MakeBlockVeryRound(MaterialGenState state, TextureBlock block)
        {
            if (state.Rand.NextDouble() > state.Settings.MakeVeryRoundChance)
            {
                return false;
            }
            return false;
        }

        public bool RemoveBlockFromWall(MaterialGenState state, TextureBlock block)
        {
            bool[,] didChangeStoneShape = new bool[state.Width, state.Height];

            if (state.Rand.NextDouble() > state.Settings.RemoveBlockChance)
            {
                return false;
            }

            for (int x = 0; x < state.Width; x++)
            {
                for (int z = 0; z < state.Height; z++)
                {
                    if (state.Block.BlockIndexes[x, z] != block.Index)
                    {
                        continue;
                    }

                    state.Block.BumpHeights[x, z] = MaterialGenConstants.RecessedBumpHeight;

                }
            }


            return true;
        }

        public void AddCracksToFrontRegions(MaterialGenState state)
        {
            int finalCrackCount = (int)(state.Settings.CrackCount * (1 + RandUtils.DeltaRange(state.Settings.CrackQuantityDelta, state.Rand)));


            int cracksLeft = finalCrackCount;

            int maxAttempts = 10 * finalCrackCount;


            while (cracksLeft > 0 && --maxAttempts > 0)
            {
                int centerX = RandUtils.IntRange(0, state.Width - 1, state.Rand);
                int centerZ = RandUtils.IntRange(0, state.Height - 1, state.Rand);

                if (state.Block.BumpHeights[centerX, centerZ] < MaterialGenConstants.MaxRecessedBumpHeight)
                {
                    continue;
                }

                if (state.Block.BlockIndexes[centerX, centerZ] < 1)
                {
                    continue;
                }

                TextureBlock block = state.Block.Blocks.FirstOrDefault(x => x.Index == state.Block.BlockIndexes[centerX, centerZ]);

                if (block == null || block.CrackCount >= state.Settings.MaxCracksPerBlock)
                {
                    continue;
                }

                block.CrackCount++;

                cracksLeft--;

                float angle1 = RandUtils.FloatRange(0, 360, state.Rand);

                if (state.Rand.NextDouble() < 0.5f)
                {
                    //angle1 = RandUtils.FloatRange(45, 135, state.Rand);
                }

                float angle2 = angle1 + RandUtils.FloatRange(-30, 30, state.Rand) + 180;

                List<float> angles = new List<float>() { angle1, angle2 };

                float crackColorScale = 1 + RandUtils.FloatRange(-state.Settings.CrackBrightnessMaxDelta, 0, state.Rand);
                foreach (float angle in angles)
                {

                    float sin = Mathf.Sin(angle * Mathf.PI / 180);
                    float cos = Mathf.Cos(angle * Mathf.PI / 180);

                    int maxDist = RandUtils.IntRange(200, 1000, state.Rand);

                    float currAngle = angle;

                    float currX = centerX;
                    float currZ = centerZ;

                    for (int dist = 0; dist < maxDist; dist++)
                    {
                        if (state.Rand.NextDouble() < state.Settings.CrackChangeDirChance)
                        {
                            currAngle = angle + RandUtils.DeltaRange(state.Settings.CrackChangeDirAngleDelta, state.Rand);
                        }

                        currX += Mathf.Cos(currAngle * Mathf.PI / 180);
                        currZ += Mathf.Sin(currAngle * Mathf.PI / 180);

                        int nx = MathUtil.ModClamp((int)(currX), state.Width);
                        int nz = MathUtil.ModClamp((int)(currZ), state.Height);

                        if (state.Block.BlockIndexes[nx, nz] != block.Index)
                        {
                            break;
                        }

                        state.Block.Colors[nx, nz] *= crackColorScale;
                    }
                }
            }
        }

        public void RandomizeBumpHeights(MaterialGenState state)
        {
            float pers = RandUtils.FloatRange(state.Settings.MinColorNoisePers, state.Settings.MaxColorNoisePers, state.Rand) * RandUtils.FloatRange(0.5f, 2.0f, state.Rand);
            float amp = RandUtils.FloatRange(state.Settings.MinColorNoiseAmp, state.Settings.MaxColorNoiseAmp, state.Rand) * RandUtils.FloatRange(0.4f, 0.8f, state.Rand);

            float freq = RandUtils.FloatRange(state.Settings.MinColorNoiseFreq, state.Settings.MaxColorNoiseFreq, state.Rand) * RandUtils.FloatRange(0.5f, 1.0f, state.Rand);

            int octaves = state.Settings.ColorNoiseOctaves;
            octaves = 3;
            float[,] noise = _noiseService.Generate(pers, freq, amp, octaves, state.Rand.Next(), state.Width, state.Height);

            for (int w = 0; w < state.Width; w++)
            {
                for (int h = 0; h < state.Height; h++)
                {

                    float bumpHeight = Math.Max(2 * MaterialGenConstants.MaxRecessedBumpHeight, state.Block.BumpHeights[w, h]) * (1 + noise[w, h]);
                    if (bumpHeight <= MaterialGenConstants.MaxRecessedBumpHeight + 0.01f)
                    {
                        bumpHeight = MaterialGenConstants.MaxRecessedBumpHeight + 0.01f;
                    }
                    state.Block.BumpHeights[w, h] = bumpHeight;
                }
            }
        }
    }
}
