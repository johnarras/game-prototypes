using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Shipments.PlayerData;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Trader.Shipments.Services
{

    public interface IShipmentService : IInitializable
    {
        ValueTask AddTradeGoodToCaravan(IUnitDataLookup lookup, long tradeGoodId, long uniqueId);
        ValueTask RemoveTradeGoodFromCaravan(IUnitDataLookup lookup, long tradeGoodId, long uniqueId);
    }


    public class ShipmentService : IShipmentService
    {

        protected ICaravanService _caravanService = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }
        public async ValueTask AddTradeGoodToCaravan(IUnitDataLookup lookup, long tradeGoodId, long uniqueId)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();
            ShipmentData shipmentData = await lookup.GetAsync<ShipmentData>();

            CaravanPosition pos = await _caravanService.GetPosition(lookup);

            if (pos.GetCurrentCity() != null)
            {

                foreach (Shipment shipment in shipmentData.Shipments)
                {
                    foreach (ShipmentTask shipmentTask in shipment.Tasks)
                    {
                        if (shipmentTask.TradeGoodId == tradeGoodId && shipmentTask.UniqueId == 0 &&
                            shipmentTask.CityId == pos.GetCurrentCity().IdKey)
                        {
                            shipmentTask.UniqueId = uniqueId;
                            // Send update message to player
                        }
                    }
                }
            }

        }

        public async ValueTask RemoveTradeGoodFromCaravan(IUnitDataLookup lookup, long tradeGoodId, long uniqueId)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();
            ShipmentData shipmentData = await lookup.GetAsync<ShipmentData>();

            foreach (Shipment shipment in shipmentData.Shipments)
            {
                foreach (ShipmentTask shipmentTask in shipment.Tasks)
                {
                    if (shipmentTask.TradeGoodId == tradeGoodId && shipmentTask.UniqueId == 0)
                    {
                        shipmentTask.UniqueId = 0;
                        // Send update message to player
                    }
                }
            }
        }
    }
}
