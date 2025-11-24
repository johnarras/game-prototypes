using Assets.Scripts.ClientEvents.Entities;
using Assets.Scripts.Entities.UI;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.UserEnergy.WebApi;
using Genrpg.Shared.Utils;
using System;

namespace Assets.Scripts.Trader.UI.Currencies
{
    public class CoreCurrencyIcon : EntityIcon
    {

        protected IClientWebService _webService = null;

        public ProgressBar FillBar;
        public EntityTypeWithIdUI EntityUI;

        public override void Init()
        {
            SetEntityData(EntityTypes.CoreCurrency, EntityUI.EntityId, 0);
        }

        public override void SetEntityData(long entityTypeId, long entityId, long quantity, long maxQuantity = 0)
        {
            CoreUserData userData = _gs.ch.Get<CoreUserData>();
            CoreCurrencyStatus status = userData.Currencies.GetStatus(entityId);
            base.SetEntityData(entityTypeId, entityId, status.Curr(), status.Storage());
            FillBar.InitRange(0, status.Storage(), status.Curr());
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
                CoreCurrencyStatus status = userData.Currencies.GetStatus(EntityUI.EntityId);

                double totalSeconds = (status.NextRegenTick - DateTime.UtcNow).TotalSeconds;

                if (totalSeconds < 1)
                {
                    _webService.SendClientUserWebRequest(new UpdateCoreCurrenciesRequest(), GetToken());
                }

                long finalSeconds = (int)Math.Max(0, totalSeconds);

                _uiService.SetText(QuantityText, $"+{status.Regen()} in " + TimeUtils.PrintTime(finalSeconds));
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
