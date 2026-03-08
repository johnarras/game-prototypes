using Genrpg.Shared.Client.Interfaces;

namespace ClientEvents
{
    public class ShowLoadingPercentEvent : IClientEvent
    {
        public int CurrStep;
        public int TotalSteps;
    }
}


