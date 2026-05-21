using OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues;

namespace OxDb.PlayerServer.MessageHandlers
{
    public class LoginUserHandler : BasePlayerMessageHandler<LoginUser>
    {
        protected override async Task InnerHandleMessage(LoginUser message)
        {
            _playerService.OnLoginUser(message);
            await Task.CompletedTask;
        }
    }
}


