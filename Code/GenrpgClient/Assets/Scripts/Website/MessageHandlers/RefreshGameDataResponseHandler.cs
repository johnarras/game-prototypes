using Assets.Scripts.ClientEvents.DataUpdates;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.GameSettings.WebApi.UpdateGameSettings;
using System.Threading;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class UpdateGameDataResponseHandler : BaseClientWebResponseHandler<UpdateGameSettingsResponse>
    {
        protected override void InnerProcess(UpdateGameSettingsResponse result, CancellationToken token)
        {
            if (_gs.ch != null)
            {
                _gs.ch.DataOverrides = result.DataOverrides;
            }
            else
            {
            }
            _gameData.AddData(result.NewSettings);
            _dispatcher.Dispatch(new OnNewGameData());
        }
    }
}


