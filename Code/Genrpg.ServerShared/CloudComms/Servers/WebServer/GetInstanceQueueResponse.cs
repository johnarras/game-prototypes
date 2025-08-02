using Genrpg.ServerShared.CloudComms.Queues.Requests.Entities;
using Genrpg.Shared.Networking.Constants;

namespace Genrpg.ServerShared.CloudComms.Servers.WebServer
{
    public class GetInstanceQueueResponse : ILoginQueueMessage, IResponseQueueMessage
    {
        public string RequestId { get; set; }
        public string ErrorText { get; set; }

        public string InstanceId { get; set; }
        public string Host { get; set; }
        public long Port { get; set; }
        public EMapApiSerializers SerializerType { get; set; }
    }
}
