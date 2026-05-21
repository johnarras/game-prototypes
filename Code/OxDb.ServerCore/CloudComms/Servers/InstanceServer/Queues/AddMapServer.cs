namespace OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues
{
    public class AddMapServer : IInstanceQueueMessage
    {
        public string ServerName { get; set; }
    }
}


