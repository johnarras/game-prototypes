using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.Attributes.WebApi;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.Stats.ResponseHandlers
{
    public class CheckBuffsResponseHandler : BaseClientWebResponseHandler<CheckBuffsResponse>
    {
        private IAttributeService _attributeService = null;
        protected override async ValueTask InnerProcess(CheckBuffsResponse response, CancellationToken token)
        {
            await _attributeService.UpdateBuffsAndDebuffs(_gs.ch);

        }
    }
}
