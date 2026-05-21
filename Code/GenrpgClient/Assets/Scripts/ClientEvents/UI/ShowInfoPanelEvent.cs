using Assets.Scripts.Info.UI;
using OxDb.SharedCore.Client.Interfaces;
using System.Collections.Generic;

namespace Assets.Scripts.ClientEvents
{
    public class ShowInfoPanelArgs : IClientEvent
    {
        public string Header { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public List<string> Lines { get; set; } = new List<string>();
        public EInfoPanelDisplayReason Reason { get; set; } = EInfoPanelDisplayReason.Pointer;
    }
}


