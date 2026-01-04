namespace Genrpg.Shared.Trader.Caravans.Entities
{
    public class CaravanPosition
    {
        public long RoadId { get; set; }
        public long CityId { get; set; }
        public long DistanceTravelled { get; set; }
        public long TargetCityId { get; set; }
        public long OutsideOfCityId { get; set; }

        public bool OnRoad()
        {
            return RoadId > 0;
        }
    }
}


