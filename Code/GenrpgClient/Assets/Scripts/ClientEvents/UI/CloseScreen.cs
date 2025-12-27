namespace Assets.Scripts.ClientEvents.UI
{
    public class CloseScreen
    {
        public long ScreenId { get; set; }

        public CloseScreen(long screenId)
        {
            ScreenId = screenId;
        }
    }
}


