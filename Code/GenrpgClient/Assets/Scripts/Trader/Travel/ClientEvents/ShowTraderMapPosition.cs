using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedGame.Trader.Caravans.Entities;

namespace Assets.Scripts.Trader.Travel.ClientEvents
{
    public class ShowTraderMapPosition : IClientEvent
    {
        public CaravanPosition Pos;
        public double DistanceGone = -1;
        public bool FullRefresh;
    }
}
