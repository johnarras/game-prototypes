using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.Servers.WebServer;

namespace Genrpg.RequestServer.RequestHandlers
{
    public abstract class BaseQueueMessageHandler<T> : IQueueMessageHandler where T : IWebsiteQueueMessage
    {

        protected abstract Task InnerHandleMessage(T message);

        public Type HelperKey => typeof(T);

        public async Task HandleMessage(IQueueMessage message, CancellationToken token)
        {
            await InnerHandleMessage((T)message);
        }
    }
}


