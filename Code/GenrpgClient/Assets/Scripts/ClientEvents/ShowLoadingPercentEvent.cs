using OxDb.SharedCore.Client.Interfaces;

namespace ClientEvents
{
    public class ShowLoadingPercentEvent : IClientEvent
    {
        public int CurrStep;
        public int TotalSteps;
    }
}


