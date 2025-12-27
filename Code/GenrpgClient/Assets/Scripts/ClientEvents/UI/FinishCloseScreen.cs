namespace Assets.Scripts.ClientEvents.UI
{
    public class FinishCloseScreen
    {
        public long ScreenId { get; set; }

        public FinishCloseScreen(long screenId)
        {
            ScreenId = screenId;
        }
    }
}


