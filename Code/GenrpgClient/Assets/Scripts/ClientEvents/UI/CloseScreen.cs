using OxDb.SharedCore.Client.Interfaces;

namespace OxDb.Client.ClientEvents.UI
{
    public class CloseScreen : IClientEvent
    {
        public long ScreenId { get; set; }

        public CloseScreen(long screenId)
        {
            ScreenId = screenId;
        }
    }
}


