using OxDb.SharedCore.Utils.Data;

namespace Assets.Scripts.Crawler.MapGen.RoomGen.Entities
{
    public class EdgeStartEndPoints
    {

        private Point2I _start;
        private Point2I _end;
        public Point2I Start => _start;
        public Point2I End => _end;

        public EdgeStartEndPoints(Point2I start, Point2I end)
        {
            _start = start;
            _end = end;
        }
    }

}
