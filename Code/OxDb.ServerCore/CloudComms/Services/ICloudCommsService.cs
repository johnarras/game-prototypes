using OxDb.ServerCore.CloudComms.PubSub.Entities;
using OxDb.ServerCore.CloudComms.Queues.Entities;
using OxDb.ServerCore.CloudComms.Queues.Requests.Entities;
using OxDb.SharedCore.Interfaces;

namespace OxDb.ServerCore.CloudComms.Services
{
    public interface ICloudCommsService : IInitializable
    {
        string GetFullServerName(string serverId);

        void SetQueueMessageHandlers<H>(Dictionary<Type, H> handlers) where H : IQueueMessageHandler;
        void SendQueueMessage(string serverId, IQueueMessage cloudMessage);
        void SendQueueMessages(string serverId, List<IQueueMessage> cloudMessages);

        Task<TResponse> SendResponseMessageAsync<TResponse>(string serverId, IRequestQueueMessage requestMessage) where TResponse : IResponseQueueMessage;

        void SendResponseMessageWithHandler<TResponse>(string serverId, IRequestQueueMessage requestMessage,
            Action<TResponse> responseHandler) where TResponse : IResponseQueueMessage;

        Task ReceiveQueueMessages(QueueMessageEnvelope envelope, CancellationToken token);


        void SendPubSubMessage(IPubSubMessage message);
    }
}


