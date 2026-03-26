using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Platforms.PubSub
{
    public interface IAzurePubSubHelper
    {
        Task Init(ServiceBusClient client, ServiceBusAdministrationClient adminClient, string env, string serverId, CancellationToken token);
        void SendMessage(IPubSubMessage message);
        bool IsValidMessage(IPubSubMessage message);

    }
}


