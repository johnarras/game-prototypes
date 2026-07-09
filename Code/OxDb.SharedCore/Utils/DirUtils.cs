using OxDb.SharedCore.Utils.Data;

namespace OxDb.SharedCore.Utils
{
    public class DirUtils
    {
        // Used with client with xz coordinates.
        public static int DirDeltaToAngle(int dx, int dy)
        {
            if (dy > 0)
            {
                return 0;
            }
            else if (dy < 0)
            {
                return 180;
            }
            else if (dx > 0)
            {
                return 90;
            }
            else if (dx < 0)
            {
                return 270;
            }
            return 0;
        }

        public static Point2I AxisAngleToDirDelta(int axisAngle)
        {
            if (axisAngle == 0)
            {
                return new Point2I(0, 1);
            }
            else if (axisAngle == 180)
            {
                return new Point2I(0, -1);
            }
            else if (axisAngle == 90)
            {
                return new Point2I(1, 0);
            }
            else if (axisAngle == 270)
            {
                return new Point2I(-1, 0);
            }
            return null;
        }
    }
}


