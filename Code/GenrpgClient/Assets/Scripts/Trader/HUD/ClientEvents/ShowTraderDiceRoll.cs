using System.Collections.Generic;

namespace Assets.Scripts.Trader.HUD.ClientEvents
{
    public class ShowTraderDiceRoll
    {
        public List<long> RolledDistances { get; set; } = new List<long>();
        public long BonusDistance { get; set; }
        public long TotalDistance { get; set; }
    }
}
