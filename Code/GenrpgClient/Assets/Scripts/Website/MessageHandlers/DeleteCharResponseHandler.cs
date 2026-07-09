using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.UI.Entities;
using OxDb.SharedGame.Characters.WebApi.DeleteChar;
using OxDb.SharedGame.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class DeleteCharResponseHandler : BaseClientWebResponseHandler<DeleteCharResponse>
    {
        IScreenService _screenService = null;
        protected override async ValueTask InnerProcess(DeleteCharResponse result, CancellationToken token)
        {
            _gs.characterStubs = result.AllCharacters;
            ActiveScreen screen = _screenService.GetScreen(ScreenNames.CharacterSelect);
            if (screen != null && screen.Screen is CharacterSelectScreen charScreen)
            {
                charScreen.SetupCharacterGrid();
            }
            await Task.CompletedTask;
        }
    }
}


