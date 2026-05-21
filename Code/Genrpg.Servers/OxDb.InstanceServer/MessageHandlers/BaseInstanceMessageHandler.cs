using OxDb.InstanceServer.Managers;
using OxDb.ServerCore.CloudComms.Queues.Entities;
using OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues;
using OxDb.SharedCore.Logalytics.Interfaces;

namespace OxDb.InstanceServer.MessageHandlers
{
    public abstract class BaseInstanceMessageHandler<T> : IInstanceMessageHandler where T : IInstanceQueueMessage
    {

        protected IInstanceManagerService _instanceManagerService = null;

        protected ILogService _logService = null;

        protected abstract Task InnerHandleMessage(T message);

        public Type HelperKey => typeof(T);

        public async Task HandleMessage(IQueueMessage message, CancellationToken token)
        {
            await InnerHandleMessage((T)message);
        }
    }
}


