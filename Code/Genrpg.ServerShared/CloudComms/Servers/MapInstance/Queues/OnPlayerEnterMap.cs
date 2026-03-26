namespace Genrpg.ServerShared.CloudComms.Servers.MapInstance.Queues
{
    public class OnPlayerEnterMap : IMapInstanceQueueMessage
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}


