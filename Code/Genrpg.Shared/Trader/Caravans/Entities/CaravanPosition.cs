using Genrpg.Shared.Trader.Cities.Settings;

namespace Genrpg.Shared.Trader.Caravans.Entities
{
    public class CaravanPosition
    {
        public long FromX { get; set; }
        public long FromY { get; set; }
        public long ToX { get; set; }
        public long ToY { get; set; }
        public long CurrX { get; set; }
        public long CurrY { get; set; }
        public long DistanceToTarget { get; set; }
        public long DistanceGone { get; set; }
        public City TargetCity { get; set; }
        public float Angle { get; set; }

        public City GetCurrentCity()
        {
            return (!OnRoad() ? TargetCity : null);
        }

        public long GetTargetCityId()
        {
            return TargetCity?.IdKey ?? 0;
        }




        public bool OnRoad()
        {
            return (FromX != ToX || FromY != ToY);
        }
    }
}


