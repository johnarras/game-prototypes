namespace Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues
{
    public class PlayerEnterMap : IPlayerQueueMessage
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string MapId { get; set; }
        public string InstanceId { get; set; }
        public long Level { get; set; }
    }
}


