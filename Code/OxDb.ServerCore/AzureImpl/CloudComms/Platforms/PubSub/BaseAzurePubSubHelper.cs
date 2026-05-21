using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using OxDb.ServerCore.CloudComms.Constants;
using OxDb.ServerCore.CloudComms.PubSub.Entities;
using OxDb.ServerCore.Constants;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedGame.Tasks.Services;
using System.Text;

namespace OxDb.ServerCore.AzureImpl.CloudComms.Platforms.PubSub
{
    public abstract class BaseAzurePubSubHelper<M, H> : IDisposable, IAzurePubSubHelper where M : class, IPubSubMessage where H : IPubSubMessageHandler
    {
        public abstract string BaseTopicName();

        protected ServiceBusClient _serviceBusClient = null;
        protected ServiceBusAdministrationClient _adminClient = null;
        protected string _topicName = null;
        protected ServiceBusSender _sender = null;
        protected ServiceBusReceiver _receiver = null;
        protected string _subscriptionName = null;
        protected CancellationToken _token = CancellationToken.None;

        protected ITaskService _taskService = null;

        protected ILogService _logService = null;
        private ITextSerializer _serializer = null;

        protected SetupDictionaryContainer<Type, H> _handlers = new SetupDictionaryContainer<Type, H>();

        public bool IsValidMessage(IPubSubMessage message)
        {
            if (message is M m)
            {
                return true;
            }
            return false;
        }

        public async Task Init(ServiceBusClient client, ServiceBusAdministrationClient adminClient, string serverName, string env, CancellationToken token)
        {
            _serviceBusClient = client;
            _adminClient = adminClient;
            _token = token;
            _topicName = BaseTopicName() + "." + env;
            _subscriptionName = serverName + "." + env;

            Response<bool> response = await _adminClient.TopicExistsAsync(_topicName, token);

            if (!response.Value)
            {
                CreateTopicOptions options = new CreateTopicOptions(_topicName)
                {
                    AutoDeleteOnIdle = CloudCommsConstants.EndpointDeleteTime,
                    DefaultMessageTimeToLive = CloudCommsConstants.MessageDeleteTime,
                };

                await _adminClient.CreateTopicAsync(options);
            }

            _sender = _serviceBusClient.CreateSender(_topicName);

            if (serverName == ServerNames.Editor.ToLower() ||
                serverName.IndexOf("minst") >= 0)
            {
                return;
            }

            _taskService.ForgetTask(RunReceiver(token), true);
        }

        public void SendMessage(IPubSubMessage message)
        {
            if (message is M m)
            {
                PubSubMessageEnvelope envelope = new PubSubMessageEnvelope() { Message = m };

                ServiceBusMessage serviceBusMessage = new ServiceBusMessage(_serializer.SerializeToString(envelope))
                {
                    TimeToLive = TimeSpan.FromSeconds(CloudCommsConstants.MessageTtlSeconds)
                };
                _taskService.ForgetTask(_sender.SendMessageAsync(serviceBusMessage), false);
            }
            else
            {
                _logService.Error("Sent incorrect message of type " + message.GetType().Name + " to topic " + _topicName);
            }
        }

        protected async Task RunReceiver(CancellationToken token)
        {
            try
            {
                Response<bool> response = await _adminClient.SubscriptionExistsAsync(_topicName, _subscriptionName, token);

                if (!response.Value)
                {
                    CreateSubscriptionOptions createOptions = new CreateSubscriptionOptions(_topicName, _subscriptionName)
                    {
                        AutoDeleteOnIdle = CloudCommsConstants.EndpointDeleteTime,
                        DefaultMessageTimeToLive = CloudCommsConstants.MessageDeleteTime,
                    };

                    await _adminClient.CreateSubscriptionAsync(createOptions, token);
                }

                while (_handlers == null)
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                }

                ServiceBusReceiverOptions receiverOptions = new ServiceBusReceiverOptions()
                {
                    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                    PrefetchCount = 50,
                };
                _receiver = _serviceBusClient.CreateReceiver(_topicName, _subscriptionName, receiverOptions);
                _logService.Info("PubSubReceiver: " + _topicName + ":" + _subscriptionName);

                while (true)
                {
                    IReadOnlyList<ServiceBusReceivedMessage> messages = await _receiver.ReceiveMessagesAsync(50, TimeSpan.FromSeconds(1.0f), token);

                    foreach (ServiceBusReceivedMessage message in messages)
                    {
                        PubSubMessageEnvelope envelope = _serializer.Deserialize<PubSubMessageEnvelope>(Encoding.UTF8.GetString(message.Body));

                        if (_handlers == null)
                        {
                            throw new Exception("Cloud PubSub handlers not set up");
                        }

                        if (_handlers.TryGetValue(envelope.Message.GetType(), out H handler))
                        {
                            await handler.HandleMessage(envelope.Message, _token);
                        }
                    }
                }
            }
            catch (OperationCanceledException ce)
            {
                _logService.Info("Shut down PubSub listener " + ce.Message + " " + _topicName + ":" + _subscriptionName);
            }
            catch (Exception e)
            {
                _logService.Exception(e, "PubSubReceiver " + e.Message + " " + _topicName + ":" + _subscriptionName);
            }
            finally
            {
                await _receiver.DisposeAsync();
            }
        }

        public void Dispose()
        {
            _serviceBusClient.DisposeAsync();
            _sender?.DisposeAsync();
        }
    }
}


