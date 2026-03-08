using Genrpg.Shared.Client.Interfaces;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.GameEvents
{
    public class ShowPartyMinimap : IClientEvent
    {
        public PartyData Party { get; set; }
        public bool PartyArrowOnly { get; set; }
    }
}


