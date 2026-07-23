using OxDb.Client.FloatingText.ClientEvents;
using OxDb.Client.Login.Messages.Core;
using OxDb.SharedGame.Trader.CaravanMembers.WebApi;
using OxDb.SharedGame.Trader.Caravans.Services;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Trader.Caravans.MessageHandlers
{
    public class UpdateCaravanMembersResponseHandler : BaseClientWebResponseHandler<UpdateCaravanMembersResponse>
    {
        protected ICaravanService _caravanService = null;
        protected override async ValueTask InnerProcess(UpdateCaravanMembersResponse response, CancellationToken token)
        {
            if (!response.Success)
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage, EFloatingTextArt.Error));
            }
            else
            {
                _dispatcher.Dispatch(response);
            }
            await Task.CompletedTask;
        }
    }
}
