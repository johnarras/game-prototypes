using OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues;

namespace OxDb.PlayerServer.MessageHandlers
{
    public class WhoListRequestHandler : BasePlayerMessageHandler<WhoListRequest>
    {
        protected override async Task InnerHandleMessage(WhoListRequest message)
        {
            _playerService.OnGetWhoList(message);
            await Task.CompletedTask;
        }
    }
}


