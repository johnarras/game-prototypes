using Assets.Scripts.ClientEvents.DataUpdates;
using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedCore.GameSettings.WebApi.UpdateGameSettings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class UpdateGameDataResponseHandler : BaseClientWebResponseHandler<UpdateGameSettingsResponse>
    {
        protected override async Awaitable InnerProcess(UpdateGameSettingsResponse result, CancellationToken token)
        {
            if (_gs.ch != null)
            {
                _gs.ch.AB = result.AB;
            }
            else
            {
            }
            _gameData.AddData(result.NewSettings);
            _dispatcher.Dispatch(new OnNewGameData());
            await Task.CompletedTask;
        }
    }
}


