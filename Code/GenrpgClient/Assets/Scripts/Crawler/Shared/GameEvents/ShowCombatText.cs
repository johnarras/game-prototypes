using Genrpg.Shared.Crawler.Combat.Constants;

namespace Genrpg.Shared.Crawler.GameEvents
{

    public class ShowCombatText
    {
        public string CasterUnitId { get; set; }
        public string CasterGroupId { get; set; }
        public string TargetUnitId { get; set; }
        public string TargetGroupId { get; set; }
        public string Text { get; set; }
        public ECombatTextTypes TextType { get; set; }
        public long ElementTypeId { get; set; }
    }
}
