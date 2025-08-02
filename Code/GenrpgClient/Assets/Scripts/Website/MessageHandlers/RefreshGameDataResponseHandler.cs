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
            else if (_gs.user != null)
            {
                _gs.user.DataOverrides = result.DataOverrides;
            }
            _gameData.AddData(result.NewSettings);
        }
    }
}
