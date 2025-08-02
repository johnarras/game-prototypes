using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.Shared.Utils;
using Genrpg.ServerShared.CloudComms.Queues.Requests.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.ServerShared.CloudComms.Platforms;
using System.Text;
using System.Collections.Concurrent;
using Genrpg.ServerShared.CloudComms.PubSub.Constants;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin;
using Genrpg.ServerShared.OnlineResources.Interfaces;
using Genrpg.Shared.Tasks.Services;

namespace Genrpg.ServerShared.CloudComms.Services
{
    public class CloudCommsService : ICloudCommsService
    {
        const double QueueRequestTimeoutSeconds = 5.0f;

        private IServiceLocator _loc = null!;
        private ILogService _logService = null!;
        private ISecretsProvider _secretsProvider = null!;
        private IServerConfig _config = null!;
        private ITextSerializer _textSerializer = null!;
        private IOnlineResourceProvider _resourceProvider = null!;
        private ITaskService _taskService = null;

        private string _serverId = null;
        private string _env = null;
        private CancellationToken _token;


        private ICloudMessageImpl _platformImpl;

        private List<string> _pubSubTopics = new List<string>() { PubSubTopicNames.Admin };

        private Dictionary<string, Type> _pubSubTypes = new Dictionary<string, Type>()
        {
            { PubSubTopicNames.Admin, typeof(AzureAdminPubSubHelper) },
        };

        private Dictionary<Type, IQueueMessageHandler> _queueHandlers;
        private ConcurrentDictionary<string, PendingQueueRequest> _pendingRequests = new ConcurrentDictionary<string, PendingQueueRequest>();

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _env = _config.MessagingEnv.ToLower();
            _serverId = _config.ServerId.ToLower();
            _platformImpl = await _resourceProvider.CreateCloudMessageImpl(_loc, _config, _logService, _textSerializer, _secretsProvider, _taskService, this, token);
        }

        #region Queues

        public string GetFullServerName(string serverId)
        {
            return _platformImpl.GetFullQueueName(serverId);
        }

        public void SendQueueMessage(string serverId, IQueueMessage cloudMessage)
        {
            SendQueueMessages(serverId, new List<IQueueMessage>() { cloudMessage });
        }

        public void SendQueueMessages(string serverId, List<IQueueMessage> cloudMessages)
        {
            _platformImpl.SendQueueMessages(serverId, cloudMessages);
        }

        public void SetQueueMessageHandlers<H>(Dictionary<Type, H> handlers) where H : IQueueMessageHandler
        {
            Dictionary<Type, IQueueMessageHandler> newDict = new Dictionary<Type, IQueueMessageHandler>();

            foreach (var handlerType in handlers.Keys)
            {
                newDict[handlerType] = handlers[handlerType];
            }
            _queueHandlers = newDict;
        }

        public async Task ReceiveQueueMessages(QueueMessageEnvelope envelope, CancellationToken token)
        {   
            foreach (IQueueMessage queueMessage in envelope.Messages)
            {
                if (queueMessage is IResponseQueueMessage responseQueueMessage &&
                    _pendingRequests.TryRemove(responseQueueMessage.RequestId, out PendingQueueRequest pendingRequest))
                {
                    pendingRequest.Response = responseQueueMessage;
                }
                else if (_queueHandlers.TryGetValue(queueMessage.GetType(), out IQueueMessageHandler handler))
                {
                    await handler.HandleMessage(queueMessage, token);
                }
                else
                {
                    _logService.Info("Missing queue handler for type " + queueMessage.GetType().Name + " in " + _serverId);
                }
            }
        }

        public async Task<TResponse> SendResponseMessageAsync<TResponse>(string serverId, IRequestQueueMessage requestMessage) where TResponse : IResponseQueueMessage
        {
            PendingQueueRequest pendingQueueRequest = new PendingQueueRequest()
            {
                ToServerId = serverId,
                FromServerId = _serverId,
                SendTime = DateTime.UtcNow,
                Request = requestMessage,
                RequestId = HashUtils.NewUUId().ToString(),
            };
            _pendingRequests[pendingQueueRequest.RequestId] = pendingQueueRequest;
            requestMessage.RequestId = pendingQueueRequest.RequestId;
            requestMessage.FromServerId = _platformImpl.GetFullQueueName(_serverId);

            SendQueueMessages(serverId, new List<IQueueMessage>() { requestMessage });

            do
            {
                await Task.Delay(1, _token).ConfigureAwait(false);

                if (pendingQueueRequest.Response != null)
                {
                    return (TResponse)pendingQueueRequest.Response;
                }
            }
            while (pendingQueueRequest.Response == null &&
            (DateTime.UtcNow - pendingQueueRequest.SendTime).TotalSeconds < QueueRequestTimeoutSeconds);

            if (_pendingRequests.TryRemove(pendingQueueRequest.RequestId, out PendingQueueRequest orphanedRequest))
            {
                return (TResponse)orphanedRequest.Response;
            }

            return default;
        }


        public void SendResponseMessageWithHandler<TResponse>(string serverId, IRequestQueueMessage requestMessage, Action<TResponse> responseHandler) where TResponse : IResponseQueueMessage
        {
            _taskService.ForgetTask(SendAsyncRequestWithHandler(serverId, requestMessage, responseHandler), false);
        }

        private async Task SendAsyncRequestWithHandler<TResponse>(string serverId, IRequestQueueMessage requestMessage, Action<TResponse> responseHandler) where TResponse : IResponseQueueMessage
        {
            TResponse response = await SendResponseMessageAsync<TResponse>(serverId, requestMessage);
            responseHandler?.Invoke(response);
        }
        #endregion

        #region PubSub

        // This is mostly platform implementation
        public void SendPubSubMessage(IPubSubMessage pubSubMessage)
        {
            _platformImpl.SendPubSubMessage(pubSubMessage);
        }

        #endregion

    }
}
