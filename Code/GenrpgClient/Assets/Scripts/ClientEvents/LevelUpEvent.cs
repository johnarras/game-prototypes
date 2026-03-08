using Genrpg.Shared.Client.Interfaces;

namespace ClientEvents
{
    public class LevelUpEvent : IClientEvent
    {
        public string Id { get; set; }
        public long Level { get; set; }
        public long Exp { get; set; }
    }
}


