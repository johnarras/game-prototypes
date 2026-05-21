using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.Trader.Maps.Services;
using OxDb.SharedGame.Trader.TradeEconomy.Settings;
using OxDb.SharedGame.Trader.TradeGoods.Settings;
using OxDb.SharedGame.Trader.Travel.Services;

namespace OxDb.DataUtils.Importers.Trader
{
    public abstract class BaseTraderDataImporter<TParent, TChild> : ParentChildImporter<TParent, TChild> where TParent : ParentSettings<TChild>, new() where TChild : ChildSettings, IIdName, new()
    {

        protected ITravelService _travelService = null;
        protected ICaravanService _caravanService = null;
        protected ITraderMapService _traderMapService = null;

        protected override async Task<bool> UpdateAfterImport(EditorGameState gs)
        {

            IReadOnlyList<City> allCities = gs.data.Get<CitySettings>(null).GetData();

            IReadOnlyList<TradeGood> allTradeGoods = gs.data.Get<TradeGoodSettings>(null).GetData();

            SetupTradeGoodProducerCities(gs, allCities, allTradeGoods);

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

        private void SetupTradeGoodProducerCities(EditorGameState gs, IReadOnlyList<City> cities, IReadOnlyList<TradeGood> tradeGoods)
        {

            TradeEconomySettings econ = gs.data.Get<TradeEconomySettings>(null);

            foreach (TradeGood tradeGood in tradeGoods)
            {
                tradeGood.ProducerCities.Clear(); ;
                foreach (City city in cities)
                {
                    if (city.TradeGoodsProduced.Any(x => x.TradeGoodId == tradeGood.IdKey))
                    {
                        tradeGood.ProducerCities.Add(new TradeGoodProducerCity() { CityId = city.IdKey });
                    }
                }

                if (tradeGood.ProducerCities.Count < 1)
                {
                    _logService.Error("No cities produce " + tradeGood.Name);
                }
            }
        }
    }
}


