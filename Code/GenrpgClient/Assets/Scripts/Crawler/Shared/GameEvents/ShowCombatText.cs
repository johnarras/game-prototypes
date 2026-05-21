using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedGame.Crawler.Combat.Constants;

namespace OxDb.SharedGame.Crawler.GameEvents
{

    public class ShowCombatText : IClientEvent
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


