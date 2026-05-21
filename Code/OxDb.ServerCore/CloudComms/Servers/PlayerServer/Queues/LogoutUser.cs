namespace OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues
{
    public class LogoutUser : IPlayerQueueMessage
    {
        public string Id { get; set; }
    }
}


