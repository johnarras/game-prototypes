using OxDb.Client.ProcGen.Materials.Constants;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.ProcGen.Materials.MaterialGenHelpers
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

            float heightDelta = 0.2f;

            List<int> rowZValues = new List<int>();

            int startRowZValue = RandUtils.IntRange(0, state.Height - 1, state.Rand);

            float currRowZValue = startRowZValue;

            float zValuesUsed = 0;

            Dictionary<int, List<int>> rowColumnPoints = new Dictionary<int, List<int>>();

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

                int currZValueInt = (int)currRowZValue;

                rowZValues.Add(currZValueInt % state.Height);

                if (currRowZValue - startRowZValue > state.Height - averageRowHeight * 1.5f)
                {
                    break;
                }
            }

            foreach (int rowValue in rowZValues)
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
            for (int row = 0; row < rowZValues.Count; row++)
            {
                int currZVal = rowZValues[row];
                int nextZVal = rowZValues[(row + 1) % rowZValues.Count];

                List<int> currXValues = rowColumnPoints[currZVal];

                List<int> nextXValues = rowColumnPoints[nextZVal];

                for (int xx = 0; xx < currXValues.Count; xx++)
                {
                    int currXVal = currXValues[xx];

                    int nextXVal = currXValues[(xx + 1) % currXValues.Count];

                    int xmid = MathUtil.LerpInModRange(currXVal, nextXVal, state.Width, 0.5f);
                    int zmid = MathUtil.LerpInModRange(currZVal, nextZVal, state.Height, 0.5f);

                    state.Block.Blocks.Add(new TextureBlock()
                    {
                        CX = MathUtil.ModClamp(xmid, state.Width),
                        CZ = MathUtil.ModClamp(zmid, state.Height),
                        Index = state.Block.GetNextBlockIndex(),
                    });

                    if (!corners.Any(x => x.X == currXVal && x.Z == currZVal))
                    {
                        corners.Add(new CornerPoint(currXVal, currZVal));
                    }
                    if (!corners.Any(x => x.X == currXVal && x.Z == nextZVal))
                    {
                        corners.Add(new CornerPoint(currXVal, nextZVal));
                    }
                }
            }

            int cornerIndex = 0;

            foreach (CornerPoint cp in corners)
            {
                cp.Index = ++cornerIndex;
                cp.OrigX = cp.X;
                cp.OrigZ = cp.Z;
            }

            foreach (CornerPoint cp in corners)
            {
                List<CornerPoint> sameRow = corners.Where(x => x.Z == cp.Z && x.X != cp.X).ToList();

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
                    _logService.Info("No left point for " + cp.X + " " + cp.Z);
                }
                if (rightPoint != null)
                {
                    cp.RightIndex = rightPoint.Index;
                }
                else
                {
                    _logService.Info("No right point for " + cp.X + " " + cp.Z);
                }

                int myRowIndex = rowZValues.IndexOf(cp.Z);

                if (myRowIndex < 0)
                {
                    _logService.Info("Could not find corner point in an existing row");
                    continue;
                }

                int upRowIndex = (myRowIndex + 1 + rowZValues.Count) % rowZValues.Count;

                int downRowIndex = (myRowIndex - 1 + rowZValues.Count) % rowZValues.Count;

                int upYValue = rowZValues[upRowIndex];

                int downYValue = rowZValues[downRowIndex];

                CornerPoint upPoint = corners.FirstOrDefault(x => x.X == cp.X && x.Z == upYValue);

                if (upPoint != null)
                {
                    cp.UpIndex = upPoint.Index;
                }

                CornerPoint downPoint = corners.FirstOrDefault(x => x.X == cp.X && x.Z == downYValue);

                if (downPoint != null)
                {
                    cp.DownIndex = downPoint.Index;
                }

            }

            corners = corners.OrderBy(x => x.X + x.Z * 12345).ToList();

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
                        float dz = otherPoint.OrigZ - cp.OrigZ;

                        double dist = Math.Sqrt(dx * dx + dz * dz);

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
                        cp.Z += (int)RandUtils.DeltaRange(maxPerturb, state.Rand);
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

            foreach (TextureBlock block in state.Block.Blocks)
            {
                _materialGenUtilsService.SetupColorBlockRegion(state, block);

                if (_materialGenUtilsService.MakeBlockVeryRound(state, block))
                {
                    continue;
                }
                if (_materialGenUtilsService.ChangeBrightnessInForegroundRegion(state, block))
                {
                    continue;
                }
                if (_materialGenUtilsService.RemoveBlockFromWall(state, block))
                {
                    continue;
                }
            }

            _materialGenUtilsService.AddCracksToFrontRegions(state);

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
