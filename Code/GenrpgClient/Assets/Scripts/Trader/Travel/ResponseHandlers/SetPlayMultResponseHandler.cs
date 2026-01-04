using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.PlayMultiplier.WebApi;
using System.Threading;

namespace Assets.Scripts.Trader.MessageHandlers.PlayMult
{
    public class SetPlayMultResponseHandler : BaseClientWebResponseHandler<SetPlayMultResponse>
    {
        protected override void InnerProcess(SetPlayMultResponse response, CancellationToken token)
        {
            _dispatcher.Dispatch(response);
        }
    }
}


