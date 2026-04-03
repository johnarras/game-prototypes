using Assets.Scripts.Assets.Sprites.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using Genrpg.Shared.Trader.CurrencySpend.WebApi;
using System;

namespace Assets.Scripts.Trader.Currencies.UI
{
    public class SpendButtonCustomData
    {
        public string Text { get; set; }
        public long CoreCurrencyTypeId { get; set; }
        public long Quantity { get; set; }
        public string Action { get; set; }
    }

    public class SpendCurrencyButton : BaseBehaviour
    {

        protected IClientWebService _webService = null;
        protected ISpriteService _spriteService = null;
        public GImage CurrencyIcon;
        public GText QuantityText;
        public GText CallToActionText;

        public GButton Button;

        private Func<SpendCurrencyRequest,bool> _updateAndValidateRequestFunc;

        public override void Init()
        {
            base.Init();
            _uiService.SetButton(Button, GetType().Name, ClickButton);
        }

        private SpendLocation _loc = null;
        private SpendType _spendType = null;

        private bool _useCurrentCity = true;
        // This does a generic spend using the loc + Type
        public void SetSpendType(SpendLocation loc, SpendType spendType, Func<SpendCurrencyRequest,bool> updateAndValidateRequestFunc)
        {         
            _updateAndValidateRequestFunc = updateAndValidateRequestFunc;
            _loc = loc;
            _spendType = spendType;

            _uiService.SetText(QuantityText, _spendType.SpendQuantity.ToString());
            _spriteService.SetEntityIcon(EntityTypes.CoreCurrency, _spendType.SpendCoreCurrencyTypeId, CurrencyIcon, GetToken());
            _uiService.SetText(CallToActionText, _spendType.Name);
        }

        public void RemoveUseCurrentCityRequirement()
        {
            _useCurrentCity = false;
        }

        private void ClickButton()
        {
            AttemptSpend();
        }

        private void AttemptSpend()
        {
            if (_loc == null || _spendType == null)
            {
                return;
            }
            SpendCurrencyRequest request = new SpendCurrencyRequest()
            {
                SpendLocationId = _loc.IdKey,
                SpendTypeIndex = _spendType.Index,
                SpendCoreCurrencyTypeId = _spendType.SpendCoreCurrencyTypeId,
                SpendQuantity = _spendType.SpendQuantity,
                UseCurrentCity = _useCurrentCity,
            };
           
            if (!_updateAndValidateRequestFunc(request))
            {
                return;
            }

            _webService.SendWebRequest(request, GetToken());
        }
    }
}
