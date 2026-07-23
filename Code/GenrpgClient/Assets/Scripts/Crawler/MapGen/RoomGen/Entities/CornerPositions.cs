using OxDb.SharedCore.Utils.Data;
using System;

namespace OxDb.Client.Crawler.MapGen.RoomGen.Entities
{
    public class CornerPositions
    {
        public Point2I UpperLeft => new Point2I(_xmin, _zmax);
        public Point2I UpperRight => new Point2I(_xmax, _zmax);
        public Point2I LowerLeft => new Point2I(_xmin, _zmin);
        public Point2I LowerRight => new Point2I(_xmax, _zmin);

        private int _xmin;
        private int _xmax;
        private int _zmin;
        private int _zmax;

        public CornerPositions(int xmin, int zmin, int xmax, int zmax)
        {
            _xmin = xmin;
            _zmin = zmin;
            _xmax = xmax;
            _zmax = zmax;
        }

        // These should always go in increasing axis order for the pair.
        public EdgeStartEndPoints GetStartEndPoints(int dx, int dz)
        {
            if (dx < 0 && dz == 0) // Left wall
            {
                return new EdgeStartEndPoints(LowerLeft, UpperLeft);
            }
            else if (dx > 0 && dz == 0) // Right wall
            {
                return new EdgeStartEndPoints(LowerRight, UpperRight);
            }
            else if (dz > 0 && dx == 0) // Top
            {
                return new EdgeStartEndPoints(UpperLeft, UpperRight);
            }
            else if (dz < 0 && dx == 0)
            {
                return new EdgeStartEndPoints(LowerLeft, LowerRight);
            }
            else
            {
                throw new Exception("Start End Points require exactly one of dx and dy to be +/-1 and one to be zero.");
            }
        }
    }
}
