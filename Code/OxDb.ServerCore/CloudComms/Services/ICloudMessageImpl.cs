using OxDb.ServerCore.CloudComms.PubSub.Entities;
using OxDb.ServerCore.CloudComms.Queues.Entities;
using OxDb.ServerCore.Config;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Tasks.Services;
using OxDb.SharedCore.Utils;

namespace OxDb.ServerCore.CloudComms.Services
{
    public interface ICloudMessageImpl : IDisposable
    {
        Task Init(IServiceLocator loc, IServerConfig config, ILogService logService, ITextSerializer serializer,
            ITaskService taskService, ICloudCommsService cloudCommsService, IReflectionService reflectionService, CancellationToken token);
        string GetFullQueueName(string serverId);
        void SendQueueMessages(string serverId, List<IQueueMessage> cloudMessages);
        void SendPubSubMessage(IPubSubMessage message);
    }
}
