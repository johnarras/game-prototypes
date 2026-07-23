using OxDb.Client.Minigames.Services;
using OxDb.SharedGame.Minigames.Games.Settings;

namespace OxDb.Client.Minigames.UI
{
    public class BaseMinigameUI : BaseBehaviour
    {

        protected IClientMinigameService _clientMinigameService = null;

        public GText HeaderText;
        public GButton WinButton;
        public GButton LoseButton;


        protected MinigameType _mtype;
        public virtual void SetData(MinigameType mtype)
        {
            _uiService.SetText(HeaderText, mtype.Name);
            _uiService.SetButton(WinButton, GetName(), ClickWin);
            _uiService.SetButton(LoseButton, GetName(), ClickLose);
            _mtype = mtype;
        }

        private void ClickWin()
        {
            _clientMinigameService.ClickWin(_mtype?.IdKey ?? 0);
        }

        private void ClickLose()
        {
            _clientMinigameService.ClickLose(_mtype?.IdKey ?? 0);

        }
    }
}
