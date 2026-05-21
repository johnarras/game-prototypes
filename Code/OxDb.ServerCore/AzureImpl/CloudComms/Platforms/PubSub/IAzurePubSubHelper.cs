using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using OxDb.ServerCore.CloudComms.PubSub.Entities;

namespace OxDb.ServerCore.AzureImpl.CloudComms.Platforms.PubSub
{
    public interface IAzurePubSubHelper
    {
        Task Init(ServiceBusClient client, ServiceBusAdministrationClient adminClient, string env, string serverId, CancellationToken token);
        void SendMessage(IPubSubMessage message);
        bool IsValidMessage(IPubSubMessage message);

    }
}


