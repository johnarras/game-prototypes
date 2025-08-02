
namespace Assets.Scripts.Crawler.ClientEvents.CombatEvents
{
    public class ShowCombatBolt
    {
        public string CasterId { get; set; }
        public string TargetId { get; set; }
        public long ElementTypeId { get; set; }
        public double Seconds { get; set; }
    }
}
