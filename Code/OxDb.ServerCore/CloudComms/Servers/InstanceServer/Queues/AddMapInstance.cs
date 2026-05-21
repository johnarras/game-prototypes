using OxDb.SharedGame.Networking.Constants;

namespace OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues
{
    public class AddMapInstance : IInstanceQueueMessage
    {
        public string ServerName { get; set; }
        public string MapId { get; set; }
        public string InstanceId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public int Size { get; set; }
        public EMapApiSerializers SerializerType { get; set; }
    }
}


