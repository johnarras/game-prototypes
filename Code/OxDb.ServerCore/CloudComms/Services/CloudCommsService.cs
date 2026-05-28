using OxDb.ServerCore.AzureImpl.CloudComms.Platforms.PubSub;
using OxDb.ServerCore.CloudComms.PubSub.Constants;
using OxDb.ServerCore.CloudComms.PubSub.Entities;
using OxDb.ServerCore.CloudComms.Queues.Entities;
using OxDb.ServerCore.CloudComms.Queues.Requests.Entities;
using OxDb.ServerCore.Config;
using OxDb.ServerCore.DataStores.Services;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Tasks.Services;
using OxDb.SharedCore.Utils;
using System.Collections.Concurrent;

namespace OxDb.ServerCore.CloudComms.Services
{
    public class CloudCommsService : ICloudCommsService
    {
        const double QueueRequestTimeoutSeconds = 5.0f;

        private IServiceLocator _loc = null!;
        private ILogService _logService = null!;
        private IServerConfig _config = null!;
        private ITextSerializer _textSerializer = null!;
        private IOnlineResourceProvider _resourceProvider = null!;
        private ITaskService _taskService = null;
        private IReflectionService _reflectionService = null;

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
            _env = _config.GetConfigVal(AppConfigKeys.MessagingEnv).ToLower();
            _serverId = _config.GameComponent.ToLower();
            _platformImpl = await _resourceProvider.CreateCloudMessageImpl(_loc, _config, _logService, _textSerializer, _taskService, this, _reflectionService, token);
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

            foreach (Type handlerType in handlers.Keys)
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
                ToServerName = serverId,
                FromServerName = _serverId,
                SendTime = DateTime.UtcNow,
                Request = requestMessage,
                RequestId = HashUtils.NewGuid().ToString(),
            };
            _pendingRequests[pendingQueueRequest.RequestId] = pendingQueueRequest;
            requestMessage.RequestId = pendingQueueRequest.RequestId;
            requestMessage.FromServerName = _platformImpl.GetFullQueueName(_serverId);

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


