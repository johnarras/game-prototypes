using OxDb.ServerCore.CloudComms.Queues.Entities;

namespace OxDb.MonsterServer.MessageHandlers
{
    public abstract class BaseMonsterMessageHandler<T> : IMonsterMessageHandler where T : IQueueMessage
    {
        protected abstract Task InnerHandleMessage(T message);

        public Type HelperKey => typeof(T);

        public async Task HandleMessage(IQueueMessage message, CancellationToken token)
        {
            await InnerHandleMessage((T)message);
        }
    }
}


