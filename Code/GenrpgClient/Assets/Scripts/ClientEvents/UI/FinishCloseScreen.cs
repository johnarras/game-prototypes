using Genrpg.Shared.Client.Interfaces;

namespace Assets.Scripts.ClientEvents.UI
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


