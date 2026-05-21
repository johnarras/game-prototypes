using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Networking.Interfaces;

namespace OxDb.SharedGame.Networking.Entities
{
    public class ServerConnectionState
    {
        public IConnection conn { get; set; }
        public Character ch { get; set; }
    }
}


