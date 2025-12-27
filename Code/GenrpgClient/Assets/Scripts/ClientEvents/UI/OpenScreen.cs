namespace Assets.Scripts.ClientEvents.UI
{
    public class OpenScreen
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


