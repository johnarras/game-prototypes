namespace Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues
{
    public class PlayerEnterZone : IPlayerQueueMessage
    {
        public string Id { get; set; }
        public long ZoneId { get; set; }
    }
}


