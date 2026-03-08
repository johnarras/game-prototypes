
using Genrpg.Shared.Client.Interfaces;

namespace Assets.Scripts.Crawler.ClientEvents.CombatEvents
{
    public class ShowCombatBolt : IClientEvent
    {

        public string CasterId { get; set; }
        public string TargetId { get; set; }
        public long ElementTypeId { get; set; }
        public float Seconds { get; set; }
        public double SizeScale { get; set; }
    }
}


