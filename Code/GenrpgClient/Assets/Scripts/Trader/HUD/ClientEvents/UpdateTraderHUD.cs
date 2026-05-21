using OxDb.SharedCore.Client.Interfaces;

namespace Assets.Scripts.Trader.ClientEvents
{
    public class UpdateTraderHUD : IClientEvent
    {
        public bool FullRefresh { get; set; }
    }
}
