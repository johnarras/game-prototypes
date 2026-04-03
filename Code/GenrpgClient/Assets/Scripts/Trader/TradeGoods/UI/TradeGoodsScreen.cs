using Assets.Scripts.Trader.Currencies.UI;
using Assets.Scripts.Trader.UI.Icons;
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.TradeGoods.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.UI.TradeGoods
{
    /// <summary>
    ///  This is a spend location, but also lets you sell.
    /// </summary>
    public class TradeGoodsScreen : SpendLocationScreen
    {
        protected ITextService _textService = null;

        public GameObject MyTradeGoodsAnchor;

        public TradeGoodIcon TradeGoodIconPrefab;

        protected List<TradeGoodIcon> _tradeGoodIcons = new List<TradeGoodIcon>();

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await base.OnStartOpen(data, token);

            _dispatcher.AddListener<RemoveTradeGoodFromCaravanResponse>(OnRemoveTradeGoodFromCaravan, GetToken());
        }

        protected void OnRemoveTradeGoodFromCaravan(RemoveTradeGoodFromCaravanResponse response)
        {
            _awaitableService.ForgetAwaitable(ShowPurchaseItems());
        }

        protected override async Awaitable ShowPurchaseItems()
        {
            await base.ShowPurchaseItems();

            CoreData coreData = _gs.ch.Get<CoreData>();

            CaravanData caravanData = _gs.ch.Get<CaravanData>();

            bool overloaded = coreData.Vars[TraderVars.InventoryUsed] > coreData.Vars[TraderVars.MaxInventory];
            _uiService.SetText(Message,
                _textService.HighlightText(
                "Inventory: " + coreData.Vars[TraderVars.InventoryUsed] + "/" + coreData.Vars[TraderVars.MaxInventory],
                overloaded ? TextColors.ColorRed : TextColors.ColorWhite));

            await ShowMyItems();

        }

        protected async Awaitable ShowMyItems()
        {
            CaravanData caravanData = _gs.ch.Get<CaravanData>();

            List<CaravanTradeGood> currGoods = caravanData.TradeGoods.OrderBy(x => x.UniqueId).ToList();


            List<CaravanTradeGood> addGoods = new List<CaravanTradeGood>();
            List<TradeGoodIcon> removeIcons = new List<TradeGoodIcon>();

            foreach (CaravanTradeGood item in currGoods)
            {
                if (!_tradeGoodIcons.Any(x=>x.UniqueId == item.UniqueId))
                {
                    addGoods.Add(item); 
                }
            }
            foreach (TradeGoodIcon icon in _tradeGoodIcons)
            {
                if (!currGoods.Any(x => x.UniqueId == icon.UniqueId))
                {
                    removeIcons.Add(icon);
                }
            }

            foreach (TradeGoodIcon icon in removeIcons)
            {
                _tradeGoodIcons.Remove(icon);
                _clientEntityService.Destroy(icon);
            }

            foreach (CaravanTradeGood tg in addGoods)
            {
                TradeGoodIcon icon = _clientEntityService.FullInstantiate(TradeGoodIconPrefab);

                _clientEntityService.AddToParent(icon, MyTradeGoodsAnchor);

                await icon.SetData(EntityTypes.TradeGood, tg.TradeGoodId, 1, tg.UniqueId);

                _tradeGoodIcons.Add(icon);
            }

            _tradeGoodIcons = _tradeGoodIcons.OrderBy(x => x.UniqueId).ToList();

            _clientEntityService.ReorderSiblings(_tradeGoodIcons);
        }
    }
}


