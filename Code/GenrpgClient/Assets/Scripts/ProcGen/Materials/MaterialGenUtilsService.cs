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
        List<MyPointF> ConnectLowerLeftToUpperRightPoint(MaterialGenState state, CornerPoint lowerLeft, CornerPoint upperRight, float bumpHeight, int minWidth, int maxWidth);

        void RoundCreviceCorner(MaterialGenState state, List<CornerPoint> corners, CornerPoint currentCorner, int lowAnglePoint, int highAnglePoint);

        void AddColorNoise(MaterialGenState state);
        void ApplyBlockToTexture(MaterialGenState state, MaterialGenBlock block, Texture2D tex);

        void ApplyRecessedColors(MaterialGenState state);

        void RandomizeBumpHeights(MaterialGenState state);

        void SmoothColors(MaterialGenState state);

        bool ChangeBrightnessInForegroundRegion(MaterialGenState state, int x, int y);

        bool MakeBlockVeryRound(MaterialGenState state, int x, int y);

        bool RemoveBlockFromWall(MaterialGenState state, int cx, int cy);


        void AddCracksToFrontRegions(MaterialGenState state);

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


            float dy = RandUtils.FloatRange(maxPerturb / 2, maxPerturb, state.Rand);
            if (centerDownCorner != null)
            {
                dy = -dy;
            }


            CornerPoint newLeftCorner = new CornerPoint(leftCorner.X, leftCorner.Y)
            {
                Index = corners.Max(x => x.Index) + 1,
                LeftIndex = leftCorner.LeftIndex,
                ReplacesIndex = leftCorner.Index,
                IsLeftReplace = true
            };
            corners.Add(newLeftCorner);
            CornerPoint newRightCorner = new CornerPoint(rightCorner.X, rightCorner.Y)
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

            thisCorner.Y += (int)dy;
            leftCorner.Y += (int)dy;
            rightCorner.Y += (int)dy;

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
                for (int y = 0; y < state.Height; y++)
                {
                    distances[x, y] = (float)(state.MaxDistanceToCrevice);
                }
            }

            for (int x = 0; x < state.Width; x++)
            {
                for (int y = 0; y < state.Height; y++)
                {

                    int currDistToCrevice = (int)Math.Ceiling((state.MaxDistanceToCrevice * (1 + noiseOutputs[x, y])));
                    if (state.Block.BumpHeights[x, y] == MaterialGenConstants.RecessedBumpHeight)
                    {
                        distances[x, y] = 0;
                    }
                    else
                    {
                        continue;
                    }

                    for (int xx = 1; xx < currDistToCrevice; xx++)
                    {
                        int cx = (x + xx).SafeMod(state.Width);

                        if (state.Block.BumpHeights[cx, y] == MaterialGenConstants.RecessedBumpHeight)
                        {
                            break;
                        }

                        if (distances[cx, y] > xx)
                        {
                            distances[cx, y] = xx;
                        }
                    }

                    for (int xx = 1; xx < currDistToCrevice; xx++)
                    {
                        int cx = (x - xx).SafeMod(state.Width);

                        if (state.Block.BumpHeights[cx, y] == MaterialGenConstants.RecessedBumpHeight)
                        {
                            break;
                        }

                        if (distances[cx, y] > xx)
                        {
                            distances[cx, y] = xx;
                        }
                    }

                    for (int yy = 1; yy < currDistToCrevice; yy++)
                    {
                        int cy = (y + yy).SafeMod(state.Height);

                        if (state.Block.BumpHeights[x, cy] == MaterialGenConstants.RecessedBumpHeight)
                        {
                            break;
                        }

                        if (distances[x, cy] > yy)
                        {
                            distances[x, cy] = yy;
                        }
                    }

                    for (int yy = 1; yy < currDistToCrevice; yy++)
                    {
                        int cy = (y - yy).SafeMod(state.Height);

                        if (state.Block.BumpHeights[x, cy] == MaterialGenConstants.RecessedBumpHeight)
                        {
                            break;
                        }

                        if (distances[x, cy] > yy)
                        {
                            distances[x, cy] = yy;
                        }
                    }
                }
            }

            for (int x = 0; x < state.Width; x++)
            {
                for (int y = 0; y < state.Height; y++)
                {
                    if (distances[x, y] > 0)
                    {

                        float startDist = distances[x, y] / (state.MaxDistanceToCrevice);

                        startDist = (float)Math.Pow(startDist, 0.1f);
                        float initialHeight = startDist * (1 + noiseOutputs[x, y]);
                        state.Block.BumpHeights[x, y] *= initialHeight;
                    }
                    else
                    {
                        state.Block.BumpHeights[x, y] = 0;
                    }
                }
            }
        }



        public List<MyPointF> ConnectLowerLeftToUpperRightPoint(MaterialGenState state, CornerPoint lowerLeft, CornerPoint upperRight, float bumpHeight, int minWidth, int maxWidth)
        {
            List<MyPointF> retval = new List<MyPointF>();
            if (lowerLeft == null || upperRight == null)
            {
                return retval;
            }
            int llx = lowerLeft.X;
            int lly = lowerLeft.Y;
            int urx = upperRight.X;
            int ury = upperRight.Y;


            int dx = Math.Abs(urx - llx);
            int dy = Math.Abs(ury - lly);

            int origDx = Math.Abs(lowerLeft.OrigX - upperRight.OrigX);
            int origDy = Math.Abs(lowerLeft.OrigY - upperRight.OrigY);



            // Vertical, fix it if the original looped.
            if (lowerLeft.OrigX == upperRight.OrigX && lowerLeft.OrigY > upperRight.OrigY)
            {
                ury += state.Height;
            }


            // Horizontal
            if (lowerLeft.OrigY == upperRight.OrigY && lowerLeft.OrigX > upperRight.OrigX)
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
            int newdy = lly - ury;

            float newDist = Mathf.Sqrt((newdx * newdx + newdy * newdy));

            if (state.Rand.NextDouble() < state.CurvedWallChance)
            {
                lineGenParams.LinePathNoiseScale = 1.0f;
            }

            retval = _lineGenService.GetBressenhamLine(new MyPoint(llx, lly), new MyPoint(urx, ury), lineGenParams);

            foreach (MyPointF pt in retval)
            {
                int px = ((int)pt.X).SafeMod(state.Width);
                int py = ((int)pt.Y).SafeMod(state.Height);

                state.Block.BumpHeights[px, py] = bumpHeight;
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
                full.NoiseBumpScale = RandUtils.FloatRange(state.Settings.MaxColorNoiseBumpScale / 2, state.Settings.MaxColorNoiseBumpScale, state.Rand);

            }


            for (int w = 0; w < state.Width; w++)
            {
                for (int h = 0; h < state.Height; h++)
                {
                    if (state.Block.BumpHeights[w, h] <= MaterialGenConstants.MaxRecessedBumpHeight == isFront)
                    {
                        continue;
                    }

                    float maxNoiseValue = -1;
                    FullScaledColor maxIntensityColor = null;
                    for (int s = 0; s < fullScaledColors.Count; s++)
                    {
                        FullScaledColor full = fullScaledColors[s];

                        float noiseValue = full.Noise[w, h];

                        noiseValue *= (1 + RandUtils.FloatRange(-state.Settings.ColorPerPixelNoiseDelta, state.Settings.ColorPerPixelNoiseDelta, state.Rand));

                        noiseValue -= full.ScaledColor.EffectThreshold;

                        noiseValue = MathUtil.Clamp(0, noiseValue, 1);

                        if (noiseValue > 0 && noiseValue >= maxNoiseValue)
                        {
                            if (noiseValue > maxNoiseValue || (noiseValue == maxNoiseValue && state.Rand.NextDouble() < 0.5))
                            {
                                maxIntensityColor = full;
                                maxNoiseValue = noiseValue;
                            }
                        }
                    }

                    if (maxIntensityColor != null)
                    {
                        state.Block.Colors[w, h] = Color.Lerp(state.Block.Colors[w, h], maxIntensityColor.ScaledColor.Color,
                            maxNoiseValue);
                        state.Block.BumpHeights[w, h] += maxNoiseValue * maxIntensityColor.NoiseBumpScale;
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
                float dy = (c2.Y - currentCorner.Y);


                double otherDist = Math.Sqrt(dx * dx + dy * dy);

                if (otherDist < maxDistToAnotherCorner)
                {
                    maxDistToAnotherCorner = otherDist;
                }
            }

            int shiftDistance = state.Width;
            int maxDistance = state.Width / 3;

            int cx = currentCorner.X;
            int cy = currentCorner.Y;
            int lx = GetClampedValueNear(currentCorner.X, low.X, maxDistance, shiftDistance);
            int ly = GetClampedValueNear(currentCorner.Y, low.Y, maxDistance, shiftDistance);

            float ldx = lx - cx;
            float ldy = ly - cy;

            int hx = GetClampedValueNear(currentCorner.X, high.X, maxDistance, shiftDistance);
            int hy = GetClampedValueNear(currentCorner.Y, high.Y, maxDistance, shiftDistance);

            float hdx = hx - cx;
            float hdy = hy - cy;


            float lowRadians = Mathf.Atan2(ldy, ldx);
            float highRadians = Mathf.Atan2(hdy, hdx);

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
            int my = (int)(cy + midSin * dist);

            int intDist = (int)Math.Ceiling(dist * 1.0f);

            float tdx = mx - cx;
            float tdy = my - cy;

            float tdist = MathF.Sqrt(tdx * tdx + tdy * tdy);

            tdist /= 2;

            for (int xMain = cx - intDist; xMain <= cx + intDist; xMain++)
            {
                int cdx = xMain - cx;
                int cmx = xMain - mx;
                for (int yMain = cy - intDist; yMain <= cy + intDist; yMain++)
                {
                    int cdy = yMain - cy;
                    int cmy = yMain - my;

                    float cdist = Mathf.Sqrt(cdx * cdx + cdy * cdy);
                    float mdist = Mathf.Sqrt(cmx * cmx + cmy * cmy);

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

                    float newRadians = Mathf.Atan2(cdy, cdx);

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

                    int ny = yMain.SafeMod(state.Height);

                    state.Block.BumpHeights[nx, ny] = MaterialGenConstants.RecessedBumpHeight;
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


        public void SmoothColors(MaterialGenState state)
        {
            int width = state.Width;
            int height = state.Height;
            float[,] tempAlphas = new float[width, height];
            Color[,] tempColors = new Color[width, height];
            float finalDivisor = 4; // 1 + 0.5*4 + 0.25*4 for middle, udlr, 4 corner
            for (int xMain = 0; xMain < width; xMain++)
            {
                for (int yMain = 0; yMain < height; yMain++)
                {
                    float alphaSum = 0;

                    Color colorSum = Color.black;

                    finalDivisor = 0;

                    int radius = 1;

                    for (int xx = xMain - radius; xx <= xMain + radius; xx++)
                    {
                        int cx = (xx + width) % width;
                        for (int yy = yMain - radius; yy <= yMain + radius; yy++)
                        {
                            int cy = (yy + height) % height;

                            int dx = Math.Abs(xx - xMain);
                            int dy = Math.Abs(yy - yMain);

                            // Final divisor depends on how this is calculated.
                            float currDivisor = (dx + 1) * (dy + 1);
                            currDivisor = 1;
                            finalDivisor += currDivisor;

                            colorSum += state.Block.Colors[cx, cy] / currDivisor;
                            alphaSum += state.Block.BumpHeights[cx, cy] / currDivisor;
                        }
                    }

                    tempAlphas[xMain, yMain] = alphaSum / finalDivisor;
                    tempColors[xMain, yMain] = colorSum / finalDivisor;
                }
            }
            state.Block.BumpHeights = tempAlphas;
            state.Block.Colors = tempColors;
        }

        public bool ChangeBrightnessInForegroundRegion(MaterialGenState state, int cx, int cy)
        {
            bool[,] didChangeBrightness = new bool[state.Width, state.Height];

            if (state.Rand.NextDouble() > state.Settings.ModifyBlockChance)
            {
                return false;
            }

            float currBrightnessDelta = RandUtils.DeltaRange(state.Settings.MaxBrightnessDelta, state.Rand);
            float brightnessScale = 1 + currBrightnessDelta;

            float bumpDelta = currBrightnessDelta * RandUtils.DeltaRange(state.Settings.MaxBrightnessBumpScale, state.Rand);

            Queue<PointXZ> openPoints = new Queue<PointXZ>();

            openPoints.Enqueue(new PointXZ(cx, cy));

            while (openPoints.TryDequeue(out PointXZ pt))
            {
                ChangeBrightnessAtPoint(state, didChangeBrightness, brightnessScale, bumpDelta, pt.X, pt.Z, openPoints);
            }
            return true;
        }

        private void ChangeBrightnessAtPoint(MaterialGenState state, bool[,] didChangeBrightness, float brightnessScale, float bumpDelta, int x, int y, Queue<PointXZ> openList)
        {
            if (x < 0 || x >= state.Width || y < 0 || y >= state.Height)
            {
                return;
            }

            if (didChangeBrightness[x, y])
            {
                return;
            }

            didChangeBrightness[x, y] = true;

            if (state.Block.BumpHeights[x, y] < MaterialGenConstants.MaxRecessedBumpHeight)
            {
                return;
            }

            state.Block.Brightness[x, y] *= brightnessScale;
            state.Block.AddFrontBumpHeight(x, y, bumpDelta);

            openList.Enqueue(new PointXZ(x - 1, y));
            openList.Enqueue(new PointXZ(x + 1, y));
            openList.Enqueue(new PointXZ(x, y - 1));
            openList.Enqueue(new PointXZ(x, y + 1));
        }

        public bool MakeBlockVeryRound(MaterialGenState state, int cx, int cy)
        {
            bool[,] didChangeStoneShape = new bool[state.Width, state.Height];

            if (state.Rand.NextDouble() > state.Settings.ModifyBlockChance)
            {
                return false;
            }
            return false;
            //int minDistSquared = 1000000;

            //Queue<PointXZ> openPoints = new Queue<PointXZ>();

            //openPoints.Enqueue(new PointXZ(cx, cy));
            //int pointCount = 0;
            //while (openPoints.TryDequeue(out PointXZ pt))
            //{
            //    AddPointToCurrentBlock(state, didChangeStoneShape, pt.X, pt.Z, openPoints);
            //    pointCount++;
            //}

            //if (pointCount < 20)
            //{
            //    return false;
            //}


            //int biggestDx = 0;
            //int biggestDy = 0;

            //for (int xMain = 0; xMain < state.Size; xMain++)
            //{
            //    int dx = Math.Abs(xMain - cx);
            //    if (dx > state.Size / 2)
            //    {
            //        dx = state.Size - dx;
            //    }

            //    for (int yMain = 0; yMain < state.Size; yMain++)
            //    {
            //        if (!didChangeStoneShape[xMain, yMain])
            //        {
            //            continue;
            //        }

            //        if (dx > biggestDx)
            //        {
            //            biggestDx = dx;
            //        }

            //        int dy = Math.Abs(yMain - cy);
            //        if (dy > state.Size / 2)
            //        {
            //            dy = state.Size - dy;
            //        }

            //        if (dy > biggestDy)
            //        {
            //            biggestDy = dy;
            //        }

            //        state.Block.BumpHeights[xMain, yMain] = MaterialGenConstants.RecessedBumpHeight;
            //    }
            //}

            //float finalDx = (biggestDx).SafeMod(state.Size);
            //float finalDy = (biggestDy).SafeMod(state.Size);

            //finalDx *= RandUtils.DeltaScale(0.1f, state.Rand);
            //finalDy *= RandUtils.DeltaScale(0.1f, state.Rand);



            //float angle = RandUtils.DeltaRange(20, state.Rand);

            //float newRadius = RandUtils.FloatRange(4, 6, state.Rand);

            //List<PointXZ> smallEllipsePoints = _lineGenService.GetRotatedEllipse(cx, cy, finalDx, finalDy, angle);

            //List<PointXZ> largeEllipsePoints = _lineGenService.GetRotatedEllipse(cx, cy, finalDx + newRadius, finalDy + newRadius, angle);

            //foreach (PointXZ pt in largeEllipsePoints)
            //{
            //    state.Block.BumpHeights[pt.X.SafeMod(state.Size), pt.Z.SafeMod(state.Size)] = MaterialGenConstants.RecessedBumpHeight;
            //}
            //foreach (PointXZ pt in smallEllipsePoints)
            //{

            //    state.Block.BumpHeights[pt.X.SafeMod(state.Size), pt.Z.SafeMod(state.Size)] = MaterialGenConstants.DefaultStartBumpHeight;
            //}

            //return true;
        }

        private void AddPointToCurrentBlock(MaterialGenState state, bool[,] isPartOfCurrentBlock, int x, int y, Queue<PointXZ> openList)
        {
            if (x < 0 || x >= state.Width || y < 0 || y >= state.Height)
            {
                return;
            }

            if (isPartOfCurrentBlock[x, y])
            {
                return;
            }

            if (state.Block.BumpHeights[x, y] < MaterialGenConstants.MaxRecessedBumpHeight)
            {
                return;
            }

            isPartOfCurrentBlock[x, y] = true;

            openList.Enqueue(new PointXZ(x - 1, y));
            openList.Enqueue(new PointXZ(x + 1, y));
            openList.Enqueue(new PointXZ(x, y - 1));
            openList.Enqueue(new PointXZ(x, y + 1));

        }


        public bool RemoveBlockFromWall(MaterialGenState state, int cx, int cy)
        {
            bool[,] didChangeStoneShape = new bool[state.Width, state.Height];

            if (state.Rand.NextDouble() > state.Settings.ModifyBlockChance)
            {
                return false;
            }

            Queue<PointXZ> openPoints = new Queue<PointXZ>();

            openPoints.Enqueue(new PointXZ(cx, cy));
            int pointCount = 0;
            while (openPoints.TryDequeue(out PointXZ pt))
            {
                AddPointToCurrentBlock(state, didChangeStoneShape, pt.X, pt.Z, openPoints);
                pointCount++;
            }
            for (int xMain = 0; xMain < state.Width; xMain++)
            {
                for (int yMain = 0; yMain < state.Height; yMain++)
                {
                    if (didChangeStoneShape[xMain, yMain])
                    {
                        state.Block.BumpHeights[xMain, yMain] = MaterialGenConstants.RecessedBumpHeight;
                    }
                }
            }
            return true;
        }

        public void AddCracksToFrontRegions(MaterialGenState state)
        {
            float midCrackCount = state.Settings.CrackDensity * state.Width * state.Height;


            int finalCrackCount = (int)(midCrackCount * RandUtils.DeltaScale(state.Settings.CrackDensity, state.Rand));


            int cracksLeft = finalCrackCount;

            int maxAttempts = 20 * finalCrackCount;


            while (cracksLeft > 0 && --maxAttempts > 0)
            {

                int cx = RandUtils.IntRange(0, state.Width - 1, state.Rand);
                int cy = RandUtils.IntRange(0, state.Height - 1, state.Rand);

                if (state.Block.BumpHeights[cx, cy] < MaterialGenConstants.MaxRecessedBumpHeight)
                {
                    continue;
                }


                cracksLeft--;

                float crackColorScale = 1 + RandUtils.FloatRange(-state.Settings.CrackBrightnessMaxDelta, 0, state.Rand);
                int cracksFromPoint = 1;
                for (int times = 0; times < cracksFromPoint; times++)
                {
                    LineGenParameters lgp = new LineGenParameters()
                    {
                        WidthSizeChangeAmount = 1,
                        InitialNoPosShiftLength = 0,
                        MaxWidthPosDrift = state.Width / 2,
                        LinePathNoiseScale = RandUtils.FloatRange(0, 0.3f, state.Rand),
                        MaxWidthSize = 2,
                        MinWidthSize = 1,
                        Seed = state.Rand.Next(),
                        WidthPosShiftChance = RandUtils.FloatRange(0, 0.1f, state.Rand),
                        WidthPosShiftSize = 1,
                        WidthSizeChangeChance = RandUtils.FloatRange(0, 0.02f, state.Rand),
                        WidthSize = RandUtils.IntRange(1, 3, state.Rand),
                    };


                    int nx = RandUtils.IntRange(0, state.Width - 1, state.Rand);
                    int ny = RandUtils.IntRange(0, state.Height - 1, state.Rand);

                    List<MyPointF> points = _lineGenService.GetBressenhamLine(new MyPoint(cx, cy), new MyPoint(nx, ny), lgp);

                    foreach (MyPointF pt in points)
                    {
                        int x = ((int)pt.X).SafeMod(state.Width);
                        int y = ((int)pt.Y).SafeMod(state.Height);

                        state.Block.Colors[x, y] *= crackColorScale;
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
