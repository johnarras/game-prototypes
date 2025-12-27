using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Characters.WebApi.CreateChar;
using Genrpg.Shared.UI.Constants;
using System.Threading;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class CreateCharResponseHandler : BaseClientWebResponseHandler<CreateCharResponse>
    {
        protected override void InnerProcess(CreateCharResponse result, CancellationToken token)
        {
            _gs.characterStubs = result.AllCharacters;
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.CharacterSelect));
            _dispatcher.Dispatch(new CloseScreen(ScreenNames.CharacterCreate));
        }
    }
}


