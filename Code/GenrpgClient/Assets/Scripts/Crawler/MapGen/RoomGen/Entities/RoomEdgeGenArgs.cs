using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.MapGen.Settings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Crawler.MapGen.RoomGen.Entities
{
    public class RoomEdgeGenArgs
    {
        public List<Point2I> StartPoints { get; set; } = new List<Point2I>();

        public int DX { get; set; }
        public int DZ { get; set; }

        public Point2I Start { get; set; }
        public Point2I End { get; set; }

        public List<Point2I> OverrideEdgePoints { get; set; }

        public int Seed { get; set; }

        public int RoomId { get; set; }

        public CornerPositions Corners { get; set; }

        public RoomEdgeType EdgeType { get; set; }

        public float DepthRatio { get; set; }

        public bool Narrow { get; set; }

        public bool LeftOffset { get; set; }

        public bool RightOffset { get; set; }

        public float EndDoorChance { get; set; }

        public float MissingChance { get; set; }

        public object ExtraData { get; set; }

        public int GetMaxLength()
        {
            List<Point2I> points = GetEdgePoints();

            return (int)Math.Ceiling(points.Count * DepthRatio / 2.0f);
        }

        public void SetSecondaryData(int seed, float depthRatio, bool narrow, bool leftOffset, bool rightOffset, float endDoorChance, float missingChance)
        {
            Seed = seed;
            DepthRatio = depthRatio;
            Narrow = narrow;
            LeftOffset = leftOffset;
            RightOffset = rightOffset;
            EndDoorChance = endDoorChance;
            MissingChance = missingChance;
        }

        public RoomEdgeGenArgs(int dx, int dz, int roomId, CornerPositions positions)
        {
            DX = dx;
            DZ = dz;
            RoomId = roomId;
            Corners = positions;
            EdgeStartEndPoints startEnd = positions.GetStartEndPoints(dx, dz);
            Start = startEnd.Start;
            End = startEnd.End;
        }

        public float GetHalfIndex()
        {
            return GetEdgePoints().Count / 2.0f;
        }


        private List<Point2I> _edgePointCache = null;
        public List<Point2I> GetEdgePoints()
        {
            if (_edgePointCache != null)
            {
                return _edgePointCache;
            }

            if (OverrideEdgePoints != null && OverrideEdgePoints.Count > 0)
            {
                List<Point2I> overrideCopy = new List<Point2I>(OverrideEdgePoints);

                if (Narrow && overrideCopy.Count > 1)
                {
                    overrideCopy.RemoveAt(0);
                    overrideCopy.RemoveAt(overrideCopy.Count - 1);
                }
                _edgePointCache = overrideCopy;
                return overrideCopy;
            }

            List<Point2I> points = new List<Point2I>();

            int dx = Math.Sign(End.X - Start.X);
            int dz = Math.Sign(End.Z - Start.Z);

            if (dx != 0)
            {
                int x = Start.X;
                bool reachedEnd = false;
                while (true)
                {
                    points.Add(new Point2I(x, Start.Z));
                    x += dx;

                    if (reachedEnd)
                    {
                        break;
                    }

                    if (x == End.X)
                    {
                        reachedEnd = true;
                    }
                }
            }

            if (dz != 0)
            {
                int z = Start.Z;
                bool reachedEnd = false;
                while (true)
                {
                    points.Add(new Point2I(Start.X, z));
                    z += dz;

                    if (reachedEnd)
                    {
                        break;
                    }

                    if (z == End.Z)
                    {
                        reachedEnd = true;
                    }
                }
            }

            if (Narrow)
            {
                if (points.Count > 1)
                {
                    points.RemoveAt(0);
                    points.RemoveAt(points.Count - 1);
                }
            }

            _edgePointCache = points;
            return points;
        }


        public EdgePositionPercents GetPositionPercents(int p)
        {

            int len = GetEdgePoints().Count;

            int lenDiv = Math.Max(len - 1, 1);

            int maxDepth = GetMaxLength();

            float halfIndex = GetHalfIndex();

            int maxDistance = Math.Max(1, (int)halfIndex);

            float midDistance = (int)Math.Abs(p - halfIndex);

            float midPct = Mathf.Abs((p - halfIndex) / maxDistance);

            float leftEdgeDistPct = 1.0f * p / lenDiv;
            float rightEdgeDistPct = 1.0f * (len - 1 - p) / lenDiv;

            float finalPercent = midPct;

            if (LeftOffset)
            {
                finalPercent = leftEdgeDistPct;
            }
            else if (RightOffset)
            {
                finalPercent = rightEdgeDistPct;
            }
            else
            {
                finalPercent = midPct;
            }

            EdgePositionPercents percents = new EdgePositionPercents(midPct, leftEdgeDistPct, rightEdgeDistPct, finalPercent);

            return percents;
        }
    }
}

