using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedGame.PlayMultiplier.WebApi;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.MessageHandlers.PlayMult
{
    public class SetPlayMultResponseHandler : BaseClientWebResponseHandler<SetPlayMultResponse>
    {
        protected override async Awaitable InnerProcess(SetPlayMultResponse response, CancellationToken token)
        {
            await Task.CompletedTask;
            _dispatcher.Dispatch(response);
        }
    }
}


