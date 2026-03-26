namespace Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues
{
    public class RemoveMapInstance : IInstanceQueueMessage
    {
        public string FullInstanceId { get; set; }
    }
}


