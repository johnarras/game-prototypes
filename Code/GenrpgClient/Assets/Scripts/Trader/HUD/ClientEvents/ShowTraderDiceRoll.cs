using OxDb.SharedCore.Client.Interfaces;
using System.Collections.Generic;

namespace Assets.Scripts.Trader.HUD.ClientEvents
{
    public class ShowTraderDiceRoll : IClientEvent
    {
        public List<int> RolledDistances { get; set; } = new List<int>();
        public int BonusDistance { get; set; }
        public int TotalDistance { get; set; }
    }
}
