using OxDb.SharedCore.Client.Interfaces;

namespace OxDb.Client.FloatingText.ClientEvents
{
    public enum EFloatingTextArt
    {
        Message = 0,
        Error = 1,
    }

    public class ShowFloatingText : IClientEvent
    {

        public ShowFloatingText(string text, EFloatingTextArt art = EFloatingTextArt.Message)
        {
            Text = text;
            Art = art;
        }

        public string Text;
        public EFloatingTextArt Art;
    }

}
