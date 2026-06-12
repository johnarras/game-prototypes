using Assets.Scripts.ProcGen.Materials.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials.MaterialGenHelpers
{
    public class BlocksMaterialGenHelper : BaseMaterialGenHelper
    {
        public override EMaterialGenTypes HelperKey => EMaterialGenTypes.Blocks;

        protected virtual void TweakStateValues(MaterialGenState state)
        {

        }

        public override async Awaitable<Texture2D> GenerateTexture(MaterialGenState state)
        {
            Texture2D tex = CreateTexture(state.Width, state.Height);
            state.Block = new MaterialGenBlock(state.Width, state.Height, state.ForegroundMain, MaterialGenConstants.DefaultStartBrightness, MaterialGenConstants.DefaultStartBumpHeight);

            TweakStateValues(state);

            int rowCount = state.BlockRowCount;


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

            Dictionary<int, List<int>> rowColumnPoints = new Dictionary<int, List<int>>();

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

            foreach (int rowValue in rowYValues)
            {
                rowColumnPoints[rowValue] = new List<int>();

                int startBrickEndX = RandUtils.IntRange(0, state.Width - 1, state.Rand);

                float minBrickWidth = state.Settings.MinBrickAspectRatio * averageRowHeight;
                float maxBrickWidth = state.Settings.MaxBrickAspectRatio * averageRowHeight;

                minBrickWidth *= RandUtils.DeltaScale(state.Settings.BrickAspectRatioDelta, state.Rand);
                maxBrickWidth *= RandUtils.DeltaScale(state.Settings.BrickAspectRatioDelta, state.Rand);

                if (minBrickWidth < 4)
                {
                    minBrickWidth = 4;
                }
                if (maxBrickWidth < minBrickWidth + 2)
                {
                    maxBrickWidth = minBrickWidth + 2;
                }

                float currBrickEndX = startBrickEndX;

                List<int> brickEndXValues = new List<int>();

                brickEndXValues.Add(startBrickEndX);

                float xValuesUsed = 0;

                while (true)
                {
                    float xValueSkip = RandUtils.FloatRange(minBrickWidth, maxBrickWidth, state.Rand);

                    float maxXSkip = (state.Width - xValuesUsed) / 2;

                    if (xValueSkip > maxXSkip)
                    {
                        xValueSkip = maxXSkip;
                    }

                    currBrickEndX += xValueSkip;

                    xValuesUsed += xValueSkip;

                    int brickEndInt = ((int)currBrickEndX) % state.Width;

                    brickEndXValues.Add(brickEndInt);

                    if (currBrickEndX - startBrickEndX > state.Width - maxBrickWidth * 1.25f)
                    {
                        break;
                    }
                }
                rowColumnPoints[rowValue] = brickEndXValues;
            }

            List<CornerPoint> corners = new List<CornerPoint>();

            // Now iterate over all rows and prepare to be able to move points, once we have all points listed.
            for (int row = 0; row < rowYValues.Count; row++)
            {
                int currYVal = rowYValues[row];
                int nextYVal = rowYValues[(row + 1) % rowYValues.Count];

                List<int> currXValues = rowColumnPoints[currYVal];

                List<int> nextXValues = rowColumnPoints[nextYVal];

                for (int xx = 0; xx < currXValues.Count; xx++)
                {
                    int currXVal = currXValues[xx];

                    int nextXVal = currXValues[(xx + 1) % currXValues.Count];

                    int xmid = MathUtil.LerpInModRange(currXVal, nextXVal, state.Width, 0.5f);
                    int ymid = MathUtil.LerpInModRange(currYVal, nextYVal, state.Height, 0.5f);

                    blockCenters.Add(new PointXZ(xmid, ymid));

                    if (!corners.Any(x => x.X == currXVal && x.Y == currYVal))
                    {
                        corners.Add(new CornerPoint(currXVal, currYVal));
                    }
                    if (!corners.Any(x => x.X == currXVal && x.Y == nextYVal))
                    {
                        corners.Add(new CornerPoint(currXVal, nextYVal));
                    }
                }
            }

            int cornerIndex = 0;

            foreach (CornerPoint cp in corners)
            {
                cp.Index = ++cornerIndex;
                cp.OrigX = cp.X;
                cp.OrigY = cp.Y;
            }

            foreach (CornerPoint cp in corners)
            {
                List<CornerPoint> sameRow = corners.Where(x => x.Y == cp.Y && x.X != cp.X).ToList();

                sameRow = sameRow.OrderBy(x => x.X).ToList();

                List<CornerPoint> leftPoints = sameRow.Where(x => x.X < cp.X).ToList();

                List<CornerPoint> rightPoints = sameRow.Where(x => x.X > cp.X).ToList();

                CornerPoint leftPoint = null;

                CornerPoint rightPoint = null;

                if (leftPoints.Count > 0)
                {
                    leftPoint = leftPoints.Last();
                }
                else
                {
                    leftPoint = rightPoints.Last();
                }

                if (rightPoints.Count > 0)
                {
                    rightPoint = rightPoints.First();
                }
                else
                {
                    rightPoint = leftPoints.First();
                }

                if (leftPoint != null)
                {
                    cp.LeftIndex = leftPoint.Index;
                }
                else
                {
                    _logService.Info("No left point for " + cp.X + " " + cp.Y);
                }
                if (rightPoint != null)
                {
                    cp.RightIndex = rightPoint.Index;
                }
                else
                {
                    _logService.Info("No right point for " + cp.X + " " + cp.Y);
                }

                int myRowIndex = rowYValues.IndexOf(cp.Y);

                if (myRowIndex < 0)
                {
                    _logService.Info("Could not find corner point in an existing row");
                    continue;
                }

                int upRowIndex = (myRowIndex + 1 + rowYValues.Count) % rowYValues.Count;

                int downRowIndex = (myRowIndex - 1 + rowYValues.Count) % rowYValues.Count;

                int upYValue = rowYValues[upRowIndex];

                int downYValue = rowYValues[downRowIndex];

                CornerPoint upPoint = corners.FirstOrDefault(x => x.X == cp.X && x.Y == upYValue);

                if (upPoint != null)
                {
                    cp.UpIndex = upPoint.Index;
                }

                CornerPoint downPoint = corners.FirstOrDefault(x => x.X == cp.X && x.Y == downYValue);

                if (downPoint != null)
                {
                    cp.DownIndex = downPoint.Index;
                }

            }

            corners = corners.OrderBy(x => x.X + x.Y * 12345).ToList();

            List<CornerPoint> cornerCopy = new List<CornerPoint>(corners);
            foreach (CornerPoint cp in cornerCopy)
            {
                if (cp.WasPerturbed)
                {
                    continue;
                }
                List<int> indexesToCheck = new List<int>();
                indexesToCheck.Add(cp.LeftIndex);
                indexesToCheck.Add(cp.RightIndex);
                indexesToCheck.Add(cp.UpIndex);
                indexesToCheck.Add(cp.DownIndex);

                double minDist = 0;

                foreach (int index in indexesToCheck)
                {
                    CornerPoint otherPoint = corners.FirstOrDefault(x => x.Index == index);

                    if (otherPoint != null)
                    {
                        float dx = otherPoint.OrigX - cp.OrigX;
                        float dy = otherPoint.OrigY - cp.OrigY;

                        double dist = Math.Sqrt(dx * dx + dy * dy);

                        if (minDist == 0 || dist < minDist)
                        {
                            minDist = dist;
                        }
                    }
                }

                if (minDist > state.RoundCornerMaxSize && state.Rand.NextDouble() < state.CornerPerturbChance)
                {
                    double maxPerturb = minDist * state.MaxCornerPerturbScale;

                    bool didVerticalPerturb = false;
                    if (state.Rand.NextDouble() < state.VerticalPerturbChance)
                    {
                        didVerticalPerturb = _materialGenUtilsService.TryVerticalPerturb(state, corners, cp, maxPerturb);
                    }
                    if (!didVerticalPerturb)
                    {
                        cp.X += (int)RandUtils.DeltaRange(maxPerturb, state.Rand);
                        cp.Y += (int)RandUtils.DeltaRange(maxPerturb, state.Rand);
                        cp.WasPerturbed = true;
                    }
                }
            }

            foreach (CornerPoint currentPoint in corners)
            {
                CornerPoint rightPoint = corners.FirstOrDefault(x => x.Index == currentPoint.RightIndex);

                _materialGenUtilsService.ConnectLowerLeftToUpperRightPoint(state, currentPoint, rightPoint, MaterialGenConstants.RecessedBumpHeight, 2, 5);
                CornerPoint upPoint = corners.FirstOrDefault(x => x.Index == currentPoint.UpIndex);

                _materialGenUtilsService.ConnectLowerLeftToUpperRightPoint(state, currentPoint, upPoint, MaterialGenConstants.RecessedBumpHeight, 2, 5);

            }

            foreach (CornerPoint currentPoint in corners)
            {
                _materialGenUtilsService.RoundCreviceCorner(state, corners, currentPoint, currentPoint.RightIndex, currentPoint.UpIndex);
                _materialGenUtilsService.RoundCreviceCorner(state, corners, currentPoint, currentPoint.UpIndex, currentPoint.LeftIndex);
                _materialGenUtilsService.RoundCreviceCorner(state, corners, currentPoint, currentPoint.LeftIndex, currentPoint.DownIndex);
                _materialGenUtilsService.RoundCreviceCorner(state, corners, currentPoint, currentPoint.DownIndex, currentPoint.RightIndex);

            }

            foreach (PointXZ blockCenter in blockCenters)
            {
                if (_materialGenUtilsService.MakeBlockVeryRound(state, blockCenter.X, blockCenter.Z))
                {
                    continue;
                }
                if (_materialGenUtilsService.ChangeBrightnessInForegroundRegion(state, blockCenter.X, blockCenter.Z))
                {
                    continue;
                }
                if (_materialGenUtilsService.RemoveBlockFromWall(state, blockCenter.X, blockCenter.Z))
                {
                    continue;
                }

            }
            //_materialGenUtilsService.AddCracksToFrontRegions(state);

            _materialGenUtilsService.RoundEdgesNearCrevices(state);

            _materialGenUtilsService.AddColorNoise(state);

            _materialGenUtilsService.ApplyRecessedColors(state);

            _materialGenUtilsService.RandomizeBumpHeights(state);

            _materialGenUtilsService.SmoothColors(state);

            _materialGenUtilsService.ApplyBlockToTexture(state, state.Block, tex);

            await Task.CompletedTask;
            return tex;
        }
    }
}
