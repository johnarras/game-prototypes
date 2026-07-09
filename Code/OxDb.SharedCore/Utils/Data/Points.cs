namespace OxDb.SharedCore.Utils.Data
{
   

    /// <summary>
    /// It's weird that it goes XZ and not XY, but this is for mapgen in Unity3d where the flat part of the map is in the XZ plane,
    /// and Z is the up direction, and since this is pretty much always used for mapgen, it's easier to see what coord maps to what
    /// in this way.
    /// </summary>
    public class Point2I
    {
        public int X { get; set; }
        public int Z { get; set; }

        public Point2I()
        {

        }
        public Point2I(int x, int z)
        {
            X = x;
            Z = z;
        }
    }

    /// <summary>
    /// It's weird that it goes XZ and not XY, but this is for mapgen in Unity3d where the flat part of the map is in the XZ plane,
    /// and Z is the up direction, and since this is pretty much always used for mapgen, it's easier to see what coord maps to what
    /// in this way.
    /// </summary>
    public class Point2F
    {
        public float X { get; set; }
        public float Z { get; set; }

        public Point2F()
        {

        }

        public Point2F(float x, float y)
        {
            X = x;
            Z = y;
        }

        public Point2F(Point3F pt)
        {
            if (pt == null)
            {
                return;
            }

            X = pt.X;
            Z = pt.Y;
        }


    }

    public class Point3F
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Point3F()
        {

        }
        public Point3F(float x, float y, float z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3F(Point3F pt)
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

    public class RectF
    {
        public float X { get; set; }
        public float Z { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public RectF()
        {
        }

        public RectF(float x, float z, float w, float h)
        {
            X = x;
            Z = z;
            Width = w;
            Height = h;
        }
    }

    public class SizeF
    {
        public float Width { get; set; }
        public float Height { get; set; }

    }
}


