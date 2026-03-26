namespace Genrpg.PlayerServer.Entities
{
    public class OnlineCharacter
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string MapId { get; set; }
        public string MapInstanceId { get; set; }
        public long ZoneId { get; set; }
        public long GuildId { get; set; }
        public long Level { get; set; }
    }
}


