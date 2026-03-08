using Assets.Scripts.Entities.UI;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.UserEnergy.WebApi;
using Genrpg.Shared.Utils;
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
            TraderStatData statData = _gs.ch.Get<TraderStatData>();
            long currQuantity = coreData.Currencies[entityId];
            long storage = _coreCurrencyService.GetStorage(entityId, coreData, statData);
            base.SetEntityData(entityTypeId, entityId, currQuantity, storage);
            FillBar.InitRange(0, storage, currQuantity);
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
                    _webService.SendClientUserWebRequest(new UpdateCoreCurrenciesRequest(), GetToken());
                }

                long finalSeconds = (int)Math.Max(0, totalSeconds);

                long regen = _coreCurrencyService.GetRegen(_entityId, coreData, _gs.ch.Get<TraderStatData>());
                _uiService.SetText(QuantityText, $"+{regen} in " + TimeUtils.PrintTime(finalSeconds));
            }
        }
    }
}


