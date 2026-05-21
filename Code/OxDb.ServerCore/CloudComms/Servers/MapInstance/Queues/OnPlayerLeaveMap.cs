namespace OxDb.ServerCore.CloudComms.Servers.MapInstance.Queues
{
    public class OnPlayerLeaveMap : IMapInstanceQueueMessage
    {
        public string Id { get; set; }
    }
}


