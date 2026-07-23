using OxDb.SharedCore.Client.Interfaces;

namespace OxDb.Client.ClientEvents.UI
{
    public class FinishCloseScreen : IClientEvent
    {
        public long ScreenId { get; set; }

        public FinishCloseScreen(long screenId)
        {
            ScreenId = screenId;
        }
    }
}


