using Assets.Scripts.Entities.UI;
using Assets.Scripts.UI.Entities;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.TradeGoods.Services;
using Genrpg.Shared.Trader.TradeGoods.WebApi;
using UnityEngine;

namespace Assets.Scripts.Trader.UI.Icons
{
    public class TradeGoodIcon : RowEntityIcon
    {

        protected ICaravanService _caravanService = null;
        protected ITradeGoodService _tradeGoodService = null;
        protected IClientWebService _webService = null;

        public long UniqueId => _uniqueId;
        private long _uniqueId { get; set; }


        public RowEntityIcon SellButtonIcon;
        public GButton SellButton;
        public GButton DropButton;
        public GText DescText;

        

        private long _sellPrice = 0;

        public async Awaitable SetData(long entityTypeId, long entityId, long quantity, long uniqueId)
        {
            base.SetEntityData(entityTypeId, entityId, quantity);

            _uniqueId = uniqueId;

            CoreData coreData = _gs.ch.Get<CoreData>();

            CaravanPosition pos = _caravanService.GetPosition(coreData);

            City city = pos.GetCurrentCity();

            _sellPrice = await _tradeGoodService.GetSellValueAtPosition(_gs.ch, entityId, pos.CurrX, pos.CurrY);
            _clientEntityService.SetActive(SellButton, _sellPrice > 0);
            _clientEntityService.SetActive(DropButton, _sellPrice == 0);


            if (_sellPrice > 0)
            {
                SellButtonIcon.SetEntityData(EntityTypes.CoreCurrency, CoreCurrencyTypes.Coins, _sellPrice);
            }

            _uiService.SetButton(SellButton, GetName(), ClickSellItem);
            _uiService.SetButton(DropButton, GetName(), ClickDropItem);

        }

        /// <summary>
        ///  These two are the same now since dropping and selling both get rid of the item. Idk, maybe separate later.
        /// </summary>
        private void ClickSellItem()
        {
            RemoveTradeGoodFromCaravanRequest request = new RemoveTradeGoodFromCaravanRequest()
            {
                UniqueId = _uniqueId,
                SellValue = _sellPrice,
                TradeGoodId = _entityId,
            };
            _webService.SendWebRequest(request, GetToken());
        }

        /// <summary>
        ///  These two are the same now since dropping and selling both get rid of the item. Idk, maybe separate later.
        /// </summary>
        private void ClickDropItem()
        {
            RemoveTradeGoodFromCaravanRequest request = new RemoveTradeGoodFromCaravanRequest()
            {
                UniqueId = _uniqueId,
                SellValue = _sellPrice,
                TradeGoodId = _entityId,
            };
            _webService.SendWebRequest(request, GetToken());

        }
    }
}


