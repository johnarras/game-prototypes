namespace OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues
{
    public class RemoveMapInstance : IInstanceQueueMessage
    {
        public string FullInstanceId { get; set; }
    }
}


