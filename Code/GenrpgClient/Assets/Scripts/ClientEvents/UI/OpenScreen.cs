using OxDb.SharedCore.Client.Interfaces;

namespace Assets.Scripts.ClientEvents.UI
{
    public class OpenScreen : IClientEvent
    {
        public long ScreenId { get; set; }
        public object Data { get; set; }

        public OpenScreen(long screenId, object data = null)
        {
            ScreenId = screenId;
            Data = data;
        }
    }
}


