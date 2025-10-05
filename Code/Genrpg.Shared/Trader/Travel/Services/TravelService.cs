using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Roads.Settings;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Trader.Travel.Services
{
    public class CityPath
    {
        public City City { get; set; }
        public double TotalDistance = 100000000;
        public CityPath PrevCityPath { get; set; } = null;
        public Road Road { get; set; } = null;
    }

    public interface ITravelService : IInjectable
    {
        List<CityPath> GetPathFrom(IFilteredObject obj, long startCityId, long endCityId);

        double GetDistanceBetween(IFilteredObject obj, long startCityId, long endCityId);
    }


    public class TravelService : ITravelService
    {
        private IGameData _gameData = null;


        public double GetDistanceBetween(IFilteredObject obj, long startCityId, long endCityId)
        {
            List<CityPath> path = GetPathFrom(obj, startCityId, endCityId);

            if (path != null && path.Count > 0)
            {
                return path.Last().TotalDistance;
            }

            return 10000000;
        }

        /// <summary>
        /// Dijkstras
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="startCityId"></param>
        /// <param name="endCityId"></param>
        /// <returns></returns>
        public List<CityPath> GetPathFrom(IFilteredObject obj, long startCityId, long endCityId)
        {
            IReadOnlyList<City> allCities = _gameData.Get<CitySettings>(obj).GetData();
            IReadOnlyList<Road> allRoads = _gameData.Get<RoadSettings>(obj).GetData();

            Dictionary<long, CityPath> distances = new Dictionary<long, CityPath>();

            foreach (City city in allCities)
            {
                distances[city.IdKey] = new CityPath() { City = city, TotalDistance = 100000000 };
            }

            distances[startCityId].TotalDistance = 0;

            List<CityPath> distanceQueue = new List<CityPath>();

            distanceQueue.Add(distances[startCityId]);

            CityPath finalDistance = null;
            while (distanceQueue.Count > 0)
            {
                if (finalDistance != null)
                {
                    break;
                }
                CityPath lastDistance = distanceQueue.Last();

                distanceQueue.Remove(lastDistance);

                bool addedSomething = false;
                foreach (CityRoad cityRoad in lastDistance.City.Roads)
                {
                    CityPath otherDistance = distances[cityRoad.OtherCityId];

                    Road road = allRoads.FirstOrDefault(x => x.IdKey == cityRoad.RoadId);

                    City otherCity = allCities.FirstOrDefault(x => x.IdKey == cityRoad.OtherCityId);

                    double totalDistance = lastDistance.TotalDistance + road.Distance;

                    // If this is farther away than the dist, then ignore it.
                    if (totalDistance > otherDistance.TotalDistance)
                    {
                        continue;
                    }

                    otherDistance.Road = road;
                    otherDistance.PrevCityPath = lastDistance;
                    otherDistance.TotalDistance = totalDistance;

                    if (cityRoad.OtherCityId == endCityId)
                    {
                        finalDistance = otherDistance;
                        break;
                    }
                    else
                    {
                        distanceQueue.Add(otherDistance);
                        addedSomething = true;
                    }
                }

                if (addedSomething)
                {
                    distanceQueue = distanceQueue.OrderByDescending(x => x.TotalDistance).ToList();
                }
            }

            if (finalDistance == null)
            {
                return null;
            }

            List<CityPath> retval = new List<CityPath>();

            CityPath prevPath = finalDistance;

            while (prevPath != null)
            {
                retval.Add(prevPath);
                prevPath = prevPath.PrevCityPath;
            }

            retval.Reverse();
            return retval;
        }
    }
}
