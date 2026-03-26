using Genrpg.Shared.GameSettings.Messages;
using Genrpg.Shared.GameSettings.WebApi.UpdateGameSettings;
using Genrpg.Shared.Purchasing.WebApi.RefreshStores;
using System.Threading;
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
        }
    }
}


