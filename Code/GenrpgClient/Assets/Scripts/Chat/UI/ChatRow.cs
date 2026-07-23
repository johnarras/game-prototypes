using OxDb.SharedGame.Chat.Messages;
using OxDb.SharedGame.Chat.Settings;

namespace OxDb.Client.UI.Chat
{
    public class ChatRow : BaseBehaviour
    {
        public GText Text;
        public GImage Background;

        private OnChatMessage _message;

        public void Init(OnChatMessage message)
        {
            _message = message;

            ChatType chatType = _gameData.Get<ChatSettings>(_gs.ch).Get(message.ChatTypeId);

            _uiService.SetText(Text, "[" + chatType?.Name + "] " + message.SenderName + ": " + message.Message);
        }

        public void InitTextOnly(string text)
        {
            _uiService.SetText(Text, text);
        }
    }
}


