using OxDb.SharedGame.Inventory.Messages;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Services;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Items
{
    public class OnUpdateItemHandler : BaseClientMapMessageHandler<OnUpdateItem>
    {
        protected ISharedItemService _sharedItemService = null;
        protected override async Awaitable InnerProcess(OnUpdateItem msg, CancellationToken token)
        {

            if (msg.UnitId != _gs.ch.Id)
            {
                return;
            }

            InventoryData inventory = _gs.ch.Get<InventoryData>();

            Item item = inventory.GetItem(msg.Item.Id);

            if (item != null)
            {
                _sharedItemService.CopyStatsFrom(msg.Item, item);
            }
            await Task.CompletedTask;
        }
    }
}


