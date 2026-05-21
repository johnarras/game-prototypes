namespace OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues
{
    public class RemoveMapServer : IInstanceQueueMessage
    {
        public string ServerName { get; set; }
    }
}


