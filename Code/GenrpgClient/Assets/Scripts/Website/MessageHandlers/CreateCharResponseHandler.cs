using OxDb.Client.ClientEvents.UI;
using OxDb.Client.Login.Messages.Core;
using OxDb.SharedGame.Characters.WebApi.CreateChar;
using OxDb.SharedGame.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Website.MessageHandlers
{
    public class CreateCharResponseHandler : BaseClientWebResponseHandler<CreateCharResponse>
    {
        protected override async ValueTask InnerProcess(CreateCharResponse result, CancellationToken token)
        {
            _gs.characterStubs = result.AllCharacters;
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.CharacterSelect));
            _dispatcher.Dispatch(new CloseScreen(ScreenNames.CharacterCreate));
            await Task.CompletedTask;
        }
    }
}


