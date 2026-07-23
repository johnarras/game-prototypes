using OxDb.Client.Login.Messages.Core;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.Attributes.WebApi;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Trader.Stats.ResponseHandlers
{
    public class AddDebuffPlayCountResponseHandler : BaseClientWebResponseHandler<AddDebuffPlayCountResponse>
    {

        private IAttributeService _attributeService = null;
        protected override async ValueTask InnerProcess(AddDebuffPlayCountResponse response, CancellationToken token)
        {
            await _attributeService.AddDebuffDaysPlayed(_gs.ch, response.DebuffDaysAdded);
            await Task.CompletedTask;
        }
    }
}
