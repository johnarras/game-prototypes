using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.Queues.Requests.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Services
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
