using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public class LogoutUserHandler : BasePlayerMessageHandler<LogoutUser>
    {
        protected override async Task InnerHandleMessage(LogoutUser message)
        {
            _playerService.OnLogoutUser(message);
            await Task.CompletedTask;
        }
    }
}


