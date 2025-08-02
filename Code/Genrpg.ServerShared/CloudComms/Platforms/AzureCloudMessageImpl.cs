using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.DataStores.Constants;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.Logging.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using System.Collections.Generic;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.Shared.Utils;
using System.Text;
using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.ServerShared.CloudComms.Services;
using System.Collections.Concurrent;
using Genrpg.Shared.Interfaces;
using Genrpg.ServerShared.CloudComms.PubSub.Constants;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin;
using Genrpg.ServerShared.CloudComms.Platforms.PubSub;
using Genrpg.Shared.Tasks.Services;

namespace Genrpg.ServerShared.CloudComms.Platforms
{
    public interface ICloudMessageImpl : IDisposable
    {
        Task Init(IServiceLocator loc, IServerConfig config, ILogService logService, ITextSerializer serializer, ISecretsProvider secretsProvider,
            ITaskService taskService, ICloudCommsService cloudCommsService, CancellationToken token);
        string GetFullQueueName(string serverId);
        void SendQueueMessages(string serverId, List<IQueueMessage> cloudMessages);
        void SendPubSubMessage(IPubSubMessage message);
    }
    public class AzureCloudMessageImpl : ICloudMessageImpl
    {
        private IServiceLocator _loc = null;
        private ISecretsProvider _secretsProvider = null;
        private ICloudCommsService _cloudCommsService = null;
        private ILogService _logService = null;
        private IServerConfig _config = null;
        private ITextSerializer _serializer = null;
        private ITaskService _taskService = null;

        private string _env;
        private string _serverId;       
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
        }

        public async Task Init(IServiceLocator loc, IServerConfig config, ILogService logService, ITextSerializer serializer, ISecretsProvider secretsProvider, ITaskService taskService, ICloudCommsService cloudCommsService, CancellationToken token)
        {
            _cloudCommsService = cloudCommsService;
            _logService = logService;
            _secretsProvider = secretsProvider;
            _serializer = serializer;
            _taskService= taskService;  
            _loc = loc;
            _token = token;
            _config = config;
            _env = _config.MessagingEnv.ToLower();
            _serverId = _config.ServerId.ToLower();
            string queuePubSubConnectionString = await _secretsProvider.GetSecret(ConnectionNames.QueuePubSub);
            _serviceBusClient = new ServiceBusClient(queuePubSubConnectionString);
            _adminClient = new ServiceBusAdministrationClient(queuePubSubConnectionString);
            _queueName = GetFullQueueName(_serverId);

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

            try
            {
                if (!await _adminClient.QueueExistsAsync(_queueName, token))
                {
                    await _adminClient.CreateQueueAsync(options, token);
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "CloudMessageSetup");
            }

            _logService.Message("Created Queue " + _queueName);

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



                        logService.Message("Received message: " + _queueName);
                        await _cloudCommsService.ReceiveQueueMessages(envelope, token);
                    }
                }
            }
            catch (OperationCanceledException ce)
            {
                _logService.Info("Shutting down cloud listener for " + ce.Message + " " + _serverId);
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
                ToServerId = serverId,
                FromServerId = _serverId,
                Messages = cloudMessages,
            };

            if (!_queueSenders.TryGetValue(envelope.ToServerId, out ServiceBusSender sender))
            {
                sender = _serviceBusClient.CreateSender(envelope.ToServerId);
                _queueSenders[envelope.ToServerId] = sender;
            }

            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(_serializer.SerializeToString(envelope));
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
            _pubSubHelpers[PubSubTopicNames.Admin] = (AzureAdminPubSubHelper)(await ReflectionUtils.CreateInstanceFromType(_loc, typeof(AzureAdminPubSubHelper), token));

            foreach (IAzurePubSubHelper helper in _pubSubHelpers.Values)
            {
                await helper.Init(_serviceBusClient, _adminClient, _serverId, _env, token);
            }
        }

        #endregion
    }
}
