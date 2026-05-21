using OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues;

namespace OxDb.PlayerServer.MessageHandlers
{
    public class PlayerEnterZoneHandler : BasePlayerMessageHandler<PlayerEnterZone>
    {
        protected override async Task InnerHandleMessage(PlayerEnterZone message)
        {
            _playerService.OnPlayerEnterZone(message);
            await Task.CompletedTask;
        }
    }
}


