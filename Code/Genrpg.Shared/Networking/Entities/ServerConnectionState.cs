using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Networking.Interfaces;

namespace Genrpg.Shared.Networking.Entities
{
    public class ServerConnectionState
    {
        public IConnection conn { get; set; }
        public Character ch { get; set; }
    }
}


