using Assets.Scripts.ClientEvents.DataUpdates;
using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedCore.GameSettings.WebApi.UpdateGameSettings;
using OxDb.SharedGame.Characters.PlayerData;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class UpdateGameSettingsResponseHandler : BaseClientWebResponseHandler<UpdateGameSettingsResponse>
    {
        protected override async ValueTask InnerProcess(UpdateGameSettingsResponse result, CancellationToken token)
        {
            if (_gs.ch != null)
            {
                _gs.ch.AB = result.AB;
            }
            else
            {
                _gs.ch = new Character(new CoreCharacter()) { Id = _gs.GameUserId, UserId = _gs.GameUserId, Name = "StubCharacter" };
            }
            _gameData.AddData(result.NewSettings);
            _dispatcher.Dispatch(new OnNewGameData());
            await Task.CompletedTask;
        }
    }
}


