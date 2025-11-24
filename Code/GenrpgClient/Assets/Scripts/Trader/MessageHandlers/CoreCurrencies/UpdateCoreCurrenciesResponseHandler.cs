using Assets.Scripts.ClientEvents.Entities;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.UserEnergy.WebApi;
using System.Threading;

namespace Assets.Scripts.Trader.MessageHandlers.CoreCurrencies
{
    public class UpdateCoreCurrenciesResponseHandler : BaseClientWebResponseHandler<UpdateCoreCurrenciesResponse>
    {
        protected override void InnerProcess(UpdateCoreCurrenciesResponse response, CancellationToken token)
        {
            CoreUserData userData = _gs.ch.Get<CoreUserData>();
            foreach (CoreCurrencyStatus status in response.ChangedStatuses)
            {
                CoreCurrencyStatus currStatus = userData.Currencies.GetStatus(status.CoreCurrencyTypeId);

                if (currStatus != null)
                {
                    currStatus.CopyDataFrom(status);
                    _dispatcher.Dispatch(new ReplaceEntityModel()
                    {
                        EntityTypeId = EntityTypes.CoreCurrency,
                        EntityId = status.CoreCurrencyTypeId
                    });
                }
            }
        }
    }
}
