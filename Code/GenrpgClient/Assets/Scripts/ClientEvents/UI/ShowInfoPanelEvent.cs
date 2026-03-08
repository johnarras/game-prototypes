using Genrpg.Shared.Client.Interfaces;
using MessagePack;
using System.Collections.Generic;

namespace Assets.Scripts.ClientEvents
{
    public class ShowInfoPanelEvent : IClientEvent
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public List<string> Lines { get; set; } = new List<string>();
    }
}


