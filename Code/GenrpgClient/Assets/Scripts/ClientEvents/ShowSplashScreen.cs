using OxDb.SharedCore.Client.Interfaces;

namespace Assets.Scripts.ClientEvents
{
    public class ShowSplashScreen : IClientEvent
    {
        public string Message { get; set; }
        public bool ShowResetButton { get; set; }
    }
}
