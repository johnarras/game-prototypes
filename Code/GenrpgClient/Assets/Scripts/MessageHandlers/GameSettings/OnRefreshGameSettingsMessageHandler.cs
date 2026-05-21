using OxDb.SharedCore.GameSettings.WebApi.UpdateGameSettings;
using OxDb.SharedGame.GameSettings.Messages;
using OxDb.SharedGame.Purchasing.WebApi.RefreshStores;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.GameSettings
{
    public class OnUpdateGameSettingsMessageHandler : BaseClientMapMessageHandler<UpdateGameSettings>
    {
        private IClientWebService _webNetworkService = null;

        protected override async Awaitable InnerProcess(UpdateGameSettings msg, CancellationToken token)
        {
            _webNetworkService.SendWebRequest(new UpdateGameSettingsRequest() { CharId = _gs.ch.Id }, token);
            _webNetworkService.SendWebRequest(new RefreshStoresRequest() { CharId = _gs.ch.Id }, token);
            await Task.CompletedTask;
        }
    }
}


