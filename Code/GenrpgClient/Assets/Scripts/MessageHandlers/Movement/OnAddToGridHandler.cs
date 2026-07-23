using OxDb.SharedGame.Movement.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.MessageHandlers.Movement
{
    public class OnAddToGridHandler : BaseClientMapMessageHandler<OnAddToGrid>
    {
        protected override async ValueTask InnerProcess(OnAddToGrid msg, CancellationToken token)
        {
            _objectManager.OnServerAddtoGrid(msg);
            await Task.CompletedTask;
        }
    }
}


