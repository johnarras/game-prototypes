using OxDb.SharedCore.Client.Interfaces;

namespace Assets.Scripts.ClientEvents.UI
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


