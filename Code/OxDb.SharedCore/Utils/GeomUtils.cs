using OxDb.SharedCore.LineGen;
using OxDb.SharedCore.Utils.Data;
using System;
using System.Collections.Generic;

namespace OxDb.SharedCore.Utils
{
    /// <summary>
    /// List of geometric utils.
    /// </summary>
    public class GeomUtils
    {
        public const float Epsilon = 0.0001f;

        public const float BadDistance = -1;


        public const float MaxDistance = 100000000;

        /// <summary>
        /// Distance from point to actual segment
        /// </summary>
        /// <param name="ls">Line segment</param>
        /// <param name="px">x val of point</param>
        /// <param name="py">y val of point</param>
        /// <returns>distance or -1 if failure</returns>
        public static float DistanceFromPointToLineSegment(LineSegment ls, float px, float py)
        {
            if (ls == null)
            {
                return BadDistance;
            }

            return DistanceFromPointToLineSegment(ls.SX, ls.SY, ls.EX, ls.EY, px, py);
        }

        /// <summary>
        /// Get the distnace from point (px,py) to line segment (ex,ey),(sx,sy)
        /// </summary>
        /// <param name="sx">Start x</param>
        /// <param name="sy">Start y</param>
        /// <param name="ex">End x</param>
        /// <param name="ey">End y</param>
        /// <param name="px">Point x off line</param>
        /// <param name="pz">Point y off line</param>
        /// <returns>distance</returns>
        /// 
        public static float DistanceFromPointToLineSegment(float sx, float sy, float ex, float ey, float px, float pz)
        {
            float ly = ey - sy;
            float lx = ex - sx;

            float segmentLength = lx * lx + ly * ly;
            if (Math.Sqrt(segmentLength) < Epsilon)
            {
                return (float)Math.Sqrt((px - ex) * (px - ex) + (pz - ey) * (pz - ey));
            }

            float u = ((px - sx) * lx + (pz - sy) * ly) / segmentLength;

            u = MathUtil.Clamp(0, u, 1);

            float x = sx + u * lx;
            float z = sy + u * ly;

            float dx = x - px;
            float dz = z - pz;

            return (float)Math.Sqrt(dx * dx + dz * dz);

        }

        public static float DistanceFromPointToPolyLine<T>(List<T> list, float px, float py) where T : LineSegment
        {
            if (list == null || list.Count < 1)
            {
                return BadDistance;
            }

            float minDist = BadDistance;
            for (int l = 0; l < list.Count; l++)
            {
                float newDist = DistanceFromPointToLineSegment(list[l], px, py);
                if (minDist < 0 || newDist < minDist)
                {
                    minDist = newDist;
                }
            }

            return minDist;

        }

        public static Point2F GetClosestPoint2(List<Point2F> points, Point2F newPoint, double p = 2)
        {
            if (points == null || points.Count < 1 || newPoint == null)
            {
                return null;
            }

            double minDist = MaxDistance;
            Point2F closestPoint = null;
            foreach (Point2F pt in points)
            {
                double dist = MathUtil.LPNorm(p, pt.X - newPoint.X, pt.Z - newPoint.Z);

                if (dist < minDist)
                {
                    minDist = dist;
                    closestPoint = pt;
                }
            }

            return closestPoint;
        }




        public static double GetMinDistance2(List<Point2F> points, Point2F newPoint, double p = 2)
        {
            Point2F closestPt = GetClosestPoint2(points, newPoint, p);
            if (closestPt == null || newPoint == null)
            {
                return MaxDistance;
            }

            return MathUtil.LPNorm(p, closestPt.X - newPoint.X, closestPt.Z - newPoint.Z);
        }



        public static Point2I GetClosestPoint2(List<Point2I> points, Point2I newPoint, double p = 2)
        {
            if (points == null || points.Count < 1 || newPoint == null)
            {
                return null;
            }

            double minDist = MaxDistance;
            Point2I closestPoint = null;
            foreach (Point2I pt in points)
            {
                double dist = MathUtil.LPNorm(p, pt.X - newPoint.X, pt.Z - newPoint.Z);

                if (dist < minDist)
                {
                    minDist = dist;
                    closestPoint = pt;
                }
            }

            return closestPoint;
        }




        public static double GetMinDistance2(List<Point2I> points, Point2I newPoint, double p = 2)
        {
            Point2I closestPt = GetClosestPoint2(points, newPoint, p);
            if (closestPt == null || newPoint == null)
            {
                return MaxDistance;
            }

            return MathUtil.LPNorm(p, closestPt.X - newPoint.X, closestPt.Z - newPoint.Z);
        }
    }
}


