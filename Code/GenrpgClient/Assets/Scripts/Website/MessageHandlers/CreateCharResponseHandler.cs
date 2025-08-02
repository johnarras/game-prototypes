using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Characters.WebApi.CreateChar;
using Genrpg.Shared.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using System.Threading;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class CreateCharResponseHandler : BaseClientWebResponseHandler<CreateCharResponse>
    {
        private IScreenService _screenService;
        protected override void InnerProcess(CreateCharResponse result, CancellationToken token)
        {
            _gs.characterStubs = result.AllCharacters;
            _screenService.Open(ScreenNames.CharacterSelect);
            _screenService.Close(ScreenNames.CharacterCreate);
        }
    }
}
