namespace Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues
{
    public class AddMapServer : IInstanceQueueMessage
    {
        public string ServerId { get; set; }
    }
}


