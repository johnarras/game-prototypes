using OxDb.ServerCore.CloudComms.Queues.Entities;
using OxDb.ServerCore.CloudComms.Servers.WebServer;

namespace OxDb.RequestServer.Queues
{
    public abstract class BaseQueueMessageHandler<T> : IWebsiteQueueMessageHandler where T : IWebsiteQueueMessage
    {

        protected abstract Task InnerHandleMessage(T message);

        public Type HelperKey => typeof(T);

        public async Task HandleMessage(IQueueMessage message, CancellationToken token)
        {
            await InnerHandleMessage((T)message);
        }
    }
}


