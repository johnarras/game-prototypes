namespace Genrpg.ServerShared.CloudComms.Servers.MapInstance.Queues
{
    public class OnPlayerLeaveMap : IMapInstanceQueueMessage
    {
        public string Id { get; set; }
    }
}


