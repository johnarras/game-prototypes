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
        public int TotalDistanceToTarget { get; set; }
        public int DistanceGone { get; set; }
        public City TargetCity { get; set; }
        public City PositionCity { get; set; }
        public float Angle { get; set; }

        public bool IsTravelling()
        {
            return GetCurrentCity() == null || TargetCity != PositionCity;
        }


        public City GetCurrentCity()
        {
            if (TargetCity == null)
            {
                return null;
            }

            if (TotalDistanceToTarget < 1 || DistanceGone >= TotalDistanceToTarget)
            {
                return TargetCity;
            }

            if (PositionCity != null)
            {
                return PositionCity;
            }

            return null;
        }

        public City GetTargetCity()
        {
            return TargetCity;
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


