using Assets.Scripts.Entities.UI;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Services;
using OxDb.SharedGame.UserEnergy.WebApi;
using System;
using UnityEngine;

namespace Assets.Scripts.Trader.UI.Currencies
{
    public class CoreCurrencyIcon : EntityIcon
    {

        protected IClientWebService _webService = null;
        protected ICoreCurrencyService _coreCurrencyService = null;

        public ProgressBar FillBar;
        public EntityTypeWithIdUI EntityUI;

        public override void Init()
        {
            SetEntityData(EntityTypes.CoreCurrency, EntityUI.EntityId, 0);
        }

        protected override GameObject GetDooberHitPosition()
        {
            return FillBar.FrontBarRHS;
        }

        public override void SetEntityData(long entityTypeId, long entityId, long quantity, long maxQuantity = 0)
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            long currQuantity = coreData.Currencies[entityId];

            long storage = _coreCurrencyService.GetStorage(_gs.ch, entityId).Result;
            base.SetEntityData(entityTypeId, entityId, currQuantity, storage);
            FillBar.InitRange(0, currQuantity, storage);
        }

        protected override void UpdateQuantity()
        {
            if (_currQuantity > 0 || _targetQuantity > 0)
            {
                base.UpdateQuantity();
                FillBar.SetValue(_currQuantity);
            }
            else
            {
                CoreData coreData = _gs.ch.Get<CoreData>();

                double totalSeconds = (coreData.NextHourlyUpdate - DateTime.UtcNow).TotalSeconds;

                if (totalSeconds < 1)
                {
                    _webService.SendWebRequest(new UpdateCoreCurrenciesRequest(), GetToken());
                }

                long finalSeconds = (int)Math.Max(0, totalSeconds);

                long regen = _coreCurrencyService.GetRegen(_gs.ch, _entityId).Result;
                _uiService.SetText(QuantityText, $"+{regen} in " + TimeUtils.PrintTime(finalSeconds));
            }
        }
    }
}


