using OxDb.Client.Assets.Sprites.Services;
using OxDb.Client.Minigames.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Minigames.Games.Settings;

namespace OxDb.Client.Minigames.Lobby
{
    public class MinigameLobbyIcon : BaseBehaviour
    {

        public GImage Image;
        public GButton Button;

        private IClientMinigameService _minigameService = null;
        private ISpriteService _spriteService = null;

        private MinigameType _mtype = null;

        private MinigameLobbyScreen _screen = null;
        public void SetData(MinigameType mtype, MinigameLobbyScreen screen)
        {
            _mtype = mtype;
            _screen = screen;
            _spriteService.SetEntityIcon(EntityTypes.MinigameType, _mtype.IdKey, Image, GetToken());
            _uiService.SetButton(Button, _screen.GetName(), ClickMinigame);
        }

        private void ClickMinigame()
        {
            if (_mtype == null)
            {
                return;
            }

            _minigameService.ShowMinigame(_mtype.IdKey);
        }
    }
}
