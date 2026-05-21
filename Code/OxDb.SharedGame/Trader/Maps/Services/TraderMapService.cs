using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.Trader.CurrencySpend.Settings;
using OxDb.SharedGame.Trader.Holdings.PlayerData;
using OxDb.SharedGame.Trader.Travel.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.Maps.Services
{
    public class CityTravelDistance
    {
        public double Distance { get; set; }
        public City City { get; set; }

        public SpendType PortalSpend { get; set; }
    }

    public interface ITraderMapService : IInjectable
    {
        MyPointF GetMapCoordinate(long fromX, long fromY, long toX, long toY, double distanceGone, double totalDistance);

        float GetAngle(long fromX, long fromY, long toX, long toY);


        Task<int> GetDistanceBetweenPoints(IUnitDataLookup lookup, long x, long y, long toX, long toY);

        Task<List<CityTravelDistance>> GetNearbyCities(IUnitDataLookup lookup);


    }

    public class TraderMapService : ITraderMapService
    {

        private ICaravanService _caravanService = null;
        private IGameData _gameData = null;


        public async Task<int> GetDistanceBetweenPoints(IUnitDataLookup lookup, long x, long y, long toX, long toY)
        {
            long dx = x - toX;
            long dy = y - toY;

            CoreData coreData = await lookup.GetAsync<CoreData>();

            TravelSettings settings = _gameData.Get<TravelSettings>(coreData);

            return (int)(Math.Sqrt(dx * dx + dy * dy) * settings.DistancePerMapUnit);
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

        public async Task<List<CityTravelDistance>> GetNearbyCities(IUnitDataLookup lookup)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();

            CaravanPosition pos = _caravanService.GetPosition(coreData);

            IReadOnlyList<City> allCities = _gameData.Get<CitySettings>(coreData).GetData();

            List<CityTravelDistance> distances = new List<CityTravelDistance>();

            TravelSettings travelSettings = _gameData.Get<TravelSettings>(coreData);

            foreach (City city in allCities)
            {
                if (city.IdKey == pos.GetTargetCityId())
                {
                    continue;
                }

                long distanceToCity = await GetDistanceBetweenPoints(lookup, pos.CurrX, pos.CurrY, city.MapPixelX, city.MapPixelY);

                if (distanceToCity == 0 || distanceToCity > travelSettings.MaxDistanceToTarget)
                {
                    continue;
                }

                distances.Add(new CityTravelDistance()
                {
                    City = city,
                    Distance = distanceToCity,
                });
            }

            distances = distances.OrderBy(x => x.Distance).ToList();

            List<CityTravelDistance> forcedDistances = distances.Where(x => x.Distance < travelSettings.MaxDistanceToTarget / 3).ToList();

            List<CityTravelDistance> otherDistances = distances.Except(forcedDistances).ToList();

            distances = forcedDistances;

            while (distances.Count < travelSettings.MaxNearbyCitiesShown && otherDistances.Count > 0)
            {
                distances.Add(otherDistances[0]);
                otherDistances.RemoveAt(0);
            }

            if (travelSettings.PortalDistancePerMana > 0 && travelSettings.MinPortalCost > 0)
            {
                HoldingsData holdingData = await lookup.GetAsync<HoldingsData>();

                foreach (CityTravelDistance dist in distances)
                {
                    if (holdingData.CitiesVisited.HasBitIndex(dist.City.IdKey))
                    {
                        SpendType stype = new SpendType()
                        {
                            Name = "Portal",
                            SpendCoreCurrencyTypeId = CoreCurrencyTypes.Mana,
                            SpendQuantity = (int)Math.Max(travelSettings.MinPortalCost, dist.Distance / travelSettings.PortalDistancePerMana),
                        };

                        SpendReward rew = new SpendReward()
                        {
                            EntityTypeId = EntityTypes.City,
                            EntityId = dist.City.IdKey,
                            Quantity = 1,
                        };

                        stype.Rewards.Add(rew);

                        dist.PortalSpend = stype;

                    }
                }
            }

            return distances;
        }
    }
}
