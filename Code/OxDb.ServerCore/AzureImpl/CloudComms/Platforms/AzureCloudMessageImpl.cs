using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using OxDb.ServerCore.AzureImpl.CloudComms.Platforms.PubSub;
using OxDb.ServerCore.AzureImpl.DataStores.Constants;
using OxDb.ServerCore.CloudComms.Constants;
using OxDb.ServerCore.CloudComms.PubSub.Constants;
using OxDb.ServerCore.CloudComms.PubSub.Entities;
using OxDb.ServerCore.CloudComms.Queues.Entities;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.Config;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Tasks.Services;
using System.Collections.Concurrent;
using System.Text;

namespace OxDb.ServerCore.AzureImpl.CloudComms.Platforms
{
    public class AzureCloudMessageImpl : ICloudMessageImpl
    {
        private IServiceLocator _loc = null;
        private ICloudCommsService _cloudCommsService = null;
        private ILogService _logService = null;
        private IServerConfig _config = null;
        private ITextSerializer _serializer = null;
        private ITaskService _taskService = null;
        private IReflectionService _reflectionService = null;

        private string _env;
        private string _serverName;
        private CancellationToken _token = CancellationToken.None;

        // Core ServiceBus
        private ServiceBusClient _serviceBusClient = null;
        private ServiceBusAdministrationClient _adminClient = null;


        private string _queueName;
        private bool _didSetupQueue;
        private ServiceBusReceiver _queueReceiver;
        private ConcurrentDictionary<string, ServiceBusSender> _queueSenders = new ConcurrentDictionary<string, ServiceBusSender>();



        Dictionary<string, IAzurePubSubHelper> _pubSubHelpers = new Dictionary<string, IAzurePubSubHelper>();


        public void Dispose()
        {

            _serviceBusClient?.DisposeAsync();

            foreach (ServiceBusSender sender in _queueSenders.Values)
            {
                sender.DisposeAsync();
            }

            _queueSenders.Clear();

        }

        public async Task Init(IServiceLocator loc, IServerConfig config, ILogService logService, ITextSerializer serializer, ITaskService taskService, ICloudCommsService cloudCommsService, IReflectionService reflectionService, CancellationToken token)
        {
            _cloudCommsService = cloudCommsService;
            _logService = logService;
            _serializer = serializer;
            _taskService = taskService;
            _reflectionService = reflectionService;
            _loc = loc;
            _token = token;
            _config = config;
            _env = _config.GetConfigVal(AppConfigKeys.MessagingEnv).ToLower();
            _serverName = _config.GameComponent.ToLower();
            string queuePubSubConnectionString = _config.GetConfigVal(ConnectionNames.QueuePubSub);
            _serviceBusClient = new ServiceBusClient(queuePubSubConnectionString);
            _adminClient = new ServiceBusAdministrationClient(queuePubSubConnectionString);
            _queueName = GetFullQueueName(_serverName);

            await SetupQueue(token);

            await SetupPubSub(_loc, token);
        }

        #region Queues
        protected string QueueSuffix()
        {
            return ("." + _env).ToLower();
        }

        public string GetFullQueueName(string serverId)
        {
            return (serverId + QueueSuffix()).ToLower();
        }

        private async Task SetupQueue(CancellationToken token)
        {

            CreateQueueOptions options = new CreateQueueOptions(_queueName)
            {
                AutoDeleteOnIdle = CloudCommsConstants.EndpointDeleteTime,
                DefaultMessageTimeToLive = CloudCommsConstants.MessageDeleteTime,
            };

            if (!await _adminClient.QueueExistsAsync(_queueName, token))
            {
                await _adminClient.CreateQueueAsync(options, token);
            }

            _logService.Info("Created Queue " + _queueName);

            _taskService.ForgetTask(RunQueueReceiver(_logService, _token), true);

            _didSetupQueue = true;
        }

        private async Task RunQueueReceiver(ILogService logService, CancellationToken token)
        {
            ServiceBusReceiverOptions options = new ServiceBusReceiverOptions()
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                PrefetchCount = 50,
            };

            _queueReceiver = _serviceBusClient.CreateReceiver(_queueName, options);

            try
            {
                while (true)
                {
                    IReadOnlyList<ServiceBusReceivedMessage> messages = await _queueReceiver.ReceiveMessagesAsync(50, TimeSpan.FromSeconds(1.0f), token);

                    foreach (ServiceBusReceivedMessage message in messages)
                    {
                        QueueMessageEnvelope envelope = _serializer.Deserialize<QueueMessageEnvelope>(Encoding.UTF8.GetString(message.Body));



                        logService.Info("Received message: " + _queueName);
                        await _cloudCommsService.ReceiveQueueMessages(envelope, token);
                    }
                }
            }
            catch (OperationCanceledException ce)
            {
                _logService.Info("Shutting down cloud listener for " + ce.Message + " " + _serverName);
            }
        }

        public void SendQueueMessages(string serverId, List<IQueueMessage> cloudMessages)
        {
            if (!_didSetupQueue)
            {
                return;
            }

            if (serverId.IndexOf(QueueSuffix()) < 0)
            {
                serverId = GetFullQueueName(serverId);
            }

            QueueMessageEnvelope envelope = new QueueMessageEnvelope()
            {
                ToServerName = serverId,
                FromServerName = _serverName,
                Messages = cloudMessages,
            };

            if (!_queueSenders.TryGetValue(envelope.ToServerName, out ServiceBusSender sender))
            {
                sender = _serviceBusClient.CreateSender(envelope.ToServerName);
                _queueSenders[envelope.ToServerName] = sender;
            }

            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(_serializer.SerializeToString(envelope))
            {
                TimeToLive = TimeSpan.FromSeconds(CloudCommsConstants.MessageTtlSeconds)
            };
            _taskService.ForgetTask(sender.SendMessageAsync(serviceBusMessage), false);

        }

        public void SendPubSubMessage(IPubSubMessage message)
        {
            foreach (IAzurePubSubHelper helper in _pubSubHelpers.Values)
            {
                if (helper.IsValidMessage(message))
                {
                    helper.SendMessage(message);
                    return;
                }
            }
        }
        #endregion


        #region PubSub

        private async Task SetupPubSub(IServiceLocator loc, CancellationToken token)
        {
            _pubSubHelpers[PubSubTopicNames.Admin] = (AzureAdminPubSubHelper)(await _reflectionService.CreateInstanceFromType(_loc, typeof(AzureAdminPubSubHelper), token));

            foreach (IAzurePubSubHelper helper in _pubSubHelpers.Values)
            {
                await helper.Init(_serviceBusClient, _adminClient, _serverName, _env, token);
            }
        }

        #endregion
    }
}


