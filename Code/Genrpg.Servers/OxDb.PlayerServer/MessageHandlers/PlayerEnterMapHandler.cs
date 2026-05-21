using OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues;

namespace OxDb.PlayerServer.MessageHandlers
{
    public class PlayerEnterMapHandler : BasePlayerMessageHandler<PlayerEnterMap>
    {
        protected override async Task InnerHandleMessage(PlayerEnterMap message)
        {
            _playerService.OnPlayerEnterMap(message);
            await Task.CompletedTask;
        }
    }
}


