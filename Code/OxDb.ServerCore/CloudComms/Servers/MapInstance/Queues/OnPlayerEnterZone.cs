namespace OxDb.ServerCore.CloudComms.Servers.MapInstance.Queues
{
    public class OnPlayerEnterZone : IMapInstanceQueueMessage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long ZoneId { get; set; }
    }
}


