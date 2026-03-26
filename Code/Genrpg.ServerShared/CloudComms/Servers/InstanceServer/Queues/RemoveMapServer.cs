namespace Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues
{
    public class RemoveMapServer : IInstanceQueueMessage
    {
        public string ServerId { get; set; }
    }
}


