using OxDb.PlayerServer.Managers;
using OxDb.ServerCore.CloudComms.Queues.Entities;
using OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues;

namespace OxDb.PlayerServer.MessageHandlers
{
    public abstract class BasePlayerMessageHandler<T> : IPlayerMessageHandler where T : IPlayerQueueMessage
    {

        protected IPlayerService _playerService;

        protected abstract Task InnerHandleMessage(T message);

        public Type HelperKey => typeof(T);

        public async Task HandleMessage(IQueueMessage message, CancellationToken token)
        {
            await InnerHandleMessage((T)message);
        }
    }
}


