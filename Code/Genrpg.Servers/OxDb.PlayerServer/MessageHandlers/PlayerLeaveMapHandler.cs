using OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues;

namespace OxDb.PlayerServer.MessageHandlers
{
    public class PlayerLeaveMapHandler : BasePlayerMessageHandler<PlayerLeaveMap>
    {
        protected override async Task InnerHandleMessage(PlayerLeaveMap message)
        {
            _playerService.OnPlayerLeaveMap(message);
            await Task.CompletedTask;
        }
    }
}


