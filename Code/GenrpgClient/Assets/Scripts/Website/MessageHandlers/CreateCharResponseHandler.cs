using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Characters.WebApi.CreateChar;
using Genrpg.Shared.UI.Constants;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class CreateCharResponseHandler : BaseClientWebResponseHandler<CreateCharResponse>
    {
        protected override async Awaitable InnerProcess(CreateCharResponse result, CancellationToken token)
        {
            _gs.characterStubs = result.AllCharacters;
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.CharacterSelect));
            _dispatcher.Dispatch(new CloseScreen(ScreenNames.CharacterCreate));
            await Task.CompletedTask;
        }
    }
}


