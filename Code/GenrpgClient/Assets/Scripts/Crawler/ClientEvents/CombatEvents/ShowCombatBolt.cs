
namespace Assets.Scripts.Crawler.ClientEvents.CombatEvents
{
    public class ShowCombatBolt
    {

        public string CasterId { get; set; }
        public string TargetId { get; set; }
        public long ElementTypeId { get; set; }
        public float Seconds { get; set; }
        public double SizeScale { get; set; }
    }
}
