using MessagePack;
using System.Collections.Generic;

namespace Assets.Scripts.ClientEvents
{
    public class ShowInfoPanelEvent
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        [Key(0)] public List<string> Lines { get; set; } = new List<string>();
    }
}
