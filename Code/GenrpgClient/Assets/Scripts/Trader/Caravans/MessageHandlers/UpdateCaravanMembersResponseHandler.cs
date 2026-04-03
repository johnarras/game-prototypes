using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.Trader.ClientEvents;
using Genrpg.Shared.Trader.CaravanMembers.WebApi;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Trader.Caravans.MessageHandlers
{
    public class UpdateCaravanMembersResponseHandler : BaseClientWebResponseHandler<UpdateCaravanMembersResponse>
    {
        protected ICaravanService _caravanService = null;
        protected override async Awaitable InnerProcess(UpdateCaravanMembersResponse response, CancellationToken token)
        {
            if (!response.Success)
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage, EFloatingTextArt.Error));
            }
            else
            {
                _dispatcher.Dispatch(response);
            }
        }
    }
}
