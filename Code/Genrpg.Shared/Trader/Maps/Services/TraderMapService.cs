using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Travel.Settings;
using Genrpg.Shared.Utils.Data;
using System;

namespace Genrpg.Shared.Trader.Maps.Services
{
    public interface ITraderMapService : IInjectable
    {
        MyPointF GetMapCoordinate(long fromX, long fromY, long toX, long toY, double distanceGone, double totalDistance);

        float GetAngle(long fromX, long fromY, long toX, long toY);


        long GetDistanceBetweenPoints(TravelSettings settings, long x, long y, long toX, long toY);
    }

    public class TraderMapService : ITraderMapService
    {
        public long GetDistanceBetweenPoints(TravelSettings settings, long x, long y, long toX, long toY)
        {
            long dx = x - toX;
            long dy = y - toY;

            return (long)(Math.Sqrt(dx * dx + dy * dy) * settings.DistancePerMapUnit);
        }
        public MyPointF GetMapCoordinate(long fromX, long fromY, long toX, long toY, double distanceGone, double totalDistance)
        {
            if (totalDistance < 1)
            {
                return new MyPointF(toX, toY);
            }

            double pctGone = 1.0 * distanceGone / totalDistance;

            double x = fromX * (1 - pctGone) + toX * pctGone;
            double y = fromY * (1 - pctGone) + toY * pctGone;

            return new MyPointF((float)x, (float)y);

        }

        public float GetAngle(long fromX, long fromY, long toX, long toY)
        {
            if (fromX != toX || fromY != toY)
            {
                float dx = toX - fromX;
                float dy = toY - fromY;

                return (float)(Math.Atan2(dy, dx) * 180.0f / Math.PI);
            }
            return 0;
        }

    }
}
