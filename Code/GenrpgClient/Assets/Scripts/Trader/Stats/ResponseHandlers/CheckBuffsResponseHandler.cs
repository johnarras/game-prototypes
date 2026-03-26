using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.Attributes.WebApi;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Trader.Stats.ResponseHandlers
{
    public class CheckBuffsResponseHandler : BaseClientWebResponseHandler<CheckBuffsResponse>
    {
        private IAttributeService _attributeService = null;
        protected override async Awaitable InnerProcess(CheckBuffsResponse response, CancellationToken token)
        {
            await _attributeService.UpdateBuffsAndDebuffs(_gs.ch);

        }
    }
}
