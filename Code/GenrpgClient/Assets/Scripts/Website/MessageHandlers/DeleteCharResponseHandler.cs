using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Characters.WebApi.DeleteChar;
using Genrpg.Shared.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using System.Threading;
using Assets.Scripts.UI.Entities;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class DeleteCharResponseHandler : BaseClientWebResponseHandler<DeleteCharResponse>
    {
        IScreenService _screenService;
        protected override void InnerProcess(DeleteCharResponse result, CancellationToken token)
        {
            _gs.characterStubs = result.AllCharacters;
            ActiveScreen screen = _screenService.GetScreen(ScreenNames.CharacterSelect);
            if (screen != null && screen.Screen is CharacterSelectScreen charScreen)
            {
                charScreen.SetupCharacterGrid();
            }
        }
    }
}
