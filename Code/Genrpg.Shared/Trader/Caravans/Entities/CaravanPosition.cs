using Genrpg.Shared.Trader.Cities.Settings;

namespace Genrpg.Shared.Trader.Caravans.Entities
{
    public class CaravanPosition
    {
        public int FromX { get; set; }
        public int FromY { get; set; }
        public int ToX { get; set; }
        public int ToY { get; set; }
        public int CurrX { get; set; }
        public int CurrY { get; set; }
        public int DistanceToTarget { get; set; }
        public int DistanceGone { get; set; }
        public City TargetCity { get; set; }
        public float Angle { get; set; }

        public City GetCurrentCity()
        {
            if (TargetCity == null)
            {
                return null;
            }
            if (CurrX == TargetCity.MapPixelX && CurrY == TargetCity.MapPixelY)
            {
                return TargetCity;
            }
            return null;
        }

        public int GetTargetCityId()
        {
            return (int)(TargetCity?.IdKey ?? 0);
        }

        public bool OnRoad()
        {
            return (FromX != ToX || FromY != ToY);
        }
    }
}


