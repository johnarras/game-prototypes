using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.Characters.PlayerData;

namespace Genrpg.Shared.Networking.Entities
{
    // MessagePackIgnore
    public class ServerConnectionState
    {
        public IConnection conn { get; set; }
        public Character ch { get; set; }
    }
}
