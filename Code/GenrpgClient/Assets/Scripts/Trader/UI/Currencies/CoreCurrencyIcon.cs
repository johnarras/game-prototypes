using Assets.Scripts.ClientEvents.Entities;
using Assets.Scripts.Entities.UI;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Services;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.UserEnergy.WebApi;
using Genrpg.Shared.Utils;
using System;

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

        public override void SetEntityData(long entityTypeId, long entityId, long quantity, long maxQuantity = 0)
        {
            CoreUserData userData = _gs.ch.Get<CoreUserData>();
            TraderStatData statData = _gs.ch.Get<TraderStatData>();
            long currQuantity = userData.Currencies.Get(entityId);
            long storage = _coreCurrencyService.GetStorage(entityId, userData, statData);
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
                CoreUserData userData = _gs.ch.Get<CoreUserData>();

                double totalSeconds = (userData.NextHourlyUpdate - DateTime.UtcNow).TotalSeconds;

                if (totalSeconds < 1)
                {
                    _webService.SendClientUserWebRequest(new UpdateCoreCurrenciesRequest(), GetToken());
                }

                long finalSeconds = (int)Math.Max(0, totalSeconds);

                long regen = _coreCurrencyService.GetRegen(_entityId, userData, _gs.ch.Get<TraderStatData>());
                _uiService.SetText(QuantityText, $"+{regen} in " + TimeUtils.PrintTime(finalSeconds));
            }
        }

        protected override void OnReplaceEntityModel(ReplaceEntityModel model)
        {
            if (model.EntityTypeId != EntityTypes.CoreCurrency || model.EntityId != EntityUI.EntityId)
            {
                return;
            }
            Init();
        }
    }
}
