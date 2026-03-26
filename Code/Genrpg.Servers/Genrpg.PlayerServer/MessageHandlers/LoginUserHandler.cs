using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;

namespace Genrpg.PlayerServer.MessageHandlers
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


