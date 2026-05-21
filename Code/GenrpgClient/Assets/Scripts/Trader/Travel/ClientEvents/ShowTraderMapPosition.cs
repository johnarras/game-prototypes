using OxDb.SharedCore.Client.Interfaces;

namespace Assets.Scripts.Trader.Travel.ClientEvents
{
    public class ShowTraderMapPosition : IClientEvent
    {
        public float X;
        public float Y;
        public bool UpdateAngle;
        public bool FullRefresh;
    }
}
