using OxDb.SharedCore.Client.Interfaces;

namespace OxDb.Client.Trader.ClientEvents
{
    public class UpdateTraderHUD : IClientEvent
    {
        public bool FullRefresh { get; set; }
    }
}
