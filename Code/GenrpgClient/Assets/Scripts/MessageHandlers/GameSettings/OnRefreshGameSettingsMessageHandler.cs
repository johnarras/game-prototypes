using OxDb.Client.Networking.Services;
using OxDb.SharedCore.GameSettings.WebApi.UpdateGameSettings;
using OxDb.SharedGame.GameSettings.Messages;
using OxDb.SharedGame.Purchasing.WebApi.RefreshStores;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.MessageHandlers.GameSettings
{
    public class OnUpdateGameSettingsMessageHandler : BaseClientMapMessageHandler<UpdateGameSettings>
    {
        private IClientWebRequestService _webNetworkService = null;

        protected override async ValueTask InnerProcess(UpdateGameSettings msg, CancellationToken token)
        {
            _webNetworkService.SendMainServerRequest(new UpdateGameSettingsRequest() { CharId = _gs.ch.Id }, token);
            _webNetworkService.SendMainServerRequest(new RefreshStoresRequest() { CharId = _gs.ch.Id }, token);
            await Task.CompletedTask;
        }
    }
}


