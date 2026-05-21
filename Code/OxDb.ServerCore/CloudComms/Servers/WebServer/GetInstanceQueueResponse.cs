using OxDb.ServerCore.CloudComms.Queues.Requests.Entities;
using OxDb.SharedGame.Networking.Constants;

namespace OxDb.ServerCore.CloudComms.Servers.WebServer
{
    public class GetInstanceQueueResponse : IWebsiteQueueMessage, IResponseQueueMessage
    {
        public string RequestId { get; set; }
        public string ErrorText { get; set; }

        public string InstanceId { get; set; }
        public string Host { get; set; }
        public long Port { get; set; }
        public EMapApiSerializers SerializerType { get; set; }
    }
}


