using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedGame.Trader.CaravanMembers.WebApi;
using OxDb.SharedGame.Trader.Caravans.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.Caravans.MessageHandlers
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
