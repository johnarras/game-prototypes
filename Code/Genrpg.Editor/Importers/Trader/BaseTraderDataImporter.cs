using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Maps.Services;
using Genrpg.Shared.Trader.TradeEconomy.Settings;
using Genrpg.Shared.Trader.TradeGoods.Settings;
using Genrpg.Shared.Trader.Travel.Services;
using Genrpg.Shared.Trader.Travel.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers.Trader
{
    public abstract class BaseTraderDataImporter<TParent, TChild> : ParentChildImporter<TParent, TChild> where TParent : ParentSettings<TChild>, new() where TChild : ChildSettings, IIdName, new()
    {

        protected ITravelService _travelService = null;
        protected ICaravanService _caravanService = null;
        protected ITraderMapService _traderMapService = null;

        protected override async Task<bool> UpdateAfterImport(WindowBase win, EditorGameState gs)
        {

            IReadOnlyList<City> allCities = gs.data.Get<CitySettings>(null).GetData();

            IReadOnlyList<TradeGood> allTradeGoods = gs.data.Get<TradeGoodSettings>(null).GetData();

            CalculateTradeGoodCosts(gs, allCities, allTradeGoods);

            foreach (City city in allCities)
            {
                if (!gs.LookedAtObjects.Contains(city))
                {
                    gs.LookedAtObjects.Add(city);
                }
            }

            foreach (TradeGood tradeGood in allTradeGoods)
            {
                if (!gs.LookedAtObjects.Contains(tradeGood))
                {
                    gs.LookedAtObjects.Add(tradeGood);
                }
            }

            await Task.CompletedTask;
            return true;
        }

        private void CalculateTradeGoodCosts(EditorGameState gs, IReadOnlyList<City> cities, IReadOnlyList<TradeGood> tradeGoods)
        {

            TradeEconomySettings econ = gs.data.Get<TradeEconomySettings>(null);

            foreach (City city in cities)
            {
                city.TradeGoodBuyCosts.Clear();
            }


            foreach (TradeGood tradeGood in tradeGoods)
            {
                tradeGood.ProducerCities.Clear();
                List<City> producerCities = new List<City>();

                List<City> consumerCities = new List<City>();
                foreach (City city in cities)
                {
                    if (city.TradeGoodsProduced.Any(x => x.TradeGoodId == tradeGood.IdKey))
                    {
                        producerCities.Add(city);
                        tradeGood.ProducerCities.Add(new TradeGoodProducerCity() { CityId = city.IdKey });
                    }
                    else
                    {
                        consumerCities.Add(city);
                    }
                }

                if (producerCities.Count < 1)
                {
                    _logService.Error("No cities produce " + tradeGood.Name);

                }
                else
                {
                    long populationSum = producerCities.Sum(x => x.Population);

                    if (populationSum > 0)
                    {
                        foreach (City city in producerCities)
                        {

                            CityTradeGood tg = city.TradeGoodsProduced.FirstOrDefault(x => x.TradeGoodId == tradeGood.IdKey);
                            double popPct = city.Population * 1.0 / populationSum;

                            double priceScale = econ.SmallProducerPriceScale * (1 - popPct) +
                                econ.BigProducerPriceScale * (popPct);

                            tg.PriceScale = priceScale;

                            city.TradeGoodBuyCosts[tradeGood.IdKey] = (long)(tradeGood.Price * priceScale);
                            tradeGood.CityBuyCosts[city.IdKey] = city.TradeGoodBuyCosts[tradeGood.IdKey];
                        }
                    }
                }

                foreach (City city in consumerCities)
                {
                    double minDistanceToProducer = 100000000;

                    foreach (City producerCity in producerCities)
                    {
                        double dist = _traderMapService.GetDistanceBetweenPoints(gs.data.Get<TravelSettings>(null), city.MapPixelX, city.MapPixelY, producerCity.MapPixelX, producerCity.MapPixelY);


                        if (dist < minDistanceToProducer)
                        {
                            minDistanceToProducer = dist;
                        }
                    }

                    double distancePct = Math.Min(1, minDistanceToProducer / econ.MaxCostDistance);

                    double costScale = econ.MinConsumerPriceScale * (1 - distancePct) + econ.MaxConsumerPriceScale * (distancePct);

                    city.TradeGoodBuyCosts[tradeGood.IdKey] = (long)(costScale * tradeGood.Price);
                    tradeGood.CityBuyCosts[city.IdKey] = city.TradeGoodBuyCosts[tradeGood.IdKey];
                }
            }

            foreach (TradeGood tradeGood in tradeGoods)
            {
                tradeGood.CityBuyCosts.Trim();
            }


            foreach (City city in cities)
            {
                city.TradeGoodBuyCosts.Trim();
            }
        }
    }
}


