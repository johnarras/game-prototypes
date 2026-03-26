using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;

namespace Genrpg.PlayerServer.MessageHandlers
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


