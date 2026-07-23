using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedGame.Trader.Caravans.Entities;

namespace OxDb.Client.Trader.Travel.ClientEvents
{
    public class ShowTraderMapPosition : IClientEvent
    {
        public CaravanPosition Pos;
        public double DistanceGone = -1;
        public bool FullRefresh;
    }
}
