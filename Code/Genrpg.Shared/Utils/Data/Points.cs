using MessagePack;
namespace Genrpg.Shared.Utils.Data
{
    public class MyPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public MyPoint()
        {

        }
        public MyPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }


    public class PointXZ
    {
        public int X { get; set; }
        public int Z { get; set; }

        public PointXZ()
        {

        }
        public PointXZ(int x, int z)
        {
            X = x;
            Z = z;
        }
    }

    public class MyPoint2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public MyPoint2()
        {

        }

        public MyPoint2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public MyPoint2(MyPointF pt)
        {
            if (pt == null)
            {
                return;
            }

            X = pt.X;
            Y = pt.Y;
        }


    }

    public class MyPointF
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public MyPointF()
        {

        }
        public MyPointF(float x, float y, float z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public MyPointF(MyPointF pt)
        {
            if (pt == null)
            {
                return;
            }

            X = pt.X;
            Y = pt.Y;
            Z = pt.Z;
        }
    }

    public class MyRect
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public MyRect()
        {
        }

        public MyRect(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }
    }

    public class MySize
    {
        public float Width { get; set; }
        public float Height { get; set; }

    }
}


