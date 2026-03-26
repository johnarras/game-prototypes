using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.UI.Entities;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Characters.WebApi.DeleteChar;
using Genrpg.Shared.UI.Constants;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class DeleteCharResponseHandler : BaseClientWebResponseHandler<DeleteCharResponse>
    {
        IScreenService _screenService = null;
        protected override async Awaitable InnerProcess(DeleteCharResponse result, CancellationToken token)
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


