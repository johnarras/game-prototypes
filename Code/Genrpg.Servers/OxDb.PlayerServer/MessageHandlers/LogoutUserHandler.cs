using OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues;

namespace OxDb.PlayerServer.MessageHandlers
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


